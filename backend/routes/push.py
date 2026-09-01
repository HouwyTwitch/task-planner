from fastapi import APIRouter, Depends
from sqlmodel import Session, select

from ..auth import get_current_user
from ..db import get_session
from ..models import PushSubscription, User
from ..schemas import PushSubIn
from ..vapid import public_key_b64

router = APIRouter(prefix="/api/push", tags=["push"])


@router.get("/vapid-public-key")
def get_vapid():
    return {"key": public_key_b64()}


@router.post("/subscribe")
def subscribe(payload: PushSubIn, user: User = Depends(get_current_user), session: Session = Depends(get_session)):
    exists = session.exec(select(PushSubscription).where(PushSubscription.endpoint == payload.endpoint)).first()
    if exists:
        exists.user_id = user.id
        exists.p256dh = payload.keys.get("p256dh", "")
        exists.auth = payload.keys.get("auth", "")
        exists.user_agent = payload.user_agent
        session.add(exists)
    else:
        session.add(PushSubscription(
            user_id=user.id,
            endpoint=payload.endpoint,
            p256dh=payload.keys.get("p256dh", ""),
            auth=payload.keys.get("auth", ""),
            user_agent=payload.user_agent,
        ))
    session.commit()
    return {"ok": True}


@router.post("/unsubscribe")
def unsubscribe(payload: dict, user: User = Depends(get_current_user), session: Session = Depends(get_session)):
    endpoint = payload.get("endpoint", "")
    sub = session.exec(select(PushSubscription).where(PushSubscription.endpoint == endpoint)).first()
    if sub and sub.user_id == user.id:
        session.delete(sub)
        session.commit()
    return {"ok": True}
