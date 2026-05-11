from fastapi import APIRouter, Depends, HTTPException
from sqlmodel import Session, select
from app.database import get_session
from app.models.database import User, GameSave
from app.api.auth import get_current_user
from pydantic import BaseModel

router = APIRouter()

class SaveUpdate(BaseModel):
    save_data: str # JSON string

@router.post("/save")
def create_or_update_save(save_in: SaveUpdate, current_user: User = Depends(get_current_user), session: Session = Depends(get_session)):
    # Check if a save already exists for this user
    save = session.exec(select(GameSave).where(GameSave.user_id == current_user.id)).first()
    if save:
        save.save_data = save_in.save_data
    else:
        save = GameSave(user_id=current_user.id, save_data=save_in.save_data)
        session.add(save)
    
    session.commit()
    return {"status": "success"}

@router.get("/save")
def get_save(current_user: User = Depends(get_current_user), session: Session = Depends(get_session)):
    save = session.exec(select(GameSave).where(GameSave.user_id == current_user.id)).first()
    if not save:
        raise HTTPException(status_code=404, detail="No save found")
    return {"save_data": save.save_data, "updated_at": save.updated_at}
