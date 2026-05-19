import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './contexts/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import LoginPage from './pages/LoginPage';
import DashboardPage from './pages/DashboardPage';
import PlayersPage from './pages/PlayersPage';
import ScoresPage from './pages/ScoresPage';
import GameDataPage from './pages/GameDataPage';

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/" element={<ProtectedRoute><DashboardPage /></ProtectedRoute>} />
          <Route path="/players" element={<ProtectedRoute><PlayersPage /></ProtectedRoute>} />
          <Route path="/scores" element={<ProtectedRoute><ScoresPage /></ProtectedRoute>} />
          <Route path="/game" element={<ProtectedRoute><GameDataPage /></ProtectedRoute>} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}
