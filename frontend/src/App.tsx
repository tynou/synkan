/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
 */

import { useState, useEffect } from 'react';
import { AuthPage } from './components/AuthPage';
import { BoardList } from './components/BoardList';
import { BoardView } from './components/BoardView';
import { Toast, ToastContainer } from './components/NeobrutalistComponents';
import { api, getAuthToken, logout } from './api';
import { UserDto } from './types';


export default function App() {
  const getBoardIdFromUrl = (): string | null => {
    const match = window.location.pathname.match(/^\/([0-9a-fA-F-]{36})$/);
    return match ? match[1] : null;
  };

  const [currentUser, setCurrentUser] = useState<UserDto | null>(null);
  const [selectedBoardId, setSelectedBoardId] = useState<string | null>(getBoardIdFromUrl());
  const [toasts, setToasts] = useState<Toast[]>([]);
  const [appLoading, setAppLoading] = useState(true);

  // Sync state with browser back/forward buttons
  useEffect(() => {
    const handlePopState = () => {
      setSelectedBoardId(getBoardIdFromUrl());
    };
    window.addEventListener('popstate', handlePopState);
    return () => {
      window.removeEventListener('popstate', handlePopState);
    };
  }, []);

  // Toast helper
  const showToast = (message: string, type: 'success' | 'error' | 'info') => {
    const id = Date.now().toString() + Math.random().toString().substring(2, 6);
    setToasts((prev) => [...prev, { id, message, type }]);

    // Auto dismiss after 4 seconds
    setTimeout(() => {
      setToasts((prev) => prev.filter((t) => t.id !== id));
    }, 4000);
  };

  const removeToast = (id: string) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  };

  // Check login status on load
  const checkAuth = async () => {
    try {
      const user = await api.getMe();
      setCurrentUser(user);
      localStorage.setItem('synkan_user', JSON.stringify(user));
    } catch (err: any) {
      console.error('Session restoration failed:', err);
      // Clear stale session
      logout();
      setCurrentUser(null);
    }
    setAppLoading(false);
  };

  useEffect(() => {
    checkAuth();
  }, []);

  const handleAuthSuccess = () => {
    checkAuth();
  };

  const handleLogout = () => {
    setCurrentUser(null);
    setSelectedBoardId(null);
    window.history.pushState(null, '', '/');
  };

  if (appLoading) {
    return (
      <div className="min-h-screen bg-[#FAF9F6] flex flex-col items-center justify-center p-6 text-black">
        <div className="animate-spin rounded-none border-2 border-black border-t-transparent w-12 h-12 mb-4 bg-[#FFDE4D]" />
        <p className="font-mono text-sm font-bold text-gray-700 animate-pulse">
          BOOTSTRAPPING COLLABORATIVE ENVIRONMENT...
        </p>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-[#FAF9F6]">
      {selectedBoardId !== null ? (
        <BoardView
          boardId={selectedBoardId}
          onBack={() => {
            setSelectedBoardId(null);
            window.history.pushState(null, '', '/');
          }}
          showToast={showToast}
          currentUser={currentUser}
        />
      ) : currentUser === null ? (
        <AuthPage 
          onAuthSuccess={handleAuthSuccess} 
          showToast={showToast} 
        />
      ) : (
        <BoardList
          onSelectBoard={(id) => {
            setSelectedBoardId(id);
            window.history.pushState(null, '', `/${id}`);
          }}
          showToast={showToast}
          currentUser={currentUser}
          onLogout={handleLogout}
        />
      )}

      {/* Floating global toasts */}
      <ToastContainer toasts={toasts} onClose={removeToast} />
    </div>
  );
}
