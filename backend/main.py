from __future__ import annotations
import logging
from contextlib import asynccontextmanager
from pathlib import Path

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import FileResponse, JSONResponse
from fastapi.staticfiles import StaticFiles

from .config import BASE_DIR, settings
from .db import init_db
from .routes import auth as auth_routes
from .routes import admin as admin_routes
from . import webdav as webdav_module


logging.basicConfig(level=logging.INFO, format="%(asctime)s %(levelname)s %(name)s: %(message)s")


@asynccontextmanager
async def lifespan(app: FastAPI):
    init_db()
    yield


app = FastAPI(title=settings.app_name, lifespan=lifespan)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
    expose_headers=["ETag", "Last-Modified", "DAV"],
)

app.include_router(auth_routes.router)
app.include_router(admin_routes.router)
app.include_router(webdav_module.router)


@app.get("/api/health")
def health():
    return {"ok": True}


ADMIN_DIR = BASE_DIR / "admin_ui"
SP_DIST = BASE_DIR / "sp-dist"


@app.get("/admin")
@app.get("/admin/")
def admin_index():
    idx = ADMIN_DIR / "index.html"
    if idx.exists():
        return FileResponse(str(idx))
    return JSONResponse({"detail": "admin UI not deployed"}, status_code=404)


@app.get("/app")
@app.get("/app/")
def sp_index():
    idx = SP_DIST / "index.html"
    if idx.exists():
        return FileResponse(str(idx))
    return JSONResponse({"detail": "super-productivity build not found"}, status_code=503)


@app.get("/app/{full_path:path}")
def sp_spa(full_path: str):
    direct = SP_DIST / full_path
    if direct.exists() and direct.is_file():
        return FileResponse(str(direct))
    idx = SP_DIST / "index.html"
    if idx.exists():
        return FileResponse(str(idx))
    return JSONResponse({"detail": "not found"}, status_code=404)


if ADMIN_DIR.exists():
    app.mount("/admin-assets", StaticFiles(directory=str(ADMIN_DIR)), name="admin-static")


@app.get("/")
def root():
    idx = ADMIN_DIR / "index.html"
    if idx.exists():
        return FileResponse(str(idx))
    return JSONResponse({
        "app": settings.app_name,
        "admin": "/admin",
        "super_productivity": "/app",
        "webdav": "/webdav/<workspace-slug>/",
    })
