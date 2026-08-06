/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
 */

import React from 'react';

// Neobrutalist Button Component
interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'danger' | 'success' | 'info';
  size?: 'sm' | 'md' | 'lg';
}

export const Button: React.FC<ButtonProps> = ({
  children,
  variant = 'primary',
  size = 'md',
  className = '',
  ...props
}) => {
  const baseStyle = 'font-extrabold tracking-wider border-2 border-black transition-all transform active:translate-x-[1px] active:translate-y-[1px] active:shadow-[1px_1px_0px_rgba(0,0,0,1)] focus:outline-none';
  
  const variants = {
    primary: 'bg-[#FFDE4D] text-black shadow-[3px_3px_0px_0px_rgba(0,0,0,1)] hover:-translate-x-[1px] hover:-translate-y-[1px] hover:shadow-[4.5px_4.5px_0px_rgba(0,0,0,1)]',
    secondary: 'bg-[#F9A8D4] text-black shadow-[3px_3px_0px_0px_rgba(0,0,0,1)] hover:-translate-x-[1px] hover:-translate-y-[1px] hover:shadow-[4.5px_4.5px_0px_rgba(0,0,0,1)]',
    success: 'bg-[#6EE7B7] text-black shadow-[3px_3px_0px_0px_rgba(0,0,0,1)] hover:-translate-x-[1px] hover:-translate-y-[1px] hover:shadow-[4.5px_4.5px_0px_rgba(0,0,0,1)]',
    danger: 'bg-red-400 text-black shadow-[3px_3px_0px_0px_rgba(0,0,0,1)] hover:-translate-x-[1px] hover:-translate-y-[1px] hover:shadow-[4.5px_4.5px_0px_rgba(0,0,0,1)]',
    info: 'bg-[#A5B4FC] text-black shadow-[3px_3px_0px_0px_rgba(0,0,0,1)] hover:-translate-x-[1px] hover:-translate-y-[1px] hover:shadow-[4.5px_4.5px_0px_rgba(0,0,0,1)]',
  };

  const sizes = {
    sm: 'px-2.5 py-1.5 text-xs',
    md: 'px-4 py-2 text-sm',
    lg: 'px-5 py-2.5 text-sm',
  };

  return (
    <button
      className={`${baseStyle} ${variants[variant]} ${sizes[size]} ${className} disabled:opacity-50 disabled:pointer-events-none`}
      {...props}
    >
      {children}
    </button>
  );
};

// Neobrutalist Input Component
interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
}

export const Input: React.FC<InputProps> = ({
  label,
  error,
  className = '',
  id,
  ...props
}) => {
  return (
    <div className="flex flex-col gap-1 w-full">
      {label && (
        <label htmlFor={id} className="font-mono text-xs font-bold text-black tracking-wide">
          {label}
        </label>
      )}
      <input
        id={id}
        className={`w-full px-3 py-2 bg-white text-black font-semibold border-2 border-black shadow-[2px_2px_0px_rgba(0,0,0,1)] focus:outline-none focus:bg-[#FFFCEE] focus:shadow-[3.5px_3.5px_0px_rgba(0,0,0,1)] transition-all text-sm ${
          error ? 'border-red-500 bg-red-50' : ''
        } ${className}`}
        {...props}
      />
      {error && <span className="text-xs font-bold text-[#FF6B6B] font-mono mt-0.5">{error}</span>}
    </div>
  );
};

// Neobrutalist Card Component
interface CardProps extends React.HTMLAttributes<HTMLDivElement> {
  bgColor?: string;
  hoverable?: boolean;
}

export const Card: React.FC<CardProps> = ({
  children,
  bgColor = 'bg-[#FFFDF6]',
  hoverable = false,
  className = '',
  ...props
}) => {
  return (
    <div
      className={`border-2 border-black p-4 ${bgColor} shadow-[4px_4px_0px_rgba(0,0,0,1)] transition-all ${
        hoverable ? 'hover:-translate-x-[2px] hover:-translate-y-[2px] hover:shadow-[6px_6px_0px_rgba(0,0,0,1)] cursor-pointer' : ''
      } ${className}`}
      {...props}
    >
      {children}
    </div>
  );
};

// Neobrutalist Badge Component
interface BadgeProps {
  children: React.ReactNode;
  color?: 'yellow' | 'pink' | 'green' | 'blue' | 'purple' | 'gray';
  className?: string;
}

export const Badge: React.FC<BadgeProps> = ({
  children,
  color = 'yellow',
  className = '',
}) => {
  const colors = {
    yellow: 'bg-[#FFDE4D]',
    pink: 'bg-[#F9A8D4]',
    green: 'bg-[#6EE7B7]',
    blue: 'bg-[#A5B4FC]',
    purple: 'bg-[#C3A6FF]',
    gray: 'bg-gray-200',
  };

  return (
    <span className={`inline-flex items-center gap-1 px-1.5 py-0.5 text-[10px] font-mono font-bold border border-black shadow-[1px_1px_0px_rgba(0,0,0,1)] text-black ${colors[color]} ${className}`}>
      {children}
    </span>
  );
};

// Neobrutalist Modal Component
interface ModalProps {
  isOpen: boolean;
  onClose: () => void;
  title: React.ReactNode;
  children: React.ReactNode;
  className?: string;
  headerAction?: React.ReactNode;
}

export const Modal: React.FC<ModalProps> = ({
  isOpen,
  onClose,
  title,
  children,
  className = '',
  headerAction,
}) => {
  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      {/* Backdrop */}
      <div 
        className="fixed inset-0 bg-black/50 backdrop-blur-[2px] transition-opacity" 
        onClick={onClose}
      />
      
      {/* Content */}
      <div className={`relative w-full max-w-lg border-2 border-black bg-[#FFFDF6] p-5 shadow-[6px_6px_0px_rgba(0,0,0,1)] z-10 animate-in fade-in zoom-in-95 duration-150 flex flex-col max-h-[90vh] ${className}`}>
        <div className="flex justify-between items-center border-b-2 border-black pb-2.5 mb-3.5 shrink-0">
          <h2 className="text-lg font-bold tracking-wider flex items-center gap-2">{title}</h2>
          <div className="flex items-center gap-1.5 shrink-0">
            {headerAction}
            <button 
              onClick={onClose}
              className="p-1 border border-black bg-[#FF6B6B] hover:bg-red-400 active:translate-x-[0.5px] active:translate-y-[0.5px] shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)] shrink-0"
            >
              <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
        </div>
        <div className="overflow-y-auto flex-1 min-h-0 pr-1">
          {children}
        </div>
      </div>
    </div>
  );
};

// Toast implementation
export interface Toast {
  id: string;
  message: string;
  type: 'success' | 'error' | 'info';
}

export const ToastContainer: React.FC<{ toasts: Toast[]; onClose: (id: string) => void }> = ({
  toasts,
  onClose,
}) => {
  return (
    <div className="fixed bottom-5 right-5 z-50 flex flex-col gap-2.5 max-w-md w-full pointer-events-none">
      {toasts.map((toast) => {
        const bgColors = {
          success: 'bg-[#6EE7B7]',
          error: 'bg-red-400',
          info: 'bg-[#A5B4FC]',
        };
        return (
          <div
            key={toast.id}
            className={`pointer-events-auto border-2 border-black p-3 shadow-[3px_3px_0px_rgba(0,0,0,1)] ${bgColors[toast.type]} text-black font-bold flex justify-between items-center gap-4 animate-bounce-short`}
          >
            <span className="font-sans text-xs tracking-wide">{toast.message}</span>
            <button
              onClick={() => onClose(toast.id)}
              className="p-0.5 border border-black bg-white/40 hover:bg-white/60 text-black active:translate-x-[0.5px] active:translate-y-[0.5px]"
            >
              <svg xmlns="http://www.w3.org/2000/svg" className="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3.5} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
        );
      })}
    </div>
  );
};
