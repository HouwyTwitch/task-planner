from fastapi import APIRouter, Depends, HTTPException, status
from fastapi.security import OAuth2PasswordRequestForm
from sqlmodel import Session, select

from ..auth import create_access_token, get_current_user, hash_password, verify_password
from ..db import get_session
from ..models import User
from ..schemas import RegisterIn, TokenOut, UserOut

router = APIRouter(prefix="/api/auth", tags=["auth"])


@router.post("/register", response_model=TokenOut)
def register(payload: RegisterIn, session: Session = Depends(get_session)):
    exists = session.exec(select(User).where(User.username == payload.username)).first()
    if exists:
        raise HTTPException(status.HTTP_400_BAD_REQUEST, "Такое имя уже занято")
    # первый пользователь становится админом
    first = session.exec(select(User)).first() is None
    u = User(
        username=payload.username,
        display_name=payload.display_name or payload.username,
        password_hash=hash_password(payload.password),
        is_admin=first,
    )
    session.add(u)
    session.commit()
    session.refresh(u)
    return TokenOut(access_token=create_access_token(u.id), user=UserOut.model_validate(u))


@router.post("/login", response_model=TokenOut)
def login(form: OAuth2PasswordRequestForm = Depends(), session: Session = Depends(get_session)):
    u = session.exec(select(User).where(User.username == form.username)).first()
    if not u or not verify_password(form.password, u.password_hash):
        raise HTTPException(status.HTTP_401_UNAUTHORIZED, "Неверный логин или пароль")
    return TokenOut(access_token=create_access_token(u.id), user=UserOut.model_validate(u))


@router.get("/me", response_model=UserOut)
def me(user: User = Depends(get_current_user)):
    return UserOut.model_validate(user)


@router.get("/users", response_model=list[UserOut])
def list_users(user: User = Depends(get_current_user), session: Session = Depends(get_session)):
    users = session.exec(select(User).order_by(User.username)).all()
    return [UserOut.model_validate(u) for u in users]
