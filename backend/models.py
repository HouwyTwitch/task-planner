from __future__ import annotations

from datetime import datetime, timezone
from typing import Optional
from sqlmodel import SQLModel, Field, Relationship, Column, JSON


def utcnow() -> datetime:
    return datetime.now(timezone.utc).replace(tzinfo=None)


class User(SQLModel, table=True):
    id: Optional[int] = Field(default=None, primary_key=True)
    username: str = Field(index=True, unique=True)
    display_name: str = ""
    password_hash: str
    is_admin: bool = False
    created_at: datetime = Field(default_factory=utcnow)


class PushSubscription(SQLModel, table=True):
    id: Optional[int] = Field(default=None, primary_key=True)
    user_id: int = Field(foreign_key="user.id", index=True)
    endpoint: str = Field(unique=True)
    p256dh: str
    auth: str
    user_agent: str = ""
    created_at: datetime = Field(default_factory=utcnow)


class Project(SQLModel, table=True):
    id: Optional[int] = Field(default=None, primary_key=True)
    name: str
    color: str = "#5b8def"
    owner_id: int = Field(foreign_key="user.id")
    created_at: datetime = Field(default_factory=utcnow)


class ProjectMember(SQLModel, table=True):
    id: Optional[int] = Field(default=None, primary_key=True)
    project_id: int = Field(foreign_key="project.id", index=True)
    user_id: int = Field(foreign_key="user.id", index=True)
    role: str = "member"  # owner | member


class Task(SQLModel, table=True):
    id: Optional[int] = Field(default=None, primary_key=True)
    title: str
    notes: str = ""
    project_id: Optional[int] = Field(default=None, foreign_key="project.id", index=True)
    assignee_id: Optional[int] = Field(default=None, foreign_key="user.id", index=True)
    created_by: int = Field(foreign_key="user.id")

    is_done: bool = False
    done_at: Optional[datetime] = None

    estimate_minutes: int = 0
    spent_minutes: int = 0

    # Планирование
    scheduled_at: Optional[datetime] = None  # UTC naive, момент выполнения
    due_at: Optional[datetime] = None

    # Приоритет
    priority: int = 0  # 0 low, 1 med, 2 high, 3 urgent

    # Регулярность (RRULE-подобное)
    # kind: none | daily | weekly | monthly | custom_days
    recurrence: dict = Field(sa_column=Column(JSON), default_factory=dict)
    # Для регулярных: следующая дата генерации инстанса
    next_occurrence_at: Optional[datetime] = None

    # Порядок (для kanban)
    sort_order: float = 0.0

    created_at: datetime = Field(default_factory=utcnow)
    updated_at: datetime = Field(default_factory=utcnow)


class Reminder(SQLModel, table=True):
    id: Optional[int] = Field(default=None, primary_key=True)
    task_id: int = Field(foreign_key="task.id", index=True)
    # смещение до scheduled_at: {"amount": 30, "unit": "minutes"}
    offset_amount: int = 0
    offset_unit: str = "minutes"  # minutes|hours|days|weeks
    # если true — если срабатывает в выходной, сдвигаем на ближайший рабочий день
    snap_to_workday: bool = True
    # рассчитанное время срабатывания (UTC naive)
    fire_at: datetime
    fired_at: Optional[datetime] = None
    acknowledged_at: Optional[datetime] = None  # пользователь подтвердил (или увидел)
    # Кому отправлять: null = всем участникам проекта / assignee
    target_user_id: Optional[int] = Field(default=None, foreign_key="user.id")


class ActivityLog(SQLModel, table=True):
    id: Optional[int] = Field(default=None, primary_key=True)
    task_id: Optional[int] = Field(default=None, foreign_key="task.id", index=True)
    user_id: int = Field(foreign_key="user.id")
    action: str  # created|updated|completed|reopened|commented|reminded
    payload: dict = Field(sa_column=Column(JSON), default_factory=dict)
    created_at: datetime = Field(default_factory=utcnow)
