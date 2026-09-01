from __future__ import annotations
import json
import logging
from typing import Iterable
from pywebpush import webpush, WebPushException
from sqlmodel import Session, select

from .config import settings
from .models import PushSubscription
from .vapid import ensure_vapid_keys

log = logging.getLogger(__name__)


def _priv_pem() -> str:
    priv_path, _, _ = ensure_vapid_keys()
    with open(priv_path, "r") as f:
        return f.read()


def send_push(sub: PushSubscription, payload: dict) -> bool:
    try:
        webpush(
            subscription_info={
                "endpoint": sub.endpoint,
                "keys": {"p256dh": sub.p256dh, "auth": sub.auth},
            },
            data=json.dumps(payload),
            vapid_private_key=_priv_pem(),
            vapid_claims={"sub": settings.vapid_subject},
            ttl=60 * 60 * 24,
        )
        return True
    except WebPushException as e:
        log.warning("push failed for %s: %s", sub.endpoint[:60], e)
        # 404/410 — подписка мертва
        if e.response is not None and e.response.status_code in (404, 410):
            return False
        return False
    except Exception as e:
        log.warning("push error: %s", e)
        return False


def push_to_users(session: Session, user_ids: Iterable[int], payload: dict) -> None:
    uids = list(set(user_ids))
    if not uids:
        return
    subs = session.exec(select(PushSubscription).where(PushSubscription.user_id.in_(uids))).all()
    dead: list[int] = []
    for s in subs:
        ok = send_push(s, payload)
        if not ok:
            dead.append(s.id)
    for sid in dead:
        obj = session.get(PushSubscription, sid)
        if obj:
            session.delete(obj)
    if dead:
        session.commit()
