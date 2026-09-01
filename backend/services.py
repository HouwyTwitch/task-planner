from __future__ import annotations
from datetime import datetime
from typing import Iterable, List, Optional
from sqlmodel import Session, select

from .models import Project, ProjectMember, Task, Reminder, User, utcnow
from .recurrence import compute_reminder_fire_at


def project_member_ids(session: Session, project_id: Optional[int]) -> List[int]:
    if project_id is None:
        return []
    rows = session.exec(select(ProjectMember).where(ProjectMember.project_id == project_id)).all()
    return [r.user_id for r in rows]


def user_can_access_project(session: Session, user: User, project_id: int) -> bool:
    proj = session.get(Project, project_id)
    if not proj:
        return False
    if proj.owner_id == user.id or user.is_admin:
        return True
    return user.id in project_member_ids(session, project_id)


def user_can_access_task(session: Session, user: User, task: Task) -> bool:
    if task.created_by == user.id or task.assignee_id == user.id or user.is_admin:
        return True
    if task.project_id is not None:
        return user_can_access_project(session, user, task.project_id)
    return False


def stakeholders_for_task(session: Session, task: Task) -> List[int]:
    """Кому рассылать события/пуши по задаче."""
    ids: set[int] = {task.created_by}
    if task.assignee_id:
        ids.add(task.assignee_id)
    if task.project_id:
        for uid in project_member_ids(session, task.project_id):
            ids.add(uid)
        proj = session.get(Project, task.project_id)
        if proj:
            ids.add(proj.owner_id)
    return list(ids)


def rebuild_reminders(
    session: Session,
    task: Task,
    reminders_in: Iterable[dict],
) -> List[Reminder]:
    """Пересоздаёт напоминания для задачи по списку {offset_amount, offset_unit, snap_to_workday, target_user_id}."""
    # удалить старые
    old = session.exec(select(Reminder).where(Reminder.task_id == task.id)).all()
    for r in old:
        session.delete(r)
    session.flush()

    created: List[Reminder] = []
    if not task.scheduled_at:
        return created
    for r in reminders_in:
        amount = int(r.get("offset_amount", 0))
        unit = r.get("offset_unit", "minutes")
        snap = bool(r.get("snap_to_workday", True))
        target = r.get("target_user_id")
        fire_at = compute_reminder_fire_at(task.scheduled_at, amount, unit, snap)
        rem = Reminder(
            task_id=task.id,
            offset_amount=amount,
            offset_unit=unit,
            snap_to_workday=snap,
            fire_at=fire_at,
            target_user_id=target,
        )
        session.add(rem)
        created.append(rem)
    session.flush()
    return created


def task_out_dict(task: Task, reminders: List[Reminder]) -> dict:
    return {
        "id": task.id,
        "title": task.title,
        "notes": task.notes,
        "project_id": task.project_id,
        "assignee_id": task.assignee_id,
        "created_by": task.created_by,
        "is_done": task.is_done,
        "done_at": task.done_at,
        "estimate_minutes": task.estimate_minutes,
        "spent_minutes": task.spent_minutes,
        "priority": task.priority,
        "scheduled_at": task.scheduled_at,
        "due_at": task.due_at,
        "sort_order": task.sort_order,
        "recurrence": task.recurrence or {},
        "next_occurrence_at": task.next_occurrence_at,
        "reminders": [
            {
                "id": r.id,
                "task_id": r.task_id,
                "offset_amount": r.offset_amount,
                "offset_unit": r.offset_unit,
                "snap_to_workday": r.snap_to_workday,
                "fire_at": r.fire_at,
                "fired_at": r.fired_at,
                "acknowledged_at": r.acknowledged_at,
                "target_user_id": r.target_user_id,
            }
            for r in reminders
        ],
        "created_at": task.created_at,
        "updated_at": task.updated_at,
    }


def load_task_with_reminders(session: Session, task: Task) -> dict:
    reminders = session.exec(select(Reminder).where(Reminder.task_id == task.id)).all()
    return task_out_dict(task, reminders)
