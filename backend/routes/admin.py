from __future__ import annotations
import re
from typing import List
from fastapi import APIRouter, Depends, HTTPException
from pydantic import BaseModel, Field
from sqlmodel import Session, select

from ..auth import get_current_user, hash_password, require_admin
from ..db import get_session
from ..models import User, Workspace, WorkspaceMember


router = APIRouter(prefix="/api/admin", tags=["admin"])


SLUG_RE = re.compile(r"^[a-z0-9][a-z0-9-]{1,40}$")


class UserOut(BaseModel):
    id: int
    username: str
    display_name: str
    is_admin: bool
    class Config: from_attributes = True


class CreateUserIn(BaseModel):
    username: str
    password: str = Field(min_length=6)
    display_name: str = ""
    is_admin: bool = False


class WorkspaceOut(BaseModel):
    id: int
    slug: str
    name: str
    owner_id: int
    member_ids: List[int] = []


class WorkspaceIn(BaseModel):
    slug: str
    name: str
    member_ids: List[int] = []


def _ws_out(session: Session, w: Workspace) -> WorkspaceOut:
    ms = session.exec(select(WorkspaceMember).where(WorkspaceMember.workspace_id == w.id)).all()
    return WorkspaceOut(id=w.id, slug=w.slug, name=w.name, owner_id=w.owner_id,
                        member_ids=[m.user_id for m in ms])


# ---------- users ----------
@router.get("/users", response_model=List[UserOut])
def list_users(_: User = Depends(require_admin), session: Session = Depends(get_session)):
    return [UserOut.model_validate(u) for u in session.exec(select(User).order_by(User.username)).all()]


@router.post("/users", response_model=UserOut)
def create_user(payload: CreateUserIn, _: User = Depends(require_admin), session: Session = Depends(get_session)):
    if session.exec(select(User).where(User.username == payload.username)).first():
        raise HTTPException(400, "Такое имя уже занято")
    u = User(
        username=payload.username,
        display_name=payload.display_name or payload.username,
        password_hash=hash_password(payload.password),
        is_admin=payload.is_admin,
    )
    session.add(u); session.commit(); session.refresh(u)
    return UserOut.model_validate(u)


class ResetPasswordIn(BaseModel):
    password: str = Field(min_length=6)


@router.post("/users/{user_id}/password", response_model=UserOut)
def reset_password(user_id: int, payload: ResetPasswordIn, _: User = Depends(require_admin), session: Session = Depends(get_session)):
    u = session.get(User, user_id)
    if not u:
        raise HTTPException(404)
    u.password_hash = hash_password(payload.password)
    session.add(u); session.commit(); session.refresh(u)
    return UserOut.model_validate(u)


@router.delete("/users/{user_id}")
def delete_user(user_id: int, admin: User = Depends(require_admin), session: Session = Depends(get_session)):
    if user_id == admin.id:
        raise HTTPException(400, "Нельзя удалить самого себя")
    u = session.get(User, user_id)
    if not u:
        raise HTTPException(404)
    for m in session.exec(select(WorkspaceMember).where(WorkspaceMember.user_id == user_id)).all():
        session.delete(m)
    session.delete(u); session.commit()
    return {"ok": True}


# ---------- workspaces ----------
@router.get("/workspaces", response_model=List[WorkspaceOut])
def list_workspaces(_: User = Depends(require_admin), session: Session = Depends(get_session)):
    return [_ws_out(session, w) for w in session.exec(select(Workspace).order_by(Workspace.slug)).all()]


@router.post("/workspaces", response_model=WorkspaceOut)
def create_workspace(payload: WorkspaceIn, admin: User = Depends(require_admin), session: Session = Depends(get_session)):
    if not SLUG_RE.match(payload.slug):
        raise HTTPException(400, "Slug: латинские буквы, цифры и дефис, 2–40 символов")
    if session.exec(select(Workspace).where(Workspace.slug == payload.slug)).first():
        raise HTTPException(400, "Такой slug уже занят")
    w = Workspace(slug=payload.slug, name=payload.name, owner_id=admin.id)
    session.add(w); session.commit(); session.refresh(w)
    ids = set(payload.member_ids) | {admin.id}
    for uid in ids:
        session.add(WorkspaceMember(workspace_id=w.id, user_id=uid, role="owner" if uid == admin.id else "member"))
    session.commit()
    return _ws_out(session, w)


@router.put("/workspaces/{ws_id}", response_model=WorkspaceOut)
def update_workspace(ws_id: int, payload: WorkspaceIn, _: User = Depends(require_admin), session: Session = Depends(get_session)):
    w = session.get(Workspace, ws_id)
    if not w:
        raise HTTPException(404)
    w.name = payload.name
    session.add(w)
    current = session.exec(select(WorkspaceMember).where(WorkspaceMember.workspace_id == w.id)).all()
    keep = set(payload.member_ids) | {w.owner_id}
    have = {m.user_id: m for m in current}
    for uid, m in have.items():
        if uid not in keep:
            session.delete(m)
    for uid in keep - set(have.keys()):
        session.add(WorkspaceMember(workspace_id=w.id, user_id=uid, role="owner" if uid == w.owner_id else "member"))
    session.commit(); session.refresh(w)
    return _ws_out(session, w)


@router.delete("/workspaces/{ws_id}")
def delete_workspace(ws_id: int, _: User = Depends(require_admin), session: Session = Depends(get_session)):
    w = session.get(Workspace, ws_id)
    if not w:
        raise HTTPException(404)
    for m in session.exec(select(WorkspaceMember).where(WorkspaceMember.workspace_id == w.id)).all():
        session.delete(m)
    session.delete(w); session.commit()
    return {"ok": True}


# ---------- what a user can see ----------
@router.get("/my-workspaces", response_model=List[WorkspaceOut])
def my_workspaces(user: User = Depends(get_current_user), session: Session = Depends(get_session)):
    ws_ids = session.exec(select(WorkspaceMember.workspace_id).where(WorkspaceMember.user_id == user.id)).all()
    if not ws_ids:
        return []
    return [_ws_out(session, w) for w in session.exec(select(Workspace).where(Workspace.id.in_(ws_ids))).all()]
