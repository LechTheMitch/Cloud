from contextlib import asynccontextmanager
from fastapi import FastAPI
from app.api import auth, game, leaderboard, admin
from app.database import init_db
from app.core.config import settings


# 1. Define the lifespan manager
@asynccontextmanager
async def lifespan(app: FastAPI):
    # Everything before 'yield' runs on STARTUP
    try:
        # If init_db is synchronous, it's fine to call it here.
        # If it's asynchronous, use 'await init_db()'
        init_db()
    except Exception as e:
        print(f"Error initializing DB: {e}")

    yield  # The application serves requests while here

    # Everything after 'yield' runs on SHUTDOWN
    # (e.g., closing DB connections if needed)


# 2. Pass the lifespan to the FastAPI constructor
app = FastAPI(title=settings.PROJECT_NAME, lifespan=lifespan)

# Routers remain exactly the same
app.include_router(auth.router, prefix="/auth", tags=["auth"])
app.include_router(game.router, prefix="/game", tags=["game"])
app.include_router(leaderboard.router, prefix="/leaderboard", tags=["leaderboard"])
app.include_router(admin.router, prefix="/admin", tags=["admin"])


@app.get("/")
def root():
    return {"message": "Welcome to the Game Backend API"}

# if __name__ == "__main__":
#     import uvicorn
#     uvicorn.run(app, host="localhost", port=8000)