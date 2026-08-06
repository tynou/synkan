/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
 */

import React, { useState } from 'react';
import { Card, Input, Button, Badge } from './NeobrutalistComponents';
import { api, setAuthToken } from '../api';
import { ShieldCheck, KeyRound, User } from 'lucide-react';

interface AuthPageProps {
  onAuthSuccess: () => void;
  showToast: (message: string, type: 'success' | 'error' | 'info') => void;
}

export const AuthPage: React.FC<AuthPageProps> = ({ onAuthSuccess, showToast }) => {
  const [isLogin, setIsLogin] = useState(true);
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [errors, setErrors] = useState<{ username?: string; password?: string }>({});

  const validate = () => {
    const newErrors: typeof errors = {};
    if (!username.trim()) {
      newErrors.username = 'Username is required';
    } else if (username.length < 3) {
      newErrors.username = 'Username must be at least 3 characters';
    }
    if (!password) {
      newErrors.password = 'Password is required';
    } else if (password.length < 4) {
      newErrors.password = 'Password must be at least 4 characters';
    }
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;

    setIsLoading(true);
    try {
      if (isLogin) {
        showToast('Logging in...', 'info');
        const authRes = await api.login({ username, password });
        // The token is saved in localStorage
        setAuthToken(authRes.token);
        
        // Retrieve me to verify JWT token validity
        const user = await api.getMe();
        localStorage.setItem('synkan_user', JSON.stringify(user));
        
        showToast(`Welcome back, ${user.username}!`, 'success');
        onAuthSuccess();
      } else {
        showToast('Registering user account...', 'info');
        await api.register({ username, password });
        showToast('Registration successful! Logging in now...', 'success');
        
        // Auto login on successful register
        const authRes = await api.login({ username, password });
        setAuthToken(authRes.token);
        const user = await api.getMe();
        localStorage.setItem('synkan_user', JSON.stringify(user));
        
        onAuthSuccess();
      }
    } catch (error: any) {
      console.error(error);
      showToast(error.message || 'An authentication error occurred', 'error');
    } finally {
      setIsLoading(false);
    }
  };


  return (
    <div className="min-h-screen bg-[#FAF9F6] flex flex-col items-center justify-center p-4 selection:bg-[#F9A8D4]">
      {/* Brand Title */}
      <div className="mb-8 text-center">
        <h1 className="text-5xl md:text-6xl font-black tracking-tight text-black flex items-center justify-center gap-3 drop-shadow-[2.5px_2.5px_0px_rgba(0,0,0,1)]">
          <span className="bg-[#FFDE4D] px-6 py-2 border-2 border-black inline-block transform -rotate-2">
            SYNKAN
          </span>
        </h1>
        <p className="mt-4 text-sm font-mono font-bold text-gray-700 tracking-widest">
          Neobrutalist Collaborative Kanban Tool
        </p>
      </div>

      <div className="w-full max-w-md relative">
        {/* Decorative Badge */}
        <div className="absolute -top-5 -right-3 z-10 transform rotate-6">
          <Badge color={isLogin ? 'yellow' : 'pink'}>
            {isLogin ? 'WELCOME BACK' : 'JOIN THE BOARD'}
          </Badge>
        </div>

        {/* Main Auth Card */}
        <Card bgColor="bg-[#FFFDF6]">
          <form onSubmit={handleSubmit} className="flex flex-col gap-5">
            <h2 className="text-2xl font-black border-b-3 border-black pb-2 mb-2 flex items-center gap-2">
              {isLogin ? <ShieldCheck className="w-6 h-6 text-[#6EE7B7]" /> : <User className="w-6 h-6 text-[#F9A8D4]" />}
              {isLogin ? 'ACCOUNT LOGIN' : 'CREATE ACCOUNT'}
            </h2>

            <div className="relative">
              <Input
                label="Username"
                id="username"
                type="text"
                placeholder="Enter username"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                error={errors.username}
                disabled={isLoading}
              />
            </div>

            <div className="relative">
              <Input
                label="Password"
                id="password"
                type="password"
                placeholder="••••••••"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                error={errors.password}
                disabled={isLoading}
              />
            </div>

            <Button
              type="submit"
              variant={isLogin ? 'primary' : 'secondary'}
              disabled={isLoading}
              className="mt-2 text-md font-extrabold"
            >
              {isLoading ? 'PROCESSING...' : isLogin ? 'SIGN IN' : 'CREATE ACCOUNT'}
            </Button>

            <div className="text-center mt-3 pt-4 border-t-2 border-black">
              <p className="font-mono text-xs font-bold text-gray-700">
                {isLogin ? "Don't have an account yet?" : 'Already have an account?'}
              </p>
              <button
                type="button"
                onClick={() => {
                  setIsLogin(!isLogin);
                  setErrors({});
                }}
                className="mt-1 font-mono text-sm font-black underline text-black hover:text-[#F9A8D4] transition-colors focus:outline-none"
              >
                {isLogin ? 'REGISTER NEW ACCOUNT' : 'BACK TO LOGIN'}
              </button>
            </div>
          </form>
        </Card>

      </div>
    </div>
  );
};
