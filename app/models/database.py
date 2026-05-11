from typing import Optional, List
from sqlmodel import SQLModel, Field, Relationship, Column, ForeignKey, Integer
from datetime import datetime

class UserBase(SQLModel):
    username: str = Field(index=True, unique=True)
    is_admin: bool = Field(default=False)

class User(UserBase, table=True):
    id: Optional[int] = Field(default=None, primary_key=True)
    hashed_password: str
    
    saves: List["GameSave"] = Relationship(back_populates="user")
    scores: List["LeaderboardScore"] = Relationship(back_populates="user")
    progress: List["PlayerProgress"] = Relationship(back_populates="user")

class GameSave(SQLModel, table=True):
    id: Optional[int] = Field(default=None, primary_key=True)
    save_data: str # JSON string
    updated_at: datetime = Field(default_factory=datetime.utcnow)

    user_id: int = Field(foreign_key="user.id")
    user: User = Relationship(back_populates="saves")

class LeaderboardScore(SQLModel, table=True):
    id: Optional[int] = Field(default=None, primary_key=True)
    completion_time: float # In seconds
    created_at: datetime = Field(default_factory=datetime.utcnow)
    
    user_id: int = Field(foreign_key="user.id")
    user: User = Relationship(back_populates="scores")

class PlayerProgress(SQLModel, table=True):
    id: Optional[int] = Field(default=None, primary_key=True)
    user_id: int = Field(sa_column=Column(Integer, ForeignKey("user.id", ondelete="CASCADE")))
    
    current_loop: int = Field(default=0)
    total_loops_completed: int = Field(default=0)
    clues_found: int = Field(default=0)
    
    user: User = Relationship(back_populates="progress")
