import React, { useEffect, useState } from 'react';
import { RefreshCw, Database, Clock, User2, Search } from 'lucide-react';
import { adminService, gameService } from '../services/api';
import { User, GameSave } from '../types';
import { Spinner, Card, PageHeader, Alert, EmptyState } from '../components/ui';
import { formatDate, getApiError } from '../utils/helpers';
import { AlertMessage } from '../types';

interface UserWithSave {
  user: User;
  save: GameSave | null;
  loading: boolean;
  error: string | null;
}

export default function GameDataPage() {
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [alert, setAlert] = useState<AlertMessage | null>(null);
  const [userSaves, setUserSaves] = useState<UserWithSave[]>([]);
  const [expandedUser, setExpandedUser] = useState<number | null>(null);
  const [mySave, setMySave] = useState<GameSave | null>(null);
  const [mySaveLoading, setMySaveLoading] = useState(false);

  const fetchUsers = async () => {
    setLoading(true);
    try {
      const u = await adminService.listUsers();
      setUsers(u);
      setUserSaves(u.map(user => ({ user, save: null, loading: false, error: null })));
    } catch (e) {
      setAlert({ type: 'error', text: getApiError(e) });
    } finally {
      setLoading(false);
    }
  };

  const fetchMySave = async () => {
    setMySaveLoading(true);
    try {
      const save = await gameService.getSave();
      setMySave(save);
    } catch (e) {
      setMySave(null);
    } finally {
      setMySaveLoading(false);
    }
  };

  useEffect(() => {
    fetchUsers();
    fetchMySave();
  }, []);

  // Parse save_data JSON safely
  const parseSave = (raw: string) => {
    try {
      return JSON.stringify(JSON.parse(raw), null, 2);
    } catch {
      return raw;
    }
  };

  const toggleUser = (id: number) => {
    setExpandedUser(prev => prev === id ? null : id);
  };

  const adminCount = users.filter(u => u.is_admin).length;
  const playerCount = users.filter(u => !u.is_admin).length;

  return (
    <div className="fade-in">
      <PageHeader
        title="Game Data"
        subtitle="Player accounts, save data & system information"
        action={
          <button onClick={fetchUsers} className="flex items-center gap-2 px-4 py-2.5 glass rounded-xl text-sm font-display font-medium text-purple-300 hover:text-white transition-all">
            <RefreshCw size={14} /> Refresh
          </button>
        }
      />

      {alert && <div className="mb-4"><Alert alert={alert} onClose={() => setAlert(null)} /></div>}

      {/* Your Save Data */}
      <Card className="p-5 mb-6">
        <div className="flex items-center justify-between mb-4">
          <div className="flex items-center gap-3">
            <div className="p-2 rounded-xl bg-cyan-500/15">
              <Database size={16} className="text-cyan-400" />
            </div>
            <div>
              <h3 className="font-display font-bold text-white">Your Save Data</h3>
              <p className="text-xs text-purple-300/40">Current admin session save</p>
            </div>
          </div>
          <button
            onClick={fetchMySave}
            className="flex items-center gap-1.5 text-xs text-purple-400/50 hover:text-purple-300 transition-colors"
          >
            <RefreshCw size={12} />
          </button>
        </div>

        {mySaveLoading ? (
          <div className="flex items-center gap-2 text-sm text-purple-400/50 py-2">
            <Spinner size={14} /> Loading…
          </div>
        ) : mySave ? (
          <div>
            <div className="flex items-center gap-2 mb-3">
              <Clock size={12} className="text-purple-400/50" />
              <span className="text-xs text-purple-400/50">Last updated: {formatDate(mySave.updated_at)}</span>
            </div>
            <div className="bg-black/30 rounded-xl p-4 border border-purple-500/10 overflow-auto max-h-40">
              <pre className="font-mono text-xs text-green-400/80 whitespace-pre-wrap">
                {parseSave(mySave.save_data)}
              </pre>
            </div>
          </div>
        ) : (
          <p className="text-sm text-purple-400/30">No save data found for your account.</p>
        )}
      </Card>

      {/* User list */}
      <div className="flex items-center justify-between mb-4">
        <h3 className="font-display font-bold text-white">All Player Accounts</h3>
        <div className="flex gap-2 text-xs font-display">
          <span className="glass px-3 py-1 rounded-lg text-purple-300/50">{playerCount} players</span>
          <span className="glass px-3 py-1 rounded-lg text-cyan-400/70">{adminCount} admins</span>
        </div>
      </div>

      {loading ? (
        <div className="flex items-center justify-center py-16"><Spinner size={28} /></div>
      ) : users.length === 0 ? (
        <EmptyState icon="👤" message="No users found" />
      ) : (
        <div className="space-y-2">
          {users.map(user => (
            <div key={user.id} className="glass rounded-xl border border-purple-500/10 overflow-hidden transition-all">
              <button
                onClick={() => toggleUser(user.id)}
                className="w-full flex items-center gap-4 px-5 py-3.5 hover:bg-purple-800/10 transition-colors text-left"
              >
                <div className="w-8 h-8 rounded-lg flex items-center justify-center bg-purple-700/30 text-purple-300 text-xs font-display font-bold">
                  {user.username.slice(0, 2).toUpperCase()}
                </div>
                <div className="flex-1">
                  <p className="text-sm font-display font-semibold text-white">
                    {user.username}
                  </p>
                  <p className="text-xs text-purple-400/40 font-mono">ID #{user.id}</p>
                </div>
                <span className={`text-xs font-display font-semibold px-2.5 py-0.5 rounded-full ${
                  user.is_admin
                    ? 'bg-purple-500/20 text-purple-400'
                    : 'bg-cyan-500/15 text-cyan-400'
                }`}>
                  {user.is_admin ? '🛡 Admin' : '🎮 Player'}
                </span>
                <span className={`text-purple-400/30 transition-transform ${expandedUser === user.id ? 'rotate-180' : ''}`}>
                  ▼
                </span>
              </button>

              {expandedUser === user.id && (
                <div className="px-5 pb-5 pt-1 border-t border-purple-500/10 bg-black/20">
                  <div className="grid grid-cols-2 gap-4 text-xs font-mono mb-4">
                    <div>
                      <p className="text-purple-400/40 uppercase tracking-wider mb-1">User ID</p>
                      <p className="text-white">#{user.id}</p>
                    </div>
                    <div>
                      <p className="text-purple-400/40 uppercase tracking-wider mb-1">Role</p>
                      <p className="text-white">{user.is_admin ? 'Administrator' : 'Player'}</p>
                    </div>
                    <div>
                      <p className="text-purple-400/40 uppercase tracking-wider mb-1">Username</p>
                      <p className="text-white">{user.username}</p>
                    </div>
                  </div>
                  <div className="rounded-xl bg-black/30 border border-purple-500/10 p-3">
                    <p className="text-xs text-purple-400/40 uppercase tracking-wider mb-2 font-display">
                      ℹ️ Save Data Access
                    </p>
                    <p className="text-xs text-purple-300/50">
                      The backend API only allows viewing your own save data (<code className="font-mono text-purple-400/70">GET /game/save</code>).
                      Admin-level save data access for other users would require a backend endpoint extension.
                    </p>
                  </div>
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {/* API Capabilities note */}
      <div className="mt-6 glass rounded-xl p-5 border border-purple-500/15">
        <h4 className="font-display font-bold text-white text-sm mb-3">📡 Available Game Endpoints</h4>
        <div className="space-y-2 font-mono text-xs">
          {[
            { method: 'GET',    path: '/game/save',             auth: 'User',  desc: 'Get your own save data' },
            { method: 'POST',   path: '/game/save',             auth: 'User',  desc: 'Create or update save' },
            { method: 'GET',    path: '/leaderboard/',          auth: 'Public',desc: 'Top scores leaderboard' },
            { method: 'POST',   path: '/leaderboard/submit',    auth: 'User',  desc: 'Submit a score' },
            { method: 'GET',    path: '/admin/users',           auth: 'Admin', desc: 'List all users' },
            { method: 'DELETE', path: '/admin/users/{id}',      auth: 'Admin', desc: 'Delete a user' },
            { method: 'GET',    path: '/admin/scores',          auth: 'Admin', desc: 'All score entries' },
          ].map(({ method, path, auth, desc }) => (
            <div key={path} className="flex items-center gap-3 py-1.5 border-b border-purple-500/5 last:border-0">
              <span className={`w-14 text-center px-1 py-0.5 rounded text-xs font-bold ${
                method === 'GET' ? 'bg-green-500/20 text-green-400' :
                method === 'POST' ? 'bg-blue-500/20 text-blue-400' :
                method === 'DELETE' ? 'bg-red-500/20 text-red-400' :
                'bg-gray-500/20 text-gray-400'
              }`}>{method}</span>
              <span className="text-purple-300/70 flex-1">{path}</span>
              <span className={`text-xs px-2 py-0.5 rounded-full ${
                auth === 'Admin' ? 'bg-purple-500/15 text-purple-400' :
                auth === 'User' ? 'bg-cyan-500/15 text-cyan-400' :
                'bg-gray-500/15 text-gray-400'
              }`}>{auth}</span>
              <span className="text-purple-400/30 hidden sm:block">{desc}</span>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
