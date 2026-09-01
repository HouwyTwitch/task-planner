from __future__ import annotations
import logging
from contextlib import asynccontextmanager
from pathlib import Path

from fastapi import FastAPI, WebSocket, WebSocketDisconnect, Depends
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import FileResponse, JSONResponse
from fastapi.staticfiles import StaticFiles
from sqlmodel import Session

from .config import BASE_DIR, settings
from .db import get_session, init_db
from .auth import get_user_by_token
from .routes import auth as auth_routes
from .routes import projects as projects_routes
from .routes import tasks as tasks_routes
from .routes import push as push_routes
from .scheduler import start_scheduler, stop_scheduler
from .vapid import ensure_vapid_keys
from .ws import hub


logging.basicConfig(level=logging.INFO, format="%(asctime)s %(levelname)s %(name)s: %(message)s")


@asynccontextmanager
async def lifespan(app: FastAPI):
    init_db()
    ensure_vapid_keys()
    start_scheduler()
    yield
    stop_scheduler()


app = FastAPI(title=settings.app_name, lifespan=lifespan)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.include_router(auth_routes.router)
app.include_router(projects_routes.router)
app.include_router(tasks_routes.router)
app.include_router(push_routes.router)


@app.get("/api/health")
def health():
    return {"ok": True}


@app.websocket("/ws")
async def websocket_endpoint(ws: WebSocket, token: str):
    from .db import Session as _S  # not used; keep get_session pattern manual
    from sqlalchemy.orm import Session as SASession
    from .db import engine
    with Session(engine) as session:
        user = get_user_by_token(token, session)
    if not user:
        await ws.close(code=4401)
        return
    await hub.connect(user.id, ws)
    try:
        while True:
            # клиент может слать пинги / ack; читаем и игнорируем
            await ws.receive_text()
    except WebSocketDisconnect:
        pass
    finally:
        await hub.disconnect(user.id, ws)


# Статика фронтенда
FRONTEND_DIR = BASE_DIR / "frontend"

app.mount("/static", StaticFiles(directory=str(FRONTEND_DIR / "static")), name="static")


@app.get("/sw.js")
def sw_js():
    return FileResponse(str(FRONTEND_DIR / "sw.js"), media_type="application/javascript")


@app.get("/")
def index():
    return FileResponse(str(FRONTEND_DIR / "index.html"))


@app.get("/{full_path:path}")
def spa_fallback(full_path: str):
    # SPA: любой не-API маршрут отдаёт index.html
    if full_path.startswith("api/") or full_path.startswith("ws"):
        return JSONResponse({"detail": "Not Found"}, status_code=404)
    idx = FRONTEND_DIR / "index.html"
    if idx.exists():
        return FileResponse(str(idx))
    return JSONResponse({"detail": "Not Found"}, status_code=404)
