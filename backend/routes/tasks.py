from __future__ import annotations
from datetime import datetime
from typing import Optional
from fastapi import APIRouter, Depends, HTTPException, Query
from sqlmodel import Session, select

from ..auth import get_current_user
from ..db import get_session
from ..models import ActivityLog, Project, ProjectMember, Reminder, Task, User, utcnow
from ..schemas import TaskIn, TaskOut, TaskPatch
from ..services import (
    load_task_with_reminders,
    project_member_ids,
    rebuild_reminders,
    stakeholders_for_task,
    user_can_access_project,
    user_can_access_task,
)
from ..ws import hub

router = APIRouter(prefix="/api/tasks", tags=["tasks"])


async def _broadcast(session: Session, task: Task, event: str, extra: Optional[dict] = None):
    payload = {"type": event, "task": load_task_with_reminders(session, task)}
    if extra:
        payload.update(extra)
    await hub.send_to_users(stakeholders_for_task(session, task), payload)


@router.get("", response_model=list[TaskOut])
def list_tasks(
    project_id: Optional[int] = None,
    include_done: bool = Query(False),
    user: User = Depends(get_current_user),
    session: Session = Depends(get_session),
):
    q = select(Task)
    if project_id is not None:
        if not user_can_access_project(session, user, project_id):
            raise HTTPException(403, "Нет доступа к проекту")
        q = q.where(Task.project_id == project_id)
    else:
        # личные + все из моих проектов
        my_projects = session.exec(select(Project.id).where(Project.owner_id == user.id)).all()
        joined = session.exec(select(ProjectMember.project_id).where(ProjectMember.user_id == user.id)).all()
        pids = set(my_projects) | set(joined)
        q = q.where(
            (Task.created_by == user.id) |
            (Task.assignee_id == user.id) |
            (Task.project_id.in_(pids)) if pids else
            ((Task.created_by == user.id) | (Task.assignee_id == user.id))
        )
    if not include_done:
        q = q.where(Task.is_done == False)  # noqa: E712
    tasks = session.exec(q.order_by(Task.sort_order, Task.id)).all()
    return [load_task_with_reminders(session, t) for t in tasks]


@router.post("", response_model=TaskOut)
async def create_task(payload: TaskIn, user: User = Depends(get_current_user), session: Session = Depends(get_session)):
    if payload.project_id is not None and not user_can_access_project(session, user, payload.project_id):
        raise HTTPException(403, "Нет доступа к проекту")
    t = Task(
        title=payload.title,
        notes=payload.notes,
        project_id=payload.project_id,
        assignee_id=payload.assignee_id,
        created_by=user.id,
        estimate_minutes=payload.estimate_minutes,
        priority=payload.priority,
        scheduled_at=payload.scheduled_at,
        due_at=payload.due_at,
        recurrence=(payload.recurrence.model_dump() if payload.recurrence else {}),
    )
    session.add(t)
    session.commit()
    session.refresh(t)

    rebuild_reminders(session, t, [r.model_dump() for r in payload.reminders])
    session.add(ActivityLog(task_id=t.id, user_id=user.id, action="created", payload={}))
    session.commit()
    await _broadcast(session, t, "task.created")
    return load_task_with_reminders(session, t)


@router.get("/{task_id}", response_model=TaskOut)
def get_task(task_id: int, user: User = Depends(get_current_user), session: Session = Depends(get_session)):
    t = session.get(Task, task_id)
    if not t:
        raise HTTPException(404)
    if not user_can_access_task(session, user, t):
        raise HTTPException(403)
    return load_task_with_reminders(session, t)


@router.patch("/{task_id}", response_model=TaskOut)
async def patch_task(task_id: int, payload: TaskPatch, user: User = Depends(get_current_user), session: Session = Depends(get_session)):
    t = session.get(Task, task_id)
    if not t:
        raise HTTPException(404)
    if not user_can_access_task(session, user, t):
        raise HTTPException(403)

    data = payload.model_dump(exclude_unset=True)
    if "recurrence" in data:
        rec = data.pop("recurrence")
        t.recurrence = rec if rec else {}
    if "is_done" in data:
        t.is_done = bool(data["is_done"])
        t.done_at = utcnow() if t.is_done else None
        data.pop("is_done")
    reminders_in = data.pop("reminders", None)
    for k, v in data.items():
        setattr(t, k, v)
    t.updated_at = utcnow()

    session.add(t)
    session.commit()
    session.refresh(t)
    if reminders_in is not None:
        rebuild_reminders(session, t, [r if isinstance(r, dict) else r.model_dump() for r in reminders_in])
    else:
        # если пересчитались scheduled_at — пересчитать fire_at существующих
        if "scheduled_at" in data and t.scheduled_at is not None:
            existing = session.exec(select(Reminder).where(Reminder.task_id == t.id)).all()
            rebuild_reminders(
                session, t,
                [
                    {
                        "offset_amount": r.offset_amount,
                        "offset_unit": r.offset_unit,
                        "snap_to_workday": r.snap_to_workday,
                        "target_user_id": r.target_user_id,
                    }
                    for r in existing
                ],
            )
    session.add(ActivityLog(task_id=t.id, user_id=user.id, action="updated", payload=data))
    session.commit()
    await _broadcast(session, t, "task.updated")
    return load_task_with_reminders(session, t)


@router.delete("/{task_id}")
async def delete_task(task_id: int, user: User = Depends(get_current_user), session: Session = Depends(get_session)):
    t = session.get(Task, task_id)
    if not t:
        raise HTTPException(404)
    if not user_can_access_task(session, user, t):
        raise HTTPException(403)
    stakeholders = stakeholders_for_task(session, t)
    for r in session.exec(select(Reminder).where(Reminder.task_id == t.id)).all():
        session.delete(r)
    session.delete(t)
    session.commit()
    await hub.send_to_users(stakeholders, {"type": "task.deleted", "task_id": task_id})
    return {"ok": True}


@router.post("/{task_id}/reminders/{rid}/ack")
async def ack_reminder(task_id: int, rid: int, user: User = Depends(get_current_user), session: Session = Depends(get_session)):
    r = session.get(Reminder, rid)
    if not r or r.task_id != task_id:
        raise HTTPException(404)
    r.acknowledged_at = utcnow()
    session.add(r)
    session.commit()
    t = session.get(Task, task_id)
    if t:
        await _broadcast(session, t, "reminder.ack", {"reminder_id": rid})
    return {"ok": True}


@router.get("/missed/all")
def missed_reminders(user: User = Depends(get_current_user), session: Session = Depends(get_session)):
    """Напоминания, которые сработали, но пользователь их не подтвердил."""
    now = utcnow()
    q = (
        select(Reminder, Task)
        .join(Task, Task.id == Reminder.task_id)
        .where(Reminder.fired_at != None)  # noqa: E711
        .where(Reminder.acknowledged_at == None)  # noqa: E711
        .where(Reminder.fire_at <= now)
    )
    result = []
    for r, t in session.exec(q).all():
        if r.target_user_id is not None and r.target_user_id != user.id:
            continue
        if not user_can_access_task(session, user, t):
            continue
        result.append({
            "reminder": {
                "id": r.id, "task_id": r.task_id,
                "offset_amount": r.offset_amount, "offset_unit": r.offset_unit,
                "snap_to_workday": r.snap_to_workday,
                "fire_at": r.fire_at, "fired_at": r.fired_at,
                "acknowledged_at": r.acknowledged_at,
                "target_user_id": r.target_user_id,
            },
            "task": load_task_with_reminders(session, t),
        })
    return result
