from fastapi import APIRouter, Depends, HTTPException, status
from fastapi.security import OAuth2PasswordRequestForm, OAuth2PasswordBearer
from sqlmodel import Session, select
from app.database import get_session
from app.models.database import User
from app.core.security import get_password_hash, verify_password, create_access_token
from app.core.config import settings
from pydantic import BaseModel
from jose import jwt, JWTError

router = APIRouter()
reusable_oauth2 = OAuth2PasswordBearer(tokenUrl="auth/login")

class UserCreate(BaseModel):
    username: str
    password: str
    is_admin: bool = False

class Token(BaseModel):
    access_token: str
    token_type: str

@router.post("/register", response_model=Token)
def register(user_in: UserCreate, session: Session = Depends(get_session)):
    user = session.exec(select(User).where(User.username == user_in.username)).first()
    if user:
        raise HTTPException(status_code=400, detail="Username already registered")
    
    db_user = User(
        username=user_in.username,
        hashed_password=get_password_hash(user_in.password),
        is_admin=user_in.is_admin
    )
    session.add(db_user)
    session.commit()
    session.refresh(db_user)
    
    access_token = create_access_token(subject=db_user.username)
    return {"access_token": access_token, "token_type": "bearer"}

@router.post("/login", response_model=Token)
def login(form_data: OAuth2PasswordRequestForm = Depends(), session: Session = Depends(get_session)):
    user = session.exec(select(User).where(User.username == form_data.username)).first()
    if not user or not verify_password(form_data.password, user.hashed_password):
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Incorrect username or password")
    
    access_token = create_access_token(subject=user.username)
    return {"access_token": access_token, "token_type": "bearer"}

def get_current_user(session: Session = Depends(get_session), token: str = Depends(reusable_oauth2)) -> User:
    try:
        payload = jwt.decode(token, settings.SECRET_KEY, algorithms=[settings.ALGORITHM])
        username: str = payload.get("sub")
        if username is None:
            raise HTTPException(status_code=403, detail="Could not validate credentials")
    except JWTError:
        raise HTTPException(status_code=403, detail="Could not validate credentials")
    
    user = session.exec(select(User).where(User.username == username)).first()
    if not user:
        raise HTTPException(status_code=404, detail="User not found")
    return user

def get_current_admin_user(current_user: User = Depends(get_current_user)) -> User:
    if not current_user.is_admin:
        raise HTTPException(status_code=403, detail="Not enough permissions")
    return current_user
