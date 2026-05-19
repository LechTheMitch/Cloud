import React, { ReactNode } from 'react';
import { AlertTriangle, CheckCircle, Info, XCircle, X, Loader2 } from 'lucide-react';
import { AlertMessage } from '../types';
import { avatarInitials, randomColor } from '../utils/helpers';

// ─── Loading Spinner ──────────────────────────────────────────────────────────
export function Spinner({ size = 20 }: { size?: number }) {
  return <Loader2 size={size} className="animate-spin text-purple-400" />;
}

// ─── Alert / Toast ────────────────────────────────────────────────────────────
const alertConfig = {
  success: { icon: CheckCircle, bg: 'bg-green-500/10 border-green-500/30', text: 'text-green-400' },
  error:   { icon: XCircle,     bg: 'bg-red-500/10 border-red-500/30',     text: 'text-red-400'   },
  warn:    { icon: AlertTriangle,bg: 'bg-amber-500/10 border-amber-500/30', text: 'text-amber-400' },
  info:    { icon: Info,         bg: 'bg-cyan-500/10 border-cyan-500/30',   text: 'text-cyan-400'  },
};

export function Alert({ alert, onClose }: { alert: AlertMessage; onClose?: () => void }) {
  const cfg = alertConfig[alert.type];
  const Icon = cfg.icon;
  return (
    <div className={`flex items-center gap-3 px-4 py-3 rounded-xl border text-sm fade-in ${cfg.bg} ${cfg.text}`}>
      <Icon size={16} className="shrink-0" />
      <span className="flex-1">{alert.text}</span>
      {onClose && (
        <button onClick={onClose} className="opacity-60 hover:opacity-100 transition-opacity">
          <X size={14} />
        </button>
      )}
    </div>
  );
}

// ─── Avatar ───────────────────────────────────────────────────────────────────
export function Avatar({ username, size = 36 }: { username: string; size?: number }) {
  const bg = randomColor(username);
  return (
    <div
      className="flex items-center justify-center rounded-xl font-display font-bold text-white shrink-0"
      style={{ width: size, height: size, background: bg, fontSize: size * 0.38 }}
    >
      {avatarInitials(username)}
    </div>
  );
}

// ─── Badge ────────────────────────────────────────────────────────────────────
export function Badge({ children, variant }: { children: ReactNode; variant: 'admin' | 'player' | 'success' | 'warn' | 'danger' | 'info' }) {
  const styles: Record<string, string> = {
    admin:   'bg-purple-500/20 text-purple-400 border border-purple-500/30',
    player:  'bg-cyan-500/20 text-cyan-400 border border-cyan-500/30',
    success: 'bg-green-500/20 text-green-400 border border-green-500/30',
    warn:    'bg-amber-500/20 text-amber-400 border border-amber-500/30',
    danger:  'bg-red-500/20 text-red-400 border border-red-500/30',
    info:    'bg-blue-500/20 text-blue-400 border border-blue-500/30',
  };
  return (
    <span className={`inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-display font-semibold ${styles[variant]}`}>
      {children}
    </span>
  );
}

// ─── Section Header ───────────────────────────────────────────────────────────
export function PageHeader({ title, subtitle, action }: { title: string; subtitle?: string; action?: ReactNode }) {
  return (
    <div className="flex items-start justify-between mb-6">
      <div>
        <h1 className="font-display text-2xl font-bold text-white tracking-tight">{title}</h1>
        {subtitle && <p className="text-sm text-purple-200/50 mt-1">{subtitle}</p>}
      </div>
      {action && <div>{action}</div>}
    </div>
  );
}

// ─── Empty State ──────────────────────────────────────────────────────────────
export function EmptyState({ icon, message }: { icon: ReactNode; message: string }) {
  return (
    <div className="flex flex-col items-center justify-center py-16 gap-3 text-purple-200/30">
      <div className="text-4xl">{icon}</div>
      <p className="text-sm font-display">{message}</p>
    </div>
  );
}

// ─── Confirm Modal ────────────────────────────────────────────────────────────
export function ConfirmModal({
  title, message, confirmLabel, variant = 'danger', onConfirm, onCancel, loading
}: {
  title: string;
  message: string;
  confirmLabel: string;
  variant?: 'danger' | 'warn';
  onConfirm: () => void;
  onCancel: () => void;
  loading?: boolean;
}) {
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-black/60 backdrop-blur-sm" onClick={onCancel} />
      <div className="relative glass rounded-2xl p-6 w-full max-w-md fade-in border border-purple-500/20">
        <h3 className="font-display text-lg font-bold text-white mb-2">{title}</h3>
        <p className="text-sm text-purple-200/60 mb-6">{message}</p>
        <div className="flex gap-3 justify-end">
          <button onClick={onCancel} className="px-4 py-2 rounded-xl glass text-sm font-display font-medium text-purple-200/70 hover:text-white transition-colors">
            Cancel
          </button>
          <button
            onClick={onConfirm}
            disabled={loading}
            className={`px-4 py-2 rounded-xl text-sm font-display font-semibold text-white flex items-center gap-2 transition-all ${
              variant === 'danger' ? 'bg-red-500 hover:bg-red-400' : 'bg-amber-500 hover:bg-amber-400 text-black'
            } disabled:opacity-50`}
          >
            {loading && <Spinner size={14} />}
            {confirmLabel}
          </button>
        </div>
      </div>
    </div>
  );
}

// ─── Glass Card ───────────────────────────────────────────────────────────────
export function Card({ children, className = '' }: { children: ReactNode; className?: string }) {
  return (
    <div className={`glass rounded-2xl ${className}`}>
      {children}
    </div>
  );
}

// ─── Search Input ─────────────────────────────────────────────────────────────
export function SearchInput({ value, onChange, placeholder }: { value: string; onChange: (v: string) => void; placeholder?: string }) {
  return (
    <div className="relative">
      <svg className="absolute left-3 top-1/2 -translate-y-1/2 text-purple-400/50" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
        <circle cx="11" cy="11" r="8"/><path d="m21 21-4.35-4.35"/>
      </svg>
      <input
        type="text"
        value={value}
        onChange={e => onChange(e.target.value)}
        placeholder={placeholder || 'Search…'}
        className="glass w-full rounded-xl pl-9 pr-4 py-2.5 text-purple-100 placeholder-purple-200/30 focus:outline-none focus:border-purple-500/60 transition-all duration-200 text-sm"
      />
    </div>
  );
}
