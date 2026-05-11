from fastapi import APIRouter, Depends, HTTPException
from sqlmodel import Session, select
from app.database import get_session
from app.models.database import User, LeaderboardScore
from app.api.auth import get_current_admin_user
from typing import List

router = APIRouter()

@router.get("/users")
def list_users(current_admin: User = Depends(get_current_admin_user), session: Session = Depends(get_session)):
    users = session.exec(select(User)).all()
    return users

@router.delete("/users/{user_id}")
def delete_user(user_id: int, current_admin: User = Depends(get_current_admin_user), session: Session = Depends(get_session)):
    user = session.get(User, user_id)
    if not user:
        raise HTTPException(status_code=404, detail="User not found")
    session.delete(user)
    session.commit()
    return {"status": "user deleted"}

@router.get("/scores")
def list_all_scores(current_admin: User = Depends(get_current_admin_user), session: Session = Depends(get_session)):
    scores = session.exec(select(LeaderboardScore)).all()
    return scores
