# Local Development Guide

This guide provides instructions on how to set up and run the Game project locally.

## Prerequisites

- **Docker & Docker Compose** (Recommended for Database and Backend)
- **Node.js (v16+) & npm** (For Admin Dashboard)
- **Python 3.9+** (If running Backend without Docker)
- **Unity Editor (2022.3+)** (For the Game)

---

## 1. Backend & Database Setup

The backend is built with FastAPI and uses MySQL as the database.

### Option A: Using Docker (Recommended)

This is the fastest way to get the backend and database running.

1.  Make sure you are in the root directory.
2.  Run the following command:
    ```bash
    docker-compose up -d
    ```
    This will start:
    - **MySQL Database** on `localhost:3306`
    - **FastAPI Backend** on `http://localhost:8000`

### Option B: Manual Setup

If you prefer to run the backend manually:

1.  **Setup MySQL:**
    - Ensure you have a MySQL server running.
    - Create a database named `game_db`.
2.  **Environment Variables:**
    - Create a `.env` file in the root directory:
      ```env
      MYSQL_SERVER=localhost
      MYSQL_USER=your_user
      MYSQL_PASSWORD=your_password
      MYSQL_DB=game_db
      SECRET_KEY=your_secret_key
      ```
3.  **Install Dependencies:**
    ```bash
    pip install -r requirements.txt
    ```
4.  **Run Backend:**
    ```bash
    uvicorn app.main:app --reload
    ```
    The API will be available at `http://localhost:8000`. Documentation can be found at `http://localhost:8000/docs`.

---

## 2. Admin Dashboard Setup

The admin dashboard is a React application located in the `Admin/` directory.

1.  Navigate to the Admin directory:
    ```bash
    cd Admin
    ```
2.  Install dependencies:
    ```bash
    npm install
    ```
3.  Configure environment:
    Create a `.env.local` file:
    ```bash
    echo "REACT_APP_API_URL=http://localhost:8000" > .env.local
    ```
4.  Start the dashboard:
    ```bash
    npm start
    ```
    The dashboard will open at `http://localhost:3000`.

---

## 3. Unity Game Setup

The game client is located in the `Game/` directory.

1.  Open **Unity Hub**.
2.  Add and open the `Game` folder as a project.
3.  **Update API URL:**
    - In the Project window, find: `Assets/_Scripts/Data/ApiConfig.cs`.
    - Change the `BaseURL` to your local backend:
      ```csharp
      public const string BaseURL = "http://localhost:8000";
      ```
4.  Open the `MainMenu` scene in `Assets/_Scenes/`.
5.  Press **Play** to start the game.

---

## Troubleshooting

- **Database Connection:** If the backend fails to start, ensure the MySQL credentials in `.env` or `docker-compose.yml` match.
- **CORS Errors:** The backend is configured to allow all origins in development, but ensure the Admin Dashboard is pointing to the correct `REACT_APP_API_URL`.
- **Unity Errors:** Ensure you have the required packages installed in Unity (Input System, TextMeshPro, etc. should be automatically handled by the manifest).
