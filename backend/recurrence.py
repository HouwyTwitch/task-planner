from __future__ import annotations
from datetime import datetime, timedelta
from typing import Optional


def _is_weekend(dt: datetime) -> bool:
    return dt.weekday() >= 5


def snap_forward_to_workday(dt: datetime) -> datetime:
    """Если попадает на сб/вс, сдвигаем на понедельник, сохраняя время."""
    while _is_weekend(dt):
        dt = dt + timedelta(days=1)
    return dt


def compute_reminder_fire_at(
    scheduled_at: datetime,
    amount: int,
    unit: str,
    snap_to_workday: bool,
) -> datetime:
    unit_map = {
        "minutes": timedelta(minutes=amount),
        "hours": timedelta(hours=amount),
        "days": timedelta(days=amount),
        "weeks": timedelta(weeks=amount),
    }
    delta = unit_map.get(unit, timedelta(0))
    fire = scheduled_at - delta
    if snap_to_workday:
        # если напоминание попадает на выходной, сдвигаем к следующему рабочему дню на то же время
        fire = snap_forward_to_workday(fire)
    return fire


def next_occurrence(base: datetime, recurrence: dict) -> Optional[datetime]:
    """Следующее время после base по правилу recurrence. None — если нет."""
    if not recurrence:
        return None
    kind = recurrence.get("kind", "none")
    interval = max(1, int(recurrence.get("interval", 1)))
    if kind == "none":
        return None
    if kind == "daily":
        return base + timedelta(days=interval)
    if kind == "workdays":
        nxt = base + timedelta(days=1)
        while _is_weekend(nxt):
            nxt = nxt + timedelta(days=1)
        return nxt
    if kind == "weekly":
        weekdays = sorted(set(int(x) for x in recurrence.get("weekdays") or [base.weekday()]))
        # ищем ближайший weekday после base в рамках недельного интервала
        for add in range(1, 7 * interval + 8):
            cand = base + timedelta(days=add)
            if cand.weekday() in weekdays:
                # проверяем недельный шаг
                if interval == 1:
                    return cand
                # для interval>1 просто прыгаем на interval недель вперёд от текущего дня недели
                weeks_delta = (cand.date() - base.date()).days // 7
                if weeks_delta % interval == 0:
                    return cand
        return None
    if kind == "monthly":
        dom = int(recurrence.get("day_of_month") or base.day)
        # прибавляем interval месяцев
        year = base.year
        month = base.month + interval
        while month > 12:
            month -= 12
            year += 1
        # клемпим день
        for d in (dom, 28, 27, 26):
            try:
                return base.replace(year=year, month=month, day=min(d, dom))
            except ValueError:
                continue
        return None
    return None
