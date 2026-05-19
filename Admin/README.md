# Game Admin Dashboard

A production-ready React admin panel for the Cloud Game Backend API.

## Backend Analysis

### Endpoints
| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | /auth/register | Public | Register new user |
| POST | /auth/login | Public | Login (form-urlencoded → JWT) |
| GET | /game/save | User | Get own save data |
| POST | /game/save | User | Create/update save data |
| GET | /leaderboard/ | Public | Top completion times |
| POST | /leaderboard/submit | User | Submit a score |
| GET | /admin/users | Admin | List all users |
| DELETE | /admin/users/{id} | Admin | Delete a user |
| GET | /admin/scores | Admin | All score entries |

### Missing Backend Features (not implemented in API)
- Suspend/ban players
- Role editing
- Password reset
- Per-player save data access for admins
- Moderation notes/warnings
- Activity logs
- Real-time notifications

## Quick Start

```bash
npm install
echo "REACT_APP_API_URL=http://localhost:8000" > .env.local
npm start
```

## Project Structure

```
src/
  types/index.ts          - TypeScript interfaces
  services/api.ts         - Axios service layer
  contexts/AuthContext.tsx - JWT auth state
  utils/helpers.ts        - Utility functions
  components/ui.tsx       - Reusable UI components
  components/ProtectedRoute.tsx
  layouts/AdminLayout.tsx  - Sidebar navigation
  pages/
    LoginPage.tsx
    DashboardPage.tsx     - Stats + charts
    PlayersPage.tsx       - User management
    ScoresPage.tsx        - Leaderboard + scores
    GameDataPage.tsx      - Save data viewer
```

## Authentication

Uses JWT Bearer tokens stored in localStorage.
Admin access verified post-login via GET /admin/users.

## Tech Stack

React 18, TypeScript, Tailwind CSS, Axios, Recharts, React Router v6, jwt-decode, Lucide React
