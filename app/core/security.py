import bcrypt
from datetime import datetime, timedelta
from typing import Any, Union
from jose import jwt
from app.core.config import settings


# Token Stuff

def create_access_token(subject: Union[str, Any], expires_delta: timedelta = None) -> str:
    if expires_delta:
        expire = datetime.utcnow() + expires_delta
    else:
        expire = datetime.utcnow() + timedelta(minutes=settings.ACCESS_TOKEN_EXPIRE_MINUTES)
    to_encode = {"exp": expire, "sub": str(subject)}
    encoded_jwt = jwt.encode(to_encode, settings.SECRET_KEY, algorithm=settings.ALGORITHM)
    return encoded_jwt


# Hashing using BCrypt

def get_password_hash(password: str) -> str:
    # 1. Convert to bytes
    pwd_bytes = password.encode('utf-8')

    # 2. Handle the 72-byte limit (prevents the ValueError you saw)
    if len(pwd_bytes) > 72:
        pwd_bytes = pwd_bytes[:72]

    # 3. Generate salt and hash
    salt = bcrypt.gensalt()
    hashed = bcrypt.hashpw(pwd_bytes, salt)

    # 4. Return as string for DB storage
    return hashed.decode('utf-8')


def verify_password(plain_password: str, hashed_password: str) -> bool:
    # 1. Convert inputs to bytes
    password_bytes = plain_password.encode('utf-8')
    if len(password_bytes) > 72:
        password_bytes = password_bytes[:72]

    hashed_bytes = hashed_password.encode('utf-8')

    # 2. Use bcrypt's native check function
    try:
        return bcrypt.checkpw(password_bytes, hashed_bytes)
    except Exception:
        return False