from __future__ import annotations
from datetime import datetime
from typing import Optional, List, Dict, Any
from pydantic import BaseModel, Field


class UserOut(BaseModel):
    id: int
    username: str
    display_name: str
    is_admin: bool

    class Config:
        from_attributes = True


class RegisterIn(BaseModel):
    username: str
    password: str = Field(min_length=6)
    display_name: str = ""


class TokenOut(BaseModel):
    access_token: str
    token_type: str = "bearer"
    user: UserOut


class ProjectIn(BaseModel):
    name: str
    color: str = "#5b8def"
    member_ids: List[int] = []


class ProjectOut(BaseModel):
    id: int
    name: str
    color: str
    owner_id: int
    member_ids: List[int] = []

    class Config:
        from_attributes = True


class ReminderIn(BaseModel):
    offset_amount: int = 0
    offset_unit: str = "minutes"  # minutes|hours|days|weeks
    snap_to_workday: bool = True
    target_user_id: Optional[int] = None


class ReminderOut(BaseModel):
    id: int
    task_id: int
    offset_amount: int
    offset_unit: str
    snap_to_workday: bool
    fire_at: datetime
    fired_at: Optional[datetime]
    acknowledged_at: Optional[datetime]
    target_user_id: Optional[int]

    class Config:
        from_attributes = True


class RecurrenceModel(BaseModel):
    """
    kind: none | daily | weekly | monthly | workdays
    interval: N (каждые N единиц)
    weekdays: [0..6] (0=Mon) для weekly
    day_of_month: 1..31 для monthly
    """
    kind: str = "none"
    interval: int = 1
    weekdays: List[int] = []
    day_of_month: Optional[int] = None


class TaskIn(BaseModel):
    title: str
    notes: str = ""
    project_id: Optional[int] = None
    assignee_id: Optional[int] = None
    estimate_minutes: int = 0
    priority: int = 0
    scheduled_at: Optional[datetime] = None
    due_at: Optional[datetime] = None
    recurrence: Optional[RecurrenceModel] = None
    reminders: List[ReminderIn] = []


class TaskPatch(BaseModel):
    title: Optional[str] = None
    notes: Optional[str] = None
    project_id: Optional[int] = None
    assignee_id: Optional[int] = None
    estimate_minutes: Optional[int] = None
    spent_minutes: Optional[int] = None
    priority: Optional[int] = None
    scheduled_at: Optional[datetime] = None
    due_at: Optional[datetime] = None
    is_done: Optional[bool] = None
    sort_order: Optional[float] = None
    recurrence: Optional[RecurrenceModel] = None
    reminders: Optional[List[ReminderIn]] = None


class TaskOut(BaseModel):
    id: int
    title: str
    notes: str
    project_id: Optional[int]
    assignee_id: Optional[int]
    created_by: int
    is_done: bool
    done_at: Optional[datetime]
    estimate_minutes: int
    spent_minutes: int
    priority: int
    scheduled_at: Optional[datetime]
    due_at: Optional[datetime]
    sort_order: float
    recurrence: Dict[str, Any] = {}
    next_occurrence_at: Optional[datetime]
    reminders: List[ReminderOut] = []
    created_at: datetime
    updated_at: datetime

    class Config:
        from_attributes = True


class PushSubIn(BaseModel):
    endpoint: str
    keys: Dict[str, str]  # p256dh, auth
    user_agent: str = ""


class MissedReminderOut(BaseModel):
    reminder: ReminderOut
    task: TaskOut
