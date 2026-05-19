// ─── Auth ────────────────────────────────────────────────────────────────────
export interface LoginPayload {
  username: string;
  password: string;
}

export interface TokenResponse {
  access_token: string;
  token_type: string;
}

export interface RegisterPayload {
  username: string;
  password: string;
  is_admin?: boolean;
}

// ─── User / Player ───────────────────────────────────────────────────────────
export interface User {
  id: number;
  username: string;
  is_admin: boolean;
}

// ─── Game Save ────────────────────────────────────────────────────────────────
export interface GameSave {
  save_data: string;      // JSON string
  updated_at: string;     // ISO datetime
}

// ─── Leaderboard ─────────────────────────────────────────────────────────────
export interface LeaderboardScore {
  id?: number;
  user_id?: number;
  completion_time: number;  // seconds
  created_at?: string;
  username?: string;        // joined from admin scores endpoint
}

export interface ScoreSubmission {
  completion_time: number;
}

// ─── Admin ───────────────────────────────────────────────────────────────────
export type AdminUsersResponse = User[];
export type AdminScoresResponse = LeaderboardScore[];

// ─── UI State ────────────────────────────────────────────────────────────────
export interface AlertMessage {
  type: 'success' | 'error' | 'warn' | 'info';
  text: string;
}

export interface ApiError {
  detail: string;
}

// ─── Dashboard Stats (derived) ───────────────────────────────────────────────
export interface DashboardStats {
  totalPlayers: number;
  adminCount: number;
  playerCount: number;
  totalScores: number;
  fastestTime: number | null;
  avgTime: number | null;
}
