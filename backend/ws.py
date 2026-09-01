from __future__ import annotations
import asyncio
import json
from typing import Dict, Set
from fastapi import WebSocket


class Hub:
    def __init__(self) -> None:
        self._by_user: Dict[int, Set[WebSocket]] = {}
        self._lock = asyncio.Lock()

    async def connect(self, user_id: int, ws: WebSocket) -> None:
        await ws.accept()
        async with self._lock:
            self._by_user.setdefault(user_id, set()).add(ws)

    async def disconnect(self, user_id: int, ws: WebSocket) -> None:
        async with self._lock:
            conns = self._by_user.get(user_id)
            if conns and ws in conns:
                conns.remove(ws)
                if not conns:
                    self._by_user.pop(user_id, None)

    async def send_to_users(self, user_ids: list[int], payload: dict) -> None:
        text = json.dumps(payload, default=str)
        targets: list[WebSocket] = []
        async with self._lock:
            for uid in set(user_ids):
                for ws in self._by_user.get(uid, set()):
                    targets.append(ws)
        for ws in targets:
            try:
                await ws.send_text(text)
            except Exception:
                pass


hub = Hub()
