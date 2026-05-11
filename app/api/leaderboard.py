from fastapi import APIRouter, Depends
from sqlmodel import Session, select, col
from app.database import get_session
from app.models.database import User, LeaderboardScore
from app.api.auth import get_current_user
from pydantic import BaseModel
from typing import List

router = APIRouter()

class ScoreIn(BaseModel):
    completion_time: float

class ScoreOut(BaseModel):
    username: str
    completion_time: float

@router.post("/submit")
def submit_score(score_in: ScoreIn, current_user: User = Depends(get_current_user), session: Session = Depends(get_session)):
    new_score = LeaderboardScore(user_id=current_user.id, completion_time=score_in.completion_time)
    session.add(new_score)
    session.commit()
    return {"status": "success"}

@router.get("/", response_model=List[ScoreOut])
def get_leaderboard(limit: int = 10, session: Session = Depends(get_session)):
    # Get top 10 fastest times (lowest first)
    # Join with User to get username
    statement = select(User.username, LeaderboardScore.completion_time)\
                .join(LeaderboardScore)\
                .order_by(col(LeaderboardScore.completion_time).asc())\
                .limit(limit)
    results = session.exec(statement).all()
    return [{"username": r.username, "completion_time": r.completion_time} for r in results]
