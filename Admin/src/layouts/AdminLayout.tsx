import React, { useState } from 'react';
import { NavLink, useNavigate } from 'react-router-dom';
import {
  LayoutDashboard, Users, Trophy, GamepadIcon,
  LogOut, Menu, X, Shield, ChevronRight
} from 'lucide-react';
import { useAuth } from '../contexts/AuthContext';
import { Avatar } from '../components/ui';

const NAV_ITEMS = [
  { to: '/',         icon: LayoutDashboard, label: 'Dashboard',      exact: true  },
  { to: '/players',  icon: Users,           label: 'Players',         exact: false },
  { to: '/scores',   icon: Trophy,          label: 'Scores & Leaderboard', exact: false },
  { to: '/game',     icon: GamepadIcon,     label: 'Game Data',       exact: false },
];

export default function AdminLayout({ children }: { children: React.ReactNode }) {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [sidebarOpen, setSidebarOpen] = useState(false);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const SidebarContent = () => (
    <div className="flex flex-col h-full">
      {/* Logo */}
      <div className="px-5 pt-6 pb-5 border-b border-purple-500/10">
        <div className="flex items-center gap-3">
          <div className="w-9 h-9 rounded-xl bg-gradient-to-br from-purple-600 to-purple-800 flex items-center justify-center shadow-lg shadow-purple-900/40">
            <Shield size={18} className="text-white" />
          </div>
          <div>
            <p className="font-display font-bold text-white text-sm tracking-wide">GAME ADMIN</p>
            <p className="text-xs text-purple-400/60 font-mono">v1.0 · Control Panel</p>
          </div>
        </div>
      </div>

      {/* Navigation */}
      <nav className="flex-1 px-3 py-4 space-y-1 overflow-y-auto">
        <p className="px-3 mb-2 text-xs font-display font-semibold uppercase tracking-widest text-purple-400/40">
          Main
        </p>
        {NAV_ITEMS.map(({ to, icon: Icon, label, exact }) => (
          <NavLink
            key={to}
            to={to}
            end={exact}
            className={({ isActive }) =>
              `flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-display font-medium transition-all duration-200 group ${
                isActive
                  ? 'bg-purple-600/25 text-purple-300 border border-purple-500/25'
                  : 'text-purple-200/50 hover:text-purple-100 hover:bg-purple-800/30'
              }`
            }
            onClick={() => setSidebarOpen(false)}
          >
            <Icon size={16} className="shrink-0" />
            <span className="flex-1">{label}</span>
            <ChevronRight size={12} className="opacity-0 group-hover:opacity-40 transition-opacity" />
          </NavLink>
        ))}
      </nav>

      {/* User info + Logout */}
      <div className="p-3 border-t border-purple-500/10">
        {user && (
          <div className="flex items-center gap-3 px-2 py-2 rounded-xl mb-2">
            <Avatar username={user.username} size={32} />
            <div className="flex-1 min-w-0">
              <p className="text-sm font-display font-semibold text-white truncate">{user.username}</p>
              <p className="text-xs text-purple-400/60">{user.is_admin ? 'Administrator' : 'Moderator'}</p>
            </div>
          </div>
        )}
        <button
          onClick={handleLogout}
          className="w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-display font-medium text-red-400/70 hover:text-red-400 hover:bg-red-500/10 transition-all duration-200"
        >
          <LogOut size={16} />
          Sign out
        </button>
      </div>
    </div>
  );

  return (
    <div className="min-h-screen grid-noise">
      <div className="mesh-bg" />

      {/* Mobile overlay */}
      {sidebarOpen && (
        <div
          className="fixed inset-0 z-30 bg-black/60 backdrop-blur-sm lg:hidden"
          onClick={() => setSidebarOpen(false)}
        />
      )}

      {/* Sidebar */}
      <aside className={`fixed top-0 left-0 h-full w-64 z-40 glass border-r border-purple-500/10 transition-transform duration-300 ${
        sidebarOpen ? 'translate-x-0' : '-translate-x-full lg:translate-x-0'
      }`}>
        <SidebarContent />
      </aside>

      {/* Main area */}
      <div className="lg:ml-64 min-h-screen flex flex-col">
        {/* Top bar (mobile) */}
        <header className="lg:hidden sticky top-0 z-20 glass border-b border-purple-500/10 px-4 py-3 flex items-center gap-3">
          <button onClick={() => setSidebarOpen(true)} className="text-purple-300 hover:text-white transition-colors">
            <Menu size={20} />
          </button>
          <span className="font-display font-bold text-white text-sm">GAME ADMIN</span>
        </header>

        {/* Page content */}
        <main className="flex-1 p-6 lg:p-8">
          {children}
        </main>
      </div>
    </div>
  );
}
