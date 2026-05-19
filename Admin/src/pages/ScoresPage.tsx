import React, { useEffect, useState, useMemo } from 'react';
import { RefreshCw, Trophy, Clock, TrendingDown, BarChart2, ChevronUp, ChevronDown } from 'lucide-react';
import { AreaChart, Area, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid } from 'recharts';
import { adminService, leaderboardService } from '../services/api';
import { LeaderboardScore, User } from '../types';
import { Spinner, Card, PageHeader, EmptyState, SearchInput, Alert } from '../components/ui';
import { formatTime, formatDate, getApiError } from '../utils/helpers';
import { AlertMessage } from '../types';

type SortKey = 'rank' | 'username' | 'completion_time';

export default function ScoresPage() {
  const [scores, setScores] = useState<LeaderboardScore[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [leaderboard, setLeaderboard] = useState<LeaderboardScore[]>([]);
  const [loading, setLoading] = useState(true);
  const [alert, setAlert] = useState<AlertMessage | null>(null);
  const [search, setSearch] = useState('');
  const [sortKey, setSortKey] = useState<SortKey>('completion_time');
  const [sortDir, setSortDir] = useState<'asc' | 'desc'>('asc');

  const fetchData = async () => {
    setLoading(true);
    try {
      const [s, u, lb] = await Promise.all([
        adminService.listAllScores(),
        adminService.listUsers(),
        leaderboardService.getLeaderboard(50),
      ]);
      setScores(s);
      setUsers(u);
      setLeaderboard(lb);
    } catch (e) {
      setAlert({ type: 'error', text: getApiError(e) });
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchData(); }, []);

  // Enrich scores with username
  const enriched = useMemo(() => {
    return scores.map(s => ({
      ...s,
      username: users.find(u => u.id === s.user_id)?.username || `User #${s.user_id}`,
    }));
  }, [scores, users]);

  const filtered = useMemo(() => {
    let list = [...enriched];
    if (search) list = list.filter(s => s.username?.toLowerCase().includes(search.toLowerCase()));
    list.sort((a, b) => {
      const av = sortKey === 'username' ? (a.username || '') : (sortKey === 'completion_time' ? a.completion_time : (a.id || 0));
      const bv = sortKey === 'username' ? (b.username || '') : (sortKey === 'completion_time' ? b.completion_time : (b.id || 0));
      if (typeof av === 'string') return sortDir === 'asc' ? av.localeCompare(bv as string) : (bv as string).localeCompare(av);
      return sortDir === 'asc' ? (av as number) - (bv as number) : (bv as number) - (av as number);
    });
    return list;
  }, [enriched, search, sortKey, sortDir]);

  const toggleSort = (key: SortKey) => {
    if (sortKey === key) setSortDir(d => d === 'asc' ? 'desc' : 'asc');
    else { setSortKey(key); setSortDir('asc'); }
  };

  const SortIcon = ({ col }: { col: SortKey }) => {
    if (sortKey !== col) return <ChevronUp size={12} className="opacity-20" />;
    return sortDir === 'asc' ? <ChevronUp size={12} className="text-purple-400" /> : <ChevronDown size={12} className="text-purple-400" />;
  };

  const TH = ({ col, label }: { col: SortKey; label: string }) => (
    <th className="px-4 py-3 text-left cursor-pointer select-none group" onClick={() => toggleSort(col)}>
      <span className="flex items-center gap-1 text-xs font-display font-semibold uppercase tracking-widest text-purple-400/60 group-hover:text-purple-300">
        {label} <SortIcon col={col} />
      </span>
    </th>
  );

  const fastest = scores.length ? Math.min(...scores.map(s => s.completion_time)) : null;
  const slowest = scores.length ? Math.max(...scores.map(s => s.completion_time)) : null;
  const avg = scores.length ? scores.reduce((a, b) => a + b.completion_time, 0) / scores.length : null;

  // Leaderboard time distribution for chart
  const chartData = leaderboard.slice(0, 15).map((e, i) => ({
    rank: `#${i + 1}`,
    time: +e.completion_time.toFixed(2),
    username: e.username,
  }));

  return (
    <div className="fade-in">
      <PageHeader
        title="Scores & Leaderboard"
        subtitle={`${scores.length} total score entries across ${users.length} players`}
        action={
          <button onClick={fetchData} className="flex items-center gap-2 px-4 py-2.5 glass rounded-xl text-sm font-display font-medium text-purple-300 hover:text-white transition-all">
            <RefreshCw size={14} /> Refresh
          </button>
        }
      />

      {alert && <div className="mb-4"><Alert alert={alert} onClose={() => setAlert(null)} /></div>}

      {/* Stats */}
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 mb-6">
        {[
          { label: 'Total Runs', value: scores.length, icon: BarChart2, color: 'text-purple-400' },
          { label: 'Fastest', value: fastest !== null ? formatTime(fastest) : '—', icon: Trophy, color: 'text-amber-400' },
          { label: 'Average', value: avg !== null ? formatTime(avg) : '—', icon: Clock, color: 'text-cyan-400' },
          { label: 'Slowest', value: slowest !== null ? formatTime(slowest) : '—', icon: TrendingDown, color: 'text-red-400' },
        ].map(({ label, value, icon: Icon, color }) => (
          <div key={label} className="stat-card">
            <div className="flex items-center gap-2 mb-2">
              <Icon size={14} className={color} />
              <p className="text-xs font-display uppercase tracking-widest text-purple-400/50">{label}</p>
            </div>
            <p className="font-display font-bold text-xl text-white font-mono">{value}</p>
          </div>
        ))}
      </div>

      {/* Chart */}
      {chartData.length > 0 && (
        <Card className="p-5 mb-6">
          <h3 className="font-display font-bold text-white mb-1">Top Leaderboard Times</h3>
          <p className="text-xs text-purple-300/40 mb-4">Completion time by rank (seconds)</p>
          <ResponsiveContainer width="100%" height={200}>
            <AreaChart data={chartData} margin={{ top: 0, right: 0, left: -20, bottom: 0 }}>
              <defs>
                <linearGradient id="areaGrad" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="0%" stopColor="#8b5cf6" stopOpacity="0.3" />
                  <stop offset="100%" stopColor="#8b5cf6" stopOpacity="0" />
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" stroke="rgba(139,92,246,0.08)" />
              <XAxis dataKey="rank" tick={{ fill: 'rgba(167,139,250,0.5)', fontSize: 11, fontFamily: 'Syne' }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fill: 'rgba(167,139,250,0.5)', fontSize: 11 }} axisLine={false} tickLine={false} />
              <Tooltip
                contentStyle={{ background: 'rgba(15,13,31,0.95)', border: '1px solid rgba(139,92,246,0.3)', borderRadius: '12px', fontSize: '12px', fontFamily: 'Syne', color: '#e4e0ff' }}
                formatter={(val: any) => [formatTime(val), 'Time']}
                labelFormatter={(label, payload) => payload?.[0]?.payload?.username || label}
              />
              <Area type="monotone" dataKey="time" stroke="#8b5cf6" strokeWidth={2} fill="url(#areaGrad)" />
            </AreaChart>
          </ResponsiveContainer>
        </Card>
      )}

      {/* Public Leaderboard */}
      {leaderboard.length > 0 && (
        <Card className="p-5 mb-6">
          <h3 className="font-display font-bold text-white mb-4">🏆 Official Leaderboard</h3>
          <div className="space-y-2">
            {leaderboard.slice(0, 10).map((entry, i) => (
              <div key={i} className={`flex items-center gap-4 px-4 py-2.5 rounded-xl transition-all ${
                i === 0 ? 'bg-amber-500/10 border border-amber-500/20' :
                i === 1 ? 'bg-zinc-400/5 border border-zinc-400/10' :
                i === 2 ? 'bg-orange-700/5 border border-orange-700/10' :
                'hover:bg-purple-800/10'
              }`}>
                <span className="w-8 text-center font-display font-bold text-lg">
                  {i === 0 ? '🥇' : i === 1 ? '🥈' : i === 2 ? '🥉' : <span className="text-sm text-purple-400/40">#{i+1}</span>}
                </span>
                <span className="flex-1 font-display font-semibold text-sm text-white">{entry.username}</span>
                <span className="font-mono text-sm text-purple-300/70">{formatTime(entry.completion_time)}</span>
              </div>
            ))}
          </div>
        </Card>
      )}

      {/* All Scores Table */}
      <Card>
        <div className="p-5 border-b border-purple-500/10">
          <div className="flex items-center justify-between">
            <h3 className="font-display font-bold text-white">All Score Entries</h3>
            <div className="w-56">
              <SearchInput value={search} onChange={setSearch} placeholder="Filter by player…" />
            </div>
          </div>
        </div>

        {loading ? (
          <div className="flex items-center justify-center py-16"><Spinner size={28} /></div>
        ) : filtered.length === 0 ? (
          <EmptyState icon="🏆" message="No scores found" />
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-purple-500/10">
                  <th className="px-4 py-3 text-left">
                    <span className="text-xs font-display font-semibold uppercase tracking-widest text-purple-400/60">Entry ID</span>
                  </th>
                  <TH col="username" label="Player" />
                  <TH col="completion_time" label="Time" />
                  <th className="px-4 py-3 text-left">
                    <span className="text-xs font-display font-semibold uppercase tracking-widest text-purple-400/60">Submitted</span>
                  </th>
                </tr>
              </thead>
              <tbody>
                {filtered.map((score, idx) => (
                  <tr key={score.id} className={`border-b border-purple-500/5 hover:bg-purple-800/10 transition-colors ${idx % 2 === 0 ? '' : 'bg-purple-800/5'}`}>
                    <td className="px-4 py-3">
                      <span className="font-mono text-xs text-purple-400/40">#{score.id}</span>
                    </td>
                    <td className="px-4 py-3">
                      <p className="text-sm font-display font-semibold text-white">{score.username}</p>
                      <p className="text-xs text-purple-400/40 font-mono">uid #{score.user_id}</p>
                    </td>
                    <td className="px-4 py-3">
                      <span className="font-mono text-sm text-cyan-400">{formatTime(score.completion_time)}</span>
                      <span className="text-xs text-purple-400/30 font-mono ml-2">({score.completion_time.toFixed(3)}s)</span>
                    </td>
                    <td className="px-4 py-3">
                      <span className="text-xs text-purple-400/40">
                        {score.created_at ? formatDate(score.created_at) : '—'}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>
    </div>
  );
}
