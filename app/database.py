from sqlmodel import create_engine, SQLModel, Session
from app.core.config import settings
# Import models here to ensure they are registered with SQLModel.metadata
from app.models.database import User, GameSave, LeaderboardScore, PlayerProgress

engine = create_engine(settings.DATABASE_URL, echo=True)

def init_db():
    print("DEBUG: Starting init_db() - Creating tables...")
    try:
        SQLModel.metadata.create_all(engine)
        print("DEBUG: init_db() finished successfully.")
    except Exception as e:
        print(f"DEBUG: Error in init_db(): {e}")
        raise e

def get_session():
    with Session(engine) as session:
        yield session
