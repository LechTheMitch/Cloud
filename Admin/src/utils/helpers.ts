import { AxiosError } from 'axios';
import { ApiError } from '../types';

export function formatTime(seconds: number): string {
  const m = Math.floor(seconds / 60);
  const s = (seconds % 60).toFixed(2);
  return m > 0 ? `${m}m ${s}s` : `${Number(s).toFixed(2)}s`;
}

export function formatDate(iso: string): string {
  return new Date(iso).toLocaleString(undefined, {
    month: 'short', day: 'numeric', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  });
}

export function getApiError(err: unknown): string {
  if (err instanceof AxiosError) {
    const data = err.response?.data as ApiError | undefined;
    return data?.detail || err.message || 'An unknown error occurred';
  }
  if (err instanceof Error) return err.message;
  return 'An unknown error occurred';
}

export function getRankIcon(rank: number): string {
  if (rank === 1) return '🥇';
  if (rank === 2) return '🥈';
  if (rank === 3) return '🥉';
  return `#${rank}`;
}

export function truncate(str: string, maxLen = 40): string {
  return str.length > maxLen ? str.slice(0, maxLen) + '…' : str;
}

export function avatarInitials(username: string): string {
  return username.slice(0, 2).toUpperCase();
}

export function randomColor(seed: string): string {
  const colors = [
    '#8b5cf6', '#06b6d4', '#10b981', '#f59e0b', '#ef4444',
    '#a78bfa', '#22d3ee', '#34d399', '#fbbf24', '#f87171',
  ];
  let hash = 0;
  for (let i = 0; i < seed.length; i++) hash = seed.charCodeAt(i) + ((hash << 5) - hash);
  return colors[Math.abs(hash) % colors.length];
}
