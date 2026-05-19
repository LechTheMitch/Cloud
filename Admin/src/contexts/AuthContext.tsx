import React, { createContext, useContext, useState, useEffect, useCallback, ReactNode } from 'react';
import { jwtDecode } from 'jwt-decode';
import { authService } from '../services/api';
import { LoginPayload, User } from '../types';

interface JwtPayload {
  sub: string;
  exp: number;
}

interface AuthContextType {
  user: User | null;
  token: string | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  login: (payload: LoginPayload) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | null>(null);

function decodeUserFromToken(token: string): Partial<User> {
  try {
    const payload = jwtDecode<JwtPayload>(token);
    return { username: payload.sub };
  } catch {
    return {};
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(() => localStorage.getItem('access_token'));
  const [user, setUser] = useState<User | null>(() => {
    const stored = localStorage.getItem('user_data');
    return stored ? JSON.parse(stored) : null;
  });
  const [isLoading, setIsLoading] = useState(false);

  const logout = useCallback(() => {
    setToken(null);
    setUser(null);
    localStorage.removeItem('access_token');
    localStorage.removeItem('user_data');
  }, []);

  // Validate token expiry on mount
  useEffect(() => {
    if (token) {
      try {
        const payload = jwtDecode<JwtPayload>(token);
        if (payload.exp * 1000 < Date.now()) {
          logout();
        }
      } catch {
        logout();
      }
    }
  }, [token, logout]);

  const login = useCallback(async (payload: LoginPayload) => {
    setIsLoading(true);
    try {
      const res = await authService.login(payload);
      const decoded = decodeUserFromToken(res.access_token);
      // We'll enrich user data after login from admin/users endpoint
      const userData: User = {
        id: 0,
        username: decoded.username || payload.username,
        is_admin: false, // will be updated once we fetch users list
      };
      setToken(res.access_token);
      setUser(userData);
      localStorage.setItem('access_token', res.access_token);
      localStorage.setItem('user_data', JSON.stringify(userData));
    } finally {
      setIsLoading(false);
    }
  }, []);

  const updateUser = useCallback((updated: User) => {
    setUser(updated);
    localStorage.setItem('user_data', JSON.stringify(updated));
  }, []);

  return (
    <AuthContext.Provider value={{
      user, token, isLoading,
      isAuthenticated: !!token,
      login, logout,
    }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used inside AuthProvider');
  return ctx;
}
