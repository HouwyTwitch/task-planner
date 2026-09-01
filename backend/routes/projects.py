from fastapi import APIRouter, Depends, HTTPException
from sqlmodel import Session, select

from ..auth import get_current_user
from ..db import get_session
from ..models import Project, ProjectMember, User
from ..schemas import ProjectIn, ProjectOut

router = APIRouter(prefix="/api/projects", tags=["projects"])


def _to_out(session: Session, p: Project) -> ProjectOut:
    members = session.exec(select(ProjectMember).where(ProjectMember.project_id == p.id)).all()
    return ProjectOut(
        id=p.id, name=p.name, color=p.color, owner_id=p.owner_id,
        member_ids=[m.user_id for m in members],
    )


@router.get("", response_model=list[ProjectOut])
def list_projects(user: User = Depends(get_current_user), session: Session = Depends(get_session)):
    mine = session.exec(
        select(Project).where(Project.owner_id == user.id)
    ).all()
    joined_ids = session.exec(
        select(ProjectMember.project_id).where(ProjectMember.user_id == user.id)
    ).all()
    joined = session.exec(select(Project).where(Project.id.in_(joined_ids))).all() if joined_ids else []
    seen: dict[int, Project] = {}
    for p in list(mine) + list(joined):
        seen[p.id] = p
    return [_to_out(session, p) for p in seen.values()]


@router.post("", response_model=ProjectOut)
def create_project(payload: ProjectIn, user: User = Depends(get_current_user), session: Session = Depends(get_session)):
    p = Project(name=payload.name, color=payload.color, owner_id=user.id)
    session.add(p)
    session.commit()
    session.refresh(p)
    # добавить членов
    member_ids = set(payload.member_ids) | {user.id}
    for uid in member_ids:
        session.add(ProjectMember(project_id=p.id, user_id=uid, role="owner" if uid == user.id else "member"))
    session.commit()
    return _to_out(session, p)


@router.put("/{project_id}", response_model=ProjectOut)
def update_project(project_id: int, payload: ProjectIn, user: User = Depends(get_current_user), session: Session = Depends(get_session)):
    p = session.get(Project, project_id)
    if not p:
        raise HTTPException(404, "Проект не найден")
    if p.owner_id != user.id and not user.is_admin:
        raise HTTPException(403, "Только владелец может редактировать проект")
    p.name = payload.name
    p.color = payload.color
    session.add(p)
    # синхронизировать участников
    current = session.exec(select(ProjectMember).where(ProjectMember.project_id == p.id)).all()
    current_ids = {m.user_id for m in current}
    new_ids = set(payload.member_ids) | {p.owner_id}
    for m in current:
        if m.user_id not in new_ids:
            session.delete(m)
    for uid in new_ids - current_ids:
        session.add(ProjectMember(project_id=p.id, user_id=uid, role="owner" if uid == p.owner_id else "member"))
    session.commit()
    session.refresh(p)
    return _to_out(session, p)


@router.delete("/{project_id}")
def delete_project(project_id: int, user: User = Depends(get_current_user), session: Session = Depends(get_session)):
    p = session.get(Project, project_id)
    if not p:
        raise HTTPException(404, "Проект не найден")
    if p.owner_id != user.id and not user.is_admin:
        raise HTTPException(403, "Только владелец может удалить проект")
    # членство
    for m in session.exec(select(ProjectMember).where(ProjectMember.project_id == p.id)).all():
        session.delete(m)
    session.delete(p)
    session.commit()
    return {"ok": True}
