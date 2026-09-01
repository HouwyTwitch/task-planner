from __future__ import annotations
import base64
from datetime import datetime, timedelta, timezone
from typing import Optional, Tuple
from fastapi import Depends, HTTPException, Request, status
from fastapi.security import OAuth2PasswordBearer
from jose import jwt, JWTError
from passlib.context import CryptContext
from sqlmodel import Session, select

from .config import settings
from .db import get_session
from .models import User


pwd_ctx = CryptContext(schemes=["bcrypt"], deprecated="auto")
oauth2_scheme = OAuth2PasswordBearer(tokenUrl="/api/auth/login", auto_error=False)


def hash_password(pw: str) -> str:
    return pwd_ctx.hash(pw)


def verify_password(pw: str, ph: str) -> bool:
    return pwd_ctx.verify(pw, ph)


def create_access_token(user_id: int) -> str:
    exp = datetime.now(timezone.utc) + timedelta(minutes=settings.access_token_ttl_minutes)
    return jwt.encode({"sub": str(user_id), "exp": exp}, settings.secret_key, algorithm=settings.algorithm)


def decode_token(token: str) -> Optional[int]:
    try:
        payload = jwt.decode(token, settings.secret_key, algorithms=[settings.algorithm])
        return int(payload["sub"])
    except (JWTError, KeyError, ValueError):
        return None


def get_current_user(
    token: Optional[str] = Depends(oauth2_scheme),
    session: Session = Depends(get_session),
) -> User:
    if not token:
        raise HTTPException(status.HTTP_401_UNAUTHORIZED, "Not authenticated")
    uid = decode_token(token)
    if uid is None:
        raise HTTPException(status.HTTP_401_UNAUTHORIZED, "Invalid token")
    user = session.get(User, uid)
    if user is None:
        raise HTTPException(status.HTTP_401_UNAUTHORIZED, "User not found")
    return user


def require_admin(user: User = Depends(get_current_user)) -> User:
    if not user.is_admin:
        raise HTTPException(status.HTTP_403_FORBIDDEN, "Требуются права администратора")
    return user


def parse_basic_auth(request: Request) -> Optional[Tuple[str, str]]:
    hdr = request.headers.get("authorization", "")
    if not hdr.lower().startswith("basic "):
        return None
    try:
        raw = base64.b64decode(hdr[6:]).decode("utf-8", errors="replace")
        if ":" not in raw:
            return None
        u, p = raw.split(":", 1)
        return u, p
    except Exception:
        return None


def authenticate_basic(session: Session, request: Request) -> Optional[User]:
    creds = parse_basic_auth(request)
    if not creds:
        return None
    username, password = creds
    user = session.exec(select(User).where(User.username == username)).first()
    if not user or not verify_password(password, user.password_hash):
        return None
    return user
