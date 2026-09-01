from __future__ import annotations
import asyncio
import logging
from apscheduler.schedulers.asyncio import AsyncIOScheduler
from sqlmodel import Session, select

from .config import settings
from .db import engine
from .models import ActivityLog, Reminder, Task, utcnow
from .push import push_to_users
from .recurrence import compute_reminder_fire_at, next_occurrence
from .services import stakeholders_for_task, load_task_with_reminders
from .ws import hub

log = logging.getLogger(__name__)
scheduler = AsyncIOScheduler(timezone="UTC")


async def _fire_reminder(session: Session, r: Reminder, t: Task) -> None:
    r.fired_at = utcnow()
    session.add(r)
    session.add(ActivityLog(task_id=t.id, user_id=t.created_by, action="reminded", payload={"reminder_id": r.id}))
    session.commit()

    if r.target_user_id is not None:
        targets = [r.target_user_id]
    else:
        targets = stakeholders_for_task(session, t)

    push_payload = {
        "type": "reminder",
        "task_id": t.id,
        "reminder_id": r.id,
        "title": t.title,
        "notes": (t.notes or "")[:200],
        "scheduled_at": t.scheduled_at.isoformat() if t.scheduled_at else None,
    }
    push_to_users(session, targets, push_payload)
    await hub.send_to_users(targets, {
        "type": "reminder.fire",
        "task": load_task_with_reminders(session, t),
        "reminder_id": r.id,
    })


async def _spawn_recurring(session: Session, t: Task) -> None:
    if not t.recurrence or (t.recurrence.get("kind") in (None, "none")):
        return
    if not t.scheduled_at:
        return
    nxt = next_occurrence(t.scheduled_at, t.recurrence)
    if not nxt:
        return
    # клонируем задачу
    new_t = Task(
        title=t.title,
        notes=t.notes,
        project_id=t.project_id,
        assignee_id=t.assignee_id,
        created_by=t.created_by,
        estimate_minutes=t.estimate_minutes,
        priority=t.priority,
        scheduled_at=nxt,
        due_at=None,
        recurrence=t.recurrence,
        sort_order=t.sort_order,
    )
    session.add(new_t)
    session.commit()
    session.refresh(new_t)

    # клонируем те же напоминания
    existing = session.exec(select(Reminder).where(Reminder.task_id == t.id)).all()
    for r in existing:
        fire_at = compute_reminder_fire_at(nxt, r.offset_amount, r.offset_unit, r.snap_to_workday)
        session.add(Reminder(
            task_id=new_t.id,
            offset_amount=r.offset_amount,
            offset_unit=r.offset_unit,
            snap_to_workday=r.snap_to_workday,
            fire_at=fire_at,
            target_user_id=r.target_user_id,
        ))
    session.commit()
    await hub.send_to_users(stakeholders_for_task(session, new_t), {
        "type": "task.created", "task": load_task_with_reminders(session, new_t),
    })


async def _tick():
    now = utcnow()
    try:
        with Session(engine) as session:
            # 1. Сработавшие напоминания
            due = session.exec(
                select(Reminder).where(Reminder.fired_at == None).where(Reminder.fire_at <= now)  # noqa: E711
            ).all()
            for r in due:
                t = session.get(Task, r.task_id)
                if not t or t.is_done:
                    r.fired_at = now
                    session.add(r); session.commit()
                    continue
                await _fire_reminder(session, r, t)

            # 2. Регулярные задачи: если задача завершена и рекуррентная — родить следующую (один раз)
            done_recurring = session.exec(
                select(Task).where(Task.is_done == True).where(Task.next_occurrence_at == None)  # noqa: E712, E711
            ).all()
            for t in done_recurring:
                if not t.recurrence or t.recurrence.get("kind") in (None, "none"):
                    continue
                if not t.scheduled_at:
                    continue
                await _spawn_recurring(session, t)
                # помечаем, что уже сгенерирована
                t.next_occurrence_at = now
                session.add(t); session.commit()

            # 3. Регулярные задачи по расписанию: если scheduled_at прошло и next_occurrence_at пусто
            passed = session.exec(
                select(Task).where(Task.is_done == False).where(Task.next_occurrence_at == None)  # noqa: E712, E711
                .where(Task.scheduled_at != None)  # noqa: E711
                .where(Task.scheduled_at <= now)
            ).all()
            for t in passed:
                if not t.recurrence or t.recurrence.get("kind") in (None, "none"):
                    continue
                await _spawn_recurring(session, t)
                t.next_occurrence_at = now
                session.add(t); session.commit()
    except Exception as e:
        log.exception("tick failed: %s", e)


def start_scheduler(loop: asyncio.AbstractEventLoop | None = None) -> None:
    if scheduler.running:
        return
    scheduler.add_job(_tick, "interval", seconds=settings.scheduler_tick_seconds, id="tick", replace_existing=True)
    scheduler.start()


def stop_scheduler() -> None:
    if scheduler.running:
        scheduler.shutdown(wait=False)
