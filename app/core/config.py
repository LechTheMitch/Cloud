from pydantic_settings import BaseSettings
from typing import Optional

class Settings(BaseSettings):
    PROJECT_NAME: str = "Game Backend"
    SECRET_KEY: str = "supersecretkey" # Change in production!
    ALGORITHM: str = "HS256"
    ACCESS_TOKEN_EXPIRE_MINUTES: int = 30
    
    # Database Settings
    MYSQL_USER: str = "root"
    MYSQL_PASSWORD: str = "rootpassword"
    MYSQL_SERVER: str = "database-1.cboqq8qwo4lj.eu-north-1.rds.amazonaws.com"
    MYSQL_PORT: int = 3306
    MYSQL_DB: str = "game_db"
    
    @property
    def DATABASE_URL(self) -> str:
        return f"mysql+pymysql://{self.MYSQL_USER}:{self.MYSQL_PASSWORD}@{self.MYSQL_SERVER}:{self.MYSQL_PORT}/{self.MYSQL_DB}"

    class Config:
        env_file = ".env"

settings = Settings()
