import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Shield, Eye, EyeOff } from 'lucide-react';
import { useAuth } from '../contexts/AuthContext';
import { adminService } from '../services/api';
import { Alert, Spinner } from '../components/ui';
import { AlertMessage } from '../types';
import { getApiError } from '../utils/helpers';

export default function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [showPw, setShowPw] = useState(false);
  const [loading, setLoading] = useState(false);
  const [alert, setAlert] = useState<AlertMessage | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!username || !password) {
      setAlert({ type: 'warn', text: 'Please enter your credentials.' });
      return;
    }
    setLoading(true);
    setAlert(null);
    try {
      await login({ username, password });

      // Verify the user is actually an admin by fetching admin data
      try {
        await adminService.listUsers();
        // If this succeeds, they're an admin
        navigate('/', { replace: true });
      } catch {
        // Not an admin — reject
        setAlert({ type: 'error', text: 'Access denied. Admin privileges required.' });
        localStorage.removeItem('access_token');
        localStorage.removeItem('user_data');
      }
    } catch (err) {
      setAlert({ type: 'error', text: getApiError(err) });
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen grid-noise flex items-center justify-center p-4">
      <div className="mesh-bg" />

      <div className="w-full max-w-md">
        {/* Header */}
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-16 h-16 rounded-2xl bg-gradient-to-br from-purple-600 to-purple-900 shadow-lg shadow-purple-900/50 mb-4">
            <Shield size={28} className="text-white" />
          </div>
          <h1 className="font-display text-3xl font-bold text-white tracking-tight">
            GAME ADMIN
          </h1>
          <p className="text-sm text-purple-300/50 mt-1 font-mono">
            Authorized Personnel Only
          </p>
        </div>

        {/* Card */}
        <div className="glass rounded-2xl p-8 border border-purple-500/20">
          <h2 className="font-display font-semibold text-white text-lg mb-6">
            Sign in to Dashboard
          </h2>

          {alert && (
            <div className="mb-4">
              <Alert alert={alert} onClose={() => setAlert(null)} />
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-xs font-display font-semibold uppercase tracking-widest text-purple-400/70 mb-2">
                Username
              </label>
              <input
                type="text"
                value={username}
                onChange={e => setUsername(e.target.value)}
                placeholder="Enter your username"
                autoComplete="username"
                className="glass w-full rounded-xl px-4 py-3 text-purple-100 placeholder-purple-200/30 focus:outline-none focus:border-purple-500/60 transition-all duration-200 text-sm"
              />
            </div>

            <div>
              <label className="block text-xs font-display font-semibold uppercase tracking-widest text-purple-400/70 mb-2">
                Password
              </label>
              <div className="relative">
                <input
                  type={showPw ? 'text' : 'password'}
                  value={password}
                  onChange={e => setPassword(e.target.value)}
                  placeholder="Enter your password"
                  autoComplete="current-password"
                  className="glass w-full rounded-xl px-4 py-3 pr-11 text-purple-100 placeholder-purple-200/30 focus:outline-none focus:border-purple-500/60 transition-all duration-200 text-sm"
                />
                <button
                  type="button"
                  onClick={() => setShowPw(v => !v)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-purple-400/50 hover:text-purple-300 transition-colors"
                >
                  {showPw ? <EyeOff size={16} /> : <Eye size={16} />}
                </button>
              </div>
            </div>

            <button
              type="submit"
              disabled={loading}
              className="w-full bg-purple-600 hover:bg-purple-500 disabled:opacity-50 text-white font-display font-semibold py-3 rounded-xl transition-all duration-200 flex items-center justify-center gap-2 mt-2 shadow-lg shadow-purple-900/30"
            >
              {loading ? <><Spinner size={16} /> Authenticating…</> : 'Sign In'}
            </button>
          </form>

          <div className="mt-6 pt-5 border-t border-purple-500/10">
            <p className="text-xs text-purple-400/40 text-center font-mono">
              This panel requires administrator privileges
            </p>
          </div>
        </div>

        {/* Backend API indicator */}
        <div className="mt-4 text-center">
          <span className="text-xs text-purple-400/30 font-mono">
            API: {process.env.REACT_APP_API_URL || 'http://localhost:8000'}
          </span>
        </div>
      </div>
    </div>
  );
}
