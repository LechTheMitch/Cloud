import axios, { AxiosError, AxiosInstance } from 'axios';
import {
  LoginPayload,
  TokenResponse,
  RegisterPayload,
  User,
  GameSave,
  LeaderboardScore,
  ScoreSubmission,
  AdminUsersResponse,
  AdminScoresResponse,
} from '../types';

// Base URL — change to match your deployed backend
const BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:8000';

// ─── Axios instance ──────────────────────────────────────────────────────────
const api: AxiosInstance = axios.create({
  baseURL: BASE_URL,
  headers: { 'Content-Type': 'application/json' },
});

// Attach JWT token from localStorage on every request
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('access_token');
  if (token) {
    config.headers['Authorization'] = `Bearer ${token}`;
  }
  return config;
});

// Redirect to login on 401/403
api.interceptors.response.use(
  (res) => res,
  (err: AxiosError) => {
    if (err.response?.status === 401 || err.response?.status === 403) {
      const isLoginRoute = err.config?.url?.includes('/auth/login');
      if (!isLoginRoute) {
        localStorage.removeItem('access_token');
        window.location.href = '/login';
      }
    }
    return Promise.reject(err);
  }
);

// ─── Auth ─────────────────────────────────────────────────────────────────────
export const authService = {
  /**
   * POST /auth/login — OAuth2PasswordRequestForm (form-urlencoded)
   */
  login: async (payload: LoginPayload): Promise<TokenResponse> => {
    const formData = new URLSearchParams();
    formData.append('username', payload.username);
    formData.append('password', payload.password);

    const res = await api.post<TokenResponse>('/auth/login', formData.toString(), {
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    });
    return res.data;
  },

  /**
   * POST /auth/register — JSON body
   */
  register: async (payload: RegisterPayload): Promise<TokenResponse> => {
    const res = await api.post<TokenResponse>('/auth/register', payload);
    return res.data;
  },
};

// ─── Game ─────────────────────────────────────────────────────────────────────
export const gameService = {
  /**
   * GET /game/save — Get current user's save
   */
  getSave: async (): Promise<GameSave> => {
    const res = await api.get<GameSave>('/game/save');
    return res.data;
  },

  /**
   * POST /game/save — Create or update save
   */
  updateSave: async (saveData: string): Promise<{ status: string }> => {
    const res = await api.post('/game/save', { save_data: saveData });
    return res.data;
  },
};

// ─── Leaderboard ──────────────────────────────────────────────────────────────
export const leaderboardService = {
  /**
   * GET /leaderboard/?limit=N — Public leaderboard
   */
  getLeaderboard: async (limit = 10): Promise<LeaderboardScore[]> => {
    const res = await api.get<LeaderboardScore[]>(`/leaderboard/?limit=${limit}`);
    return res.data;
  },

  /**
   * POST /leaderboard/submit — Submit a score
   */
  submitScore: async (payload: ScoreSubmission): Promise<{ status: string }> => {
    const res = await api.post('/leaderboard/submit', payload);
    return res.data;
  },
};

// ─── Admin ────────────────────────────────────────────────────────────────────
export const adminService = {
  /**
   * GET /admin/users — All users (admin only)
   */
  listUsers: async (): Promise<AdminUsersResponse> => {
    const res = await api.get<AdminUsersResponse>('/admin/users');
    return res.data;
  },

  /**
   * DELETE /admin/users/{user_id} — Delete a user (admin only)
   */
  deleteUser: async (userId: number): Promise<{ status: string }> => {
    const res = await api.delete(`/admin/users/${userId}`);
    return res.data;
  },

  /**
   * GET /admin/scores — All scores (admin only)
   */
  listAllScores: async (): Promise<AdminScoresResponse> => {
    const res = await api.get<AdminScoresResponse>('/admin/scores');
    return res.data;
  },
};

export default api;
