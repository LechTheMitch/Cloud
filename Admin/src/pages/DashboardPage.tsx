import React, { useEffect, useState } from 'react';
import { Users, Trophy, Shield, Zap, TrendingUp, Clock } from 'lucide-react';
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid } from 'recharts';
import { adminService, leaderboardService } from '../services/api';
import { User, LeaderboardScore, DashboardStats } from '../types';
import { Spinner, Card, PageHeader, Avatar, Badge } from '../components/ui';
import { formatTime, formatDate, getApiError } from '../utils/helpers';

export default function DashboardPage() {
  const [users, setUsers] = useState<User[]>([]);
  const [scores, setScores] = useState<LeaderboardScore[]>([]);
  const [leaderboard, setLeaderboard] = useState<LeaderboardScore[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const load = async () => {
      try {
        const [u, s, lb] = await Promise.all([
          adminService.listUsers(),
          adminService.listAllScores(),
          leaderboardService.getLeaderboard(10),
        ]);
        setUsers(u);
        setScores(s);
        setLeaderboard(lb);
      } catch (e) {
        setError(getApiError(e));
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  const stats: DashboardStats = {
    totalPlayers: users.length,
    adminCount: users.filter(u => u.is_admin).length,
    playerCount: users.filter(u => !u.is_admin).length,
    totalScores: scores.length,
    fastestTime: scores.length ? Math.min(...scores.map(s => s.completion_time)) : null,
    avgTime: scores.length ? scores.reduce((a, b) => a + b.completion_time, 0) / scores.length : null,
  };

  // Chart data: top 10 players by score count
  const playerScoreCounts = users.map(u => ({
    name: u.username.slice(0, 10),
    runs: scores.filter(s => s.user_id === u.id).length,
  })).sort((a, b) => b.runs - a.runs).slice(0, 8);

  // Recent users
  const recentUsers = [...users].slice(-5).reverse();

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <Spinner size={32} />
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex flex-col items-center justify-center h-64 text-red-400 gap-2">
        <p className="text-lg font-display font-bold">Failed to load dashboard</p>
        <p className="text-sm opacity-70">{error}</p>
      </div>
    );
  }

  const statCards = [
    {
      label: 'Total Players',
      value: stats.totalPlayers,
      icon: Users,
      color: 'from-purple-600/20 to-purple-800/10',
      border: 'border-purple-500/20',
      iconColor: 'text-purple-400',
    },
    {
      label: 'Admin Accounts',
      value: stats.adminCount,
      icon: Shield,
      color: 'from-cyan-600/20 to-cyan-800/10',
      border: 'border-cyan-500/20',
      iconColor: 'text-cyan-400',
    },
    {
      label: 'Total Score Entries',
      value: stats.totalScores,
      icon: Trophy,
      color: 'from-amber-600/20 to-amber-800/10',
      border: 'border-amber-500/20',
      iconColor: 'text-amber-400',
    },
    {
      label: 'Fastest Time',
      value: stats.fastestTime !== null ? formatTime(stats.fastestTime) : '—',
      icon: Zap,
      color: 'from-green-600/20 to-green-800/10',
      border: 'border-green-500/20',
      iconColor: 'text-green-400',
    },
    {
      label: 'Regular Players',
      value: stats.playerCount,
      icon: TrendingUp,
      color: 'from-blue-600/20 to-blue-800/10',
      border: 'border-blue-500/20',
      iconColor: 'text-blue-400',
    },
    {
      label: 'Avg Completion',
      value: stats.avgTime !== null ? formatTime(stats.avgTime) : '—',
      icon: Clock,
      color: 'from-rose-600/20 to-rose-800/10',
      border: 'border-rose-500/20',
      iconColor: 'text-rose-400',
    },
  ];

  return (
    <div className="fade-in">
      <PageHeader
        title="Dashboard"
        subtitle={`Overview of your game platform · ${new Date().toLocaleDateString(undefined, { weekday: 'long', month: 'long', day: 'numeric' })}`}
      />

      {/* Stat Cards */}
      <div className="grid grid-cols-2 lg:grid-cols-3 gap-4 mb-8">
        {statCards.map(({ label, value, icon: Icon, color, border, iconColor }) => (
          <div
            key={label}
            className={`stat-card bg-gradient-to-br ${color} border ${border}`}
          >
            <div className="flex items-start justify-between">
              <div>
                <p className="text-xs font-display font-semibold uppercase tracking-widest text-purple-300/50 mb-2">
                  {label}
                </p>
                <p className="font-display font-bold text-2xl text-white">
                  {value}
                </p>
              </div>
              <div className={`p-2.5 rounded-xl bg-black/20 ${iconColor}`}>
                <Icon size={18} />
              </div>
            </div>
          </div>
        ))}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Bar Chart */}
        <div className="lg:col-span-2">
          <Card className="p-5">
            <div className="flex items-center justify-between mb-5">
              <div>
                <h3 className="font-display font-bold text-white text-base">Runs per Player</h3>
                <p className="text-xs text-purple-300/40 mt-0.5">Score submission frequency</p>
              </div>
            </div>
            {playerScoreCounts.some(p => p.runs > 0) ? (
              <ResponsiveContainer width="100%" height={220}>
                <BarChart data={playerScoreCounts} margin={{ top: 0, right: 0, left: -20, bottom: 0 }}>
                  <CartesianGrid strokeDasharray="3 3" stroke="rgba(139,92,246,0.08)" />
                  <XAxis
                    dataKey="name"
                    tick={{ fill: 'rgba(167,139,250,0.5)', fontSize: 11, fontFamily: 'Syne' }}
                    axisLine={false}
                    tickLine={false}
                  />
                  <YAxis
                    tick={{ fill: 'rgba(167,139,250,0.5)', fontSize: 11 }}
                    axisLine={false}
                    tickLine={false}
                    allowDecimals={false}
                  />
                  <Tooltip
                    contentStyle={{
                      background: 'rgba(15,13,31,0.95)',
                      border: '1px solid rgba(139,92,246,0.3)',
                      borderRadius: '12px',
                      fontSize: '12px',
                      fontFamily: 'Syne',
                      color: '#e4e0ff',
                    }}
                    cursor={{ fill: 'rgba(139,92,246,0.08)' }}
                  />
                  <Bar dataKey="runs" fill="url(#barGrad)" radius={[6, 6, 0, 0]} maxBarSize={40} />
                  <defs>
                    <linearGradient id="barGrad" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="0%" stopColor="#8b5cf6" />
                      <stop offset="100%" stopColor="#7c3aed" stopOpacity="0.5" />
                    </linearGradient>
                  </defs>
                </BarChart>
              </ResponsiveContainer>
            ) : (
              <div className="h-48 flex items-center justify-center text-purple-300/30 text-sm font-display">
                No score data available
              </div>
            )}
          </Card>
        </div>

        {/* Recent Users */}
        <div>
          <Card className="p-5 h-full">
            <h3 className="font-display font-bold text-white text-base mb-4">Recent Players</h3>
            <div className="space-y-3">
              {recentUsers.length > 0 ? recentUsers.map(user => (
                <div key={user.id} className="flex items-center gap-3 p-2.5 rounded-xl hover:bg-purple-800/20 transition-colors">
                  <Avatar username={user.username} size={32} />
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-display font-semibold text-white truncate">{user.username}</p>
                    <p className="text-xs text-purple-300/40">ID #{user.id}</p>
                  </div>
                  <Badge variant={user.is_admin ? 'admin' : 'player'}>
                    {user.is_admin ? 'Admin' : 'Player'}
                  </Badge>
                </div>
              )) : (
                <p className="text-sm text-purple-300/30 text-center py-4">No players yet</p>
              )}
            </div>
          </Card>
        </div>
      </div>

      {/* Leaderboard Preview */}
      {leaderboard.length > 0 && (
        <div className="mt-6">
          <Card className="p-5">
            <div className="flex items-center justify-between mb-4">
              <h3 className="font-display font-bold text-white text-base">🏆 Top Times</h3>
              <span className="text-xs text-purple-300/40 font-mono">Fastest completions</span>
            </div>
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-3">
              {leaderboard.slice(0, 5).map((entry, i) => (
                <div key={i} className={`p-3 rounded-xl border transition-all ${
                  i === 0 ? 'bg-amber-500/10 border-amber-500/25' :
                  i === 1 ? 'bg-zinc-400/10 border-zinc-400/20' :
                  i === 2 ? 'bg-orange-700/10 border-orange-700/20' :
                  'bg-purple-800/10 border-purple-500/10'
                }`}>
                  <p className="text-xl mb-1">
                    {i === 0 ? '🥇' : i === 1 ? '🥈' : i === 2 ? '🥉' : `#${i + 1}`}
                  </p>
                  <p className="font-display font-bold text-white text-sm truncate">{entry.username}</p>
                  <p className="font-mono text-xs text-purple-300/60 mt-0.5">{formatTime(entry.completion_time)}</p>
                </div>
              ))}
            </div>
          </Card>
        </div>
      )}
    </div>
  );
}
