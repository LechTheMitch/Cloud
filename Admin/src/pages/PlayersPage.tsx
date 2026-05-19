import React, { useEffect, useState, useMemo } from 'react';
import { Trash2, RefreshCw, UserCheck, UserX, ChevronUp, ChevronDown } from 'lucide-react';
import { adminService } from '../services/api';
import { User } from '../types';
import {
  Spinner, Card, PageHeader, Avatar, Badge,
  ConfirmModal, Alert, EmptyState, SearchInput
} from '../components/ui';
import { getApiError } from '../utils/helpers';
import { AlertMessage } from '../types';
import { useAuth } from '../contexts/AuthContext';

type SortKey = 'id' | 'username' | 'is_admin';
type SortDir = 'asc' | 'desc';

export default function PlayersPage() {
  const { user: me } = useAuth();
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [alert, setAlert] = useState<AlertMessage | null>(null);
  const [search, setSearch] = useState('');
  const [sortKey, setSortKey] = useState<SortKey>('id');
  const [sortDir, setSortDir] = useState<SortDir>('asc');
  const [filterRole, setFilterRole] = useState<'all' | 'admin' | 'player'>('all');
  const [confirmDelete, setConfirmDelete] = useState<User | null>(null);
  const [deleting, setDeleting] = useState(false);

  const fetchUsers = async () => {
    setLoading(true);
    try {
      const data = await adminService.listUsers();
      setUsers(data);
    } catch (e) {
      setAlert({ type: 'error', text: getApiError(e) });
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchUsers(); }, []);

  const handleDelete = async () => {
    if (!confirmDelete) return;
    setDeleting(true);
    try {
      await adminService.deleteUser(confirmDelete.id);
      setUsers(prev => prev.filter(u => u.id !== confirmDelete.id));
      setAlert({ type: 'success', text: `User "${confirmDelete.username}" has been deleted.` });
    } catch (e) {
      setAlert({ type: 'error', text: getApiError(e) });
    } finally {
      setDeleting(false);
      setConfirmDelete(null);
    }
  };

  const toggleSort = (key: SortKey) => {
    if (sortKey === key) setSortDir(d => d === 'asc' ? 'desc' : 'asc');
    else { setSortKey(key); setSortDir('asc'); }
  };

  const filtered = useMemo(() => {
    let list = [...users];
    if (search) list = list.filter(u => u.username.toLowerCase().includes(search.toLowerCase()));
    if (filterRole === 'admin') list = list.filter(u => u.is_admin);
    if (filterRole === 'player') list = list.filter(u => !u.is_admin);
    list.sort((a, b) => {
      let av: any = a[sortKey], bv: any = b[sortKey];
      if (typeof av === 'boolean') { av = av ? 1 : 0; bv = bv ? 1 : 0; }
      if (typeof av === 'string') { av = av.toLowerCase(); bv = bv.toLowerCase(); }
      return sortDir === 'asc' ? (av > bv ? 1 : -1) : (av < bv ? 1 : -1);
    });
    return list;
  }, [users, search, filterRole, sortKey, sortDir]);

  const SortIcon = ({ col }: { col: SortKey }) => {
    if (sortKey !== col) return <ChevronUp size={12} className="opacity-20" />;
    return sortDir === 'asc' ? <ChevronUp size={12} className="text-purple-400" /> : <ChevronDown size={12} className="text-purple-400" />;
  };

  const TH = ({ col, label }: { col: SortKey; label: string }) => (
    <th
      className="px-4 py-3 text-left cursor-pointer select-none group"
      onClick={() => toggleSort(col)}
    >
      <span className="flex items-center gap-1 text-xs font-display font-semibold uppercase tracking-widest text-purple-400/60 group-hover:text-purple-300 transition-colors">
        {label}
        <SortIcon col={col} />
      </span>
    </th>
  );

  return (
    <div className="fade-in">
      <PageHeader
        title="Player Management"
        subtitle={`${users.length} registered accounts`}
        action={
          <button onClick={fetchUsers} className="flex items-center gap-2 px-4 py-2.5 glass rounded-xl text-sm font-display font-medium text-purple-300 hover:text-white transition-all">
            <RefreshCw size={14} />
            Refresh
          </button>
        }
      />

      {alert && (
        <div className="mb-4">
          <Alert alert={alert} onClose={() => setAlert(null)} />
        </div>
      )}

      {/* Filters */}
      <div className="flex flex-col sm:flex-row gap-3 mb-5">
        <div className="flex-1">
          <SearchInput value={search} onChange={setSearch} placeholder="Search by username…" />
        </div>
        <div className="flex gap-2">
          {(['all', 'admin', 'player'] as const).map(role => (
            <button
              key={role}
              onClick={() => setFilterRole(role)}
              className={`px-4 py-2.5 rounded-xl text-xs font-display font-semibold uppercase tracking-wide transition-all ${
                filterRole === role
                  ? 'bg-purple-600/30 text-purple-300 border border-purple-500/30'
                  : 'glass text-purple-400/50 hover:text-purple-300'
              }`}
            >
              {role === 'all' ? 'All' : role === 'admin' ? '🛡 Admins' : '🎮 Players'}
            </button>
          ))}
        </div>
      </div>

      {/* Summary chips */}
      <div className="flex gap-3 mb-5">
        <div className="glass rounded-xl px-3 py-1.5 text-xs font-display">
          <span className="text-purple-400/50">Total </span>
          <span className="text-white font-bold">{users.length}</span>
        </div>
        <div className="glass rounded-xl px-3 py-1.5 text-xs font-display">
          <span className="text-purple-400/50">Admins </span>
          <span className="text-cyan-400 font-bold">{users.filter(u => u.is_admin).length}</span>
        </div>
        <div className="glass rounded-xl px-3 py-1.5 text-xs font-display">
          <span className="text-purple-400/50">Players </span>
          <span className="text-green-400 font-bold">{users.filter(u => !u.is_admin).length}</span>
        </div>
        <div className="glass rounded-xl px-3 py-1.5 text-xs font-display">
          <span className="text-purple-400/50">Showing </span>
          <span className="text-white font-bold">{filtered.length}</span>
        </div>
      </div>

      <Card>
        {loading ? (
          <div className="flex items-center justify-center py-16">
            <Spinner size={28} />
          </div>
        ) : filtered.length === 0 ? (
          <EmptyState icon="👤" message="No players found" />
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-purple-500/10">
                  <TH col="id" label="ID" />
                  <TH col="username" label="Username" />
                  <TH col="is_admin" label="Role" />
                  <th className="px-4 py-3 text-right">
                    <span className="text-xs font-display font-semibold uppercase tracking-widest text-purple-400/60">
                      Actions
                    </span>
                  </th>
                </tr>
              </thead>
              <tbody>
                {filtered.map((user, idx) => (
                  <tr
                    key={user.id}
                    className={`border-b border-purple-500/5 hover:bg-purple-800/10 transition-colors ${
                      idx % 2 === 0 ? '' : 'bg-purple-800/5'
                    }`}
                  >
                    <td className="px-4 py-3">
                      <span className="font-mono text-xs text-purple-400/50">#{user.id}</span>
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex items-center gap-3">
                        <Avatar username={user.username} size={30} />
                        <div>
                          <p className="text-sm font-display font-semibold text-white">
                            {user.username}
                            {me?.username === user.username && (
                              <span className="ml-2 text-xs text-purple-400/40">(you)</span>
                            )}
                          </p>
                        </div>
                      </div>
                    </td>
                    <td className="px-4 py-3">
                      <Badge variant={user.is_admin ? 'admin' : 'player'}>
                        {user.is_admin ? (
                          <><UserCheck size={10} /> Admin</>
                        ) : (
                          <><UserX size={10} /> Player</>
                        )}
                      </Badge>
                    </td>
                    <td className="px-4 py-3 text-right">
                      {me?.username !== user.username ? (
                        <button
                          onClick={() => setConfirmDelete(user)}
                          className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-display font-semibold text-red-400/70 hover:text-red-400 hover:bg-red-500/10 transition-all border border-transparent hover:border-red-500/20"
                          title="Delete user"
                        >
                          <Trash2 size={12} />
                          Delete
                        </button>
                      ) : (
                        <span className="text-xs text-purple-400/20 px-3 py-1.5">—</span>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>

      {/* Note about missing features */}
      <div className="mt-4 glass rounded-xl p-4 border border-amber-500/15">
        <p className="text-xs text-amber-400/60 font-display">
          <span className="font-bold text-amber-400/80">ℹ️ Backend Note:</span> The current API supports listing and deleting users.
          Features like suspend/ban, password reset, and role editing are not available in the backend and would require API extension.
        </p>
      </div>

      {confirmDelete && (
        <ConfirmModal
          title="Delete Player Account"
          message={`Are you sure you want to permanently delete "${confirmDelete.username}"? This will remove all their data including saves and scores.`}
          confirmLabel="Delete Player"
          variant="danger"
          onConfirm={handleDelete}
          onCancel={() => setConfirmDelete(null)}
          loading={deleting}
        />
      )}
    </div>
  );
}
