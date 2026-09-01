"""Минимальный WebDAV-сервер под требования Super Productivity.

Поддерживает: OPTIONS, HEAD, GET, PUT (с If-Match / If-None-Match), DELETE,
MKCOL, PROPFIND (Depth 0/1). Ответы совместимы с webdav-провайдером SP —
использует strong ETag (SHA-256 контента).
"""
from __future__ import annotations
import hashlib
import os
import shutil
from datetime import datetime, timezone
from email.utils import formatdate
from html import escape
from pathlib import Path
from typing import Optional
from urllib.parse import quote, unquote

from fastapi import APIRouter, Request, Response
from sqlmodel import Session, select

from .auth import authenticate_basic
from .config import settings
from .db import engine
from .models import User, Workspace, WorkspaceMember


router = APIRouter()

BASE = Path(settings.webdav_root).resolve()
DAV_METHODS = "OPTIONS, HEAD, GET, PUT, DELETE, MKCOL, PROPFIND, COPY, MOVE"


# ---------- helpers ----------
def _auth_response() -> Response:
    return Response(
        status_code=401,
        headers={"WWW-Authenticate": 'Basic realm="Super Productivity Sync"'},
        content=b"Unauthorized",
    )


def _user_and_workspace(request: Request, ws_slug: str) -> tuple[Optional[User], Optional[Workspace]]:
    with Session(engine) as session:
        user = authenticate_basic(session, request)
        if not user:
            return None, None
        ws = session.exec(select(Workspace).where(Workspace.slug == ws_slug)).first()
        if not ws:
            return user, None
        member = session.exec(
            select(WorkspaceMember).where(
                WorkspaceMember.workspace_id == ws.id,
                WorkspaceMember.user_id == user.id,
            )
        ).first()
        if not member:
            return user, None
        return user, ws


def _safe_path(ws_slug: str, subpath: str) -> Optional[Path]:
    ws_root = (BASE / ws_slug).resolve()
    ws_root.mkdir(parents=True, exist_ok=True)
    target = (ws_root / subpath.lstrip("/")).resolve()
    try:
        target.relative_to(ws_root)
    except ValueError:
        return None
    return target


def _etag(p: Path) -> str:
    h = hashlib.sha256()
    with p.open("rb") as f:
        for chunk in iter(lambda: f.read(1 << 16), b""):
            h.update(chunk)
    return f'"{h.hexdigest()}"'


def _rfc1123(ts: float) -> str:
    return formatdate(ts, usegmt=True)


def _iso_creation(ts: float) -> str:
    return datetime.fromtimestamp(ts, tz=timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")


def _href(ws_slug: str, subpath: str, is_dir: bool) -> str:
    p = quote("/webdav/" + ws_slug + "/" + subpath.lstrip("/"))
    if is_dir and not p.endswith("/"):
        p += "/"
    return p


def _propfind_xml(entries: list[dict]) -> str:
    parts = ['<?xml version="1.0" encoding="utf-8"?>',
             '<D:multistatus xmlns:D="DAV:">']
    for e in entries:
        parts.append("<D:response>")
        parts.append(f"<D:href>{escape(e['href'])}</D:href>")
        parts.append("<D:propstat><D:prop>")
        if e["is_dir"]:
            parts.append("<D:resourcetype><D:collection/></D:resourcetype>")
            parts.append("<D:getcontentlength>0</D:getcontentlength>")
        else:
            parts.append("<D:resourcetype/>")
            parts.append(f"<D:getcontentlength>{e['size']}</D:getcontentlength>")
            parts.append(f"<D:getcontenttype>{escape(e.get('mime','application/octet-stream'))}</D:getcontenttype>")
            parts.append(f"<D:getetag>{escape(e['etag'])}</D:getetag>")
        parts.append(f"<D:getlastmodified>{escape(e['lastmod'])}</D:getlastmodified>")
        parts.append(f"<D:creationdate>{escape(e['created'])}</D:creationdate>")
        parts.append("<D:displayname>" + escape(e["name"]) + "</D:displayname>")
        parts.append("</D:prop><D:status>HTTP/1.1 200 OK</D:status></D:propstat>")
        parts.append("</D:response>")
    parts.append("</D:multistatus>")
    return "".join(parts)


def _entry(ws_slug: str, root: Path, target: Path) -> dict:
    is_dir = target.is_dir()
    st = target.stat()
    rel = target.relative_to(root).as_posix()
    if rel == ".":
        rel = ""
    entry = {
        "href": _href(ws_slug, rel, is_dir),
        "name": target.name or ws_slug,
        "is_dir": is_dir,
        "size": 0 if is_dir else st.st_size,
        "lastmod": _rfc1123(st.st_mtime),
        "created": _iso_creation(st.st_ctime),
    }
    if not is_dir:
        entry["etag"] = _etag(target)
        entry["mime"] = "application/json" if target.suffix.lower() == ".json" else "application/octet-stream"
    return entry


# ---------- endpoints ----------
def _register(path: str, methods: list[str]):
    def deco(fn):
        router.add_api_route(path, fn, methods=methods)
        return fn
    return deco


@router.options("/webdav/{ws_slug}")
@router.options("/webdav/{ws_slug}/{rest:path}")
async def dav_options(ws_slug: str, rest: str = ""):
    return Response(
        status_code=200,
        headers={
            "DAV": "1, 2",
            "MS-Author-Via": "DAV",
            "Allow": DAV_METHODS,
            "Accept-Ranges": "bytes",
        },
    )


async def _handle_get(request: Request, ws_slug: str, rest: str, head: bool) -> Response:
    user, ws = _user_and_workspace(request, ws_slug)
    if not user:
        return _auth_response()
    if not ws:
        return Response(status_code=404)
    target = _safe_path(ws_slug, rest)
    if target is None or not target.exists():
        return Response(status_code=404)
    if target.is_dir():
        return Response(status_code=405, content=b"Directory")
    et = _etag(target)
    inm = request.headers.get("if-none-match", "")
    if inm and (inm == "*" or et in [t.strip() for t in inm.split(",")]):
        return Response(status_code=304, headers={"ETag": et})
    st = target.stat()
    headers = {
        "ETag": et,
        "Last-Modified": _rfc1123(st.st_mtime),
        "Content-Length": str(st.st_size),
        "Content-Type": "application/json" if target.suffix.lower() == ".json" else "application/octet-stream",
        "Accept-Ranges": "bytes",
    }
    if head:
        return Response(status_code=200, headers=headers)
    data = target.read_bytes()
    return Response(status_code=200, content=data, headers=headers)


@router.get("/webdav/{ws_slug}/{rest:path}")
async def dav_get(request: Request, ws_slug: str, rest: str):
    return await _handle_get(request, ws_slug, rest, head=False)


@router.head("/webdav/{ws_slug}/{rest:path}")
async def dav_head(request: Request, ws_slug: str, rest: str):
    return await _handle_get(request, ws_slug, rest, head=True)


@router.put("/webdav/{ws_slug}/{rest:path}")
async def dav_put(request: Request, ws_slug: str, rest: str):
    user, ws = _user_and_workspace(request, ws_slug)
    if not user:
        return _auth_response()
    if not ws:
        return Response(status_code=404)
    target = _safe_path(ws_slug, rest)
    if target is None:
        return Response(status_code=403)
    if target.exists() and target.is_dir():
        return Response(status_code=409, content=b"Is a directory")

    # If-Match / If-None-Match (CAS)
    if_match = request.headers.get("if-match", "").strip()
    if_none_match = request.headers.get("if-none-match", "").strip()
    current_etag = _etag(target) if target.exists() else None
    if if_none_match == "*" and target.exists():
        return Response(status_code=412, content=b"Precondition Failed: exists")
    if if_match:
        if not target.exists():
            return Response(status_code=412, content=b"Precondition Failed: absent")
        wanted = [t.strip() for t in if_match.split(",")]
        if current_etag not in wanted and "*" not in wanted:
            return Response(
                status_code=412,
                headers={"ETag": current_etag or ""},
                content=b"Precondition Failed: etag mismatch",
            )

    target.parent.mkdir(parents=True, exist_ok=True)
    body = await request.body()
    tmp = target.with_suffix(target.suffix + ".tmp")
    tmp.write_bytes(body)
    os.replace(tmp, target)
    new_etag = _etag(target)
    return Response(
        status_code=201 if current_etag is None else 204,
        headers={
            "ETag": new_etag,
            "Last-Modified": _rfc1123(target.stat().st_mtime),
        },
    )


@router.delete("/webdav/{ws_slug}/{rest:path}")
async def dav_delete(request: Request, ws_slug: str, rest: str):
    user, ws = _user_and_workspace(request, ws_slug)
    if not user:
        return _auth_response()
    if not ws:
        return Response(status_code=404)
    target = _safe_path(ws_slug, rest)
    if target is None or not target.exists():
        return Response(status_code=404)
    if target.is_dir():
        shutil.rmtree(target)
    else:
        target.unlink()
    return Response(status_code=204)


@_register("/webdav/{ws_slug}/{rest:path}", ["MKCOL"])
async def dav_mkcol(request: Request, ws_slug: str, rest: str):
    user, ws = _user_and_workspace(request, ws_slug)
    if not user:
        return _auth_response()
    if not ws:
        return Response(status_code=404)
    target = _safe_path(ws_slug, rest)
    if target is None:
        return Response(status_code=403)
    if target.exists():
        return Response(status_code=405)
    if not target.parent.exists():
        return Response(status_code=409)
    target.mkdir(parents=False)
    return Response(status_code=201)


async def _propfind(request: Request, ws_slug: str, rest: str) -> Response:
    user, ws = _user_and_workspace(request, ws_slug)
    if not user:
        return _auth_response()
    if not ws:
        return Response(status_code=404)
    target = _safe_path(ws_slug, rest)
    if target is None:
        return Response(status_code=403)
    if not target.exists():
        return Response(status_code=404)

    depth = request.headers.get("depth", "1").lower()
    ws_root = (BASE / ws_slug).resolve()
    entries = [_entry(ws_slug, ws_root, target)]
    if target.is_dir() and depth in ("1", "infinity"):
        try:
            for child in sorted(target.iterdir()):
                entries.append(_entry(ws_slug, ws_root, child))
        except PermissionError:
            pass
    xml = _propfind_xml(entries)
    return Response(
        status_code=207,
        content=xml,
        media_type="application/xml; charset=utf-8",
    )


@_register("/webdav/{ws_slug}", ["PROPFIND"])
async def dav_propfind_root(request: Request, ws_slug: str):
    return await _propfind(request, ws_slug, "")


@_register("/webdav/{ws_slug}/{rest:path}", ["PROPFIND"])
async def dav_propfind(request: Request, ws_slug: str, rest: str):
    return await _propfind(request, ws_slug, rest)
