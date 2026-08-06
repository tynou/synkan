/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
 */

import React, { useState, useEffect } from 'react';
import { Card, Button, Modal, Input, Badge } from './NeobrutalistComponents';
import { api, logout } from '../api';
import { BoardDto, UserDto } from '../types';
import { 
  ClipboardList, 
  Plus, 
  Trash2, 
  Edit3, 
  LogOut, 
  Search, 
  RefreshCw, 
  Users, 
  ShieldAlert, 
  ArrowRight,
  Database,
  Globe,
  Lock,
  Settings,
  AlertTriangle
} from 'lucide-react';

interface BoardListProps {
  onSelectBoard: (boardId: string) => void;
  showToast: (message: string, type: 'success' | 'error' | 'info') => void;
  currentUser: UserDto | null;
  onLogout: () => void;
}

export const BoardList: React.FC<BoardListProps> = ({
  onSelectBoard,
  showToast,
  currentUser,
  onLogout,
}) => {
  const [boards, setBoards] = useState<BoardDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  
  // Create Board Modal State
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [newBoardTitle, setNewBoardTitle] = useState('');
  const [newBoardIsPublic, setNewBoardIsPublic] = useState(false);
  const [createLoading, setCreateLoading] = useState(false);

  // Edit Board Title Modal State
  const [isEditOpen, setIsEditOpen] = useState(false);
  const [editingBoard, setEditingBoard] = useState<BoardDto | null>(null);
  const [editBoardTitle, setEditBoardTitle] = useState('');
  const [editBoardIsPublic, setEditBoardIsPublic] = useState(false);
  const [editLoading, setEditLoading] = useState(false);

  // Delete Board Confirmation State
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [deletingBoard, setDeletingBoard] = useState<BoardDto | null>(null);
  const [deleteLoading, setDeleteLoading] = useState(false);

  const fetchBoards = async (silent = false) => {
    if (!silent) setLoading(true);
    try {
      const data = await api.getBoards();
      setBoards(data || []);
    } catch (err: any) {
      console.error(err);
      showToast(err.message || 'Failed to load boards. Is the backend API running?', 'error');
    } finally {
      if (!silent) setLoading(false);
    }
  };

  useEffect(() => {
    fetchBoards();
  }, []);

  const handleCreateBoard = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newBoardTitle.trim()) {
      showToast('Board title cannot be empty', 'error');
      return;
    }

    setCreateLoading(true);
    try {
      const creationRes = await api.createBoard({ 
        title: newBoardTitle.trim(),
        isPublic: newBoardIsPublic
      });
      showToast(`Board "${newBoardTitle}" created successfully!`, 'success');
      setNewBoardTitle('');
      setNewBoardIsPublic(false);
      setIsCreateOpen(false);
      // Refresh boards list
      await fetchBoards(true);
      // Auto select the new board
      if (creationRes && creationRes.id) {
        onSelectBoard(creationRes.id);
      }
    } catch (err: any) {
      showToast(err.message || 'Failed to create board', 'error');
    } finally {
      setCreateLoading(false);
    }
  };

  const handleEditBoard = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingBoard) return;
    if (!editBoardTitle.trim()) {
      showToast('Board title cannot be empty', 'error');
      return;
    }

    setEditLoading(true);
    try {
      await api.editBoardTitle(editingBoard.id, { 
        title: editBoardTitle.trim()
      });
      await api.changeBoardVisibility(editingBoard.id, {
        newIsPublic: editBoardIsPublic
      });
      showToast('Board settings updated successfully!', 'success');
      setIsEditOpen(false);
      setEditingBoard(null);
      await fetchBoards(true);
    } catch (err: any) {
      showToast(err.message || 'Failed to update board settings', 'error');
    } finally {
      setEditLoading(false);
    }
  };

  const handleDeleteBoard = async () => {
    if (!deletingBoard) return;
    
    setDeleteLoading(true);
    try {
      await api.deleteBoard(deletingBoard.id);
      showToast(`Board "${deletingBoard.title}" deleted`, 'success');
      setIsDeleteOpen(false);
      setDeletingBoard(null);
      await fetchBoards(true);
    } catch (err: any) {
      showToast(err.message || 'Failed to delete board', 'error');
    } finally {
      setDeleteLoading(false);
    }
  };

  const filteredBoards = boards.filter((board) =>
    (board.title || '').toLowerCase().includes(searchQuery.toLowerCase())
  );

  return (
    <div className="min-h-screen bg-[#FAF9F6] flex flex-col selection:bg-[#F9A8D4] text-black">
      {/* Header bar */}
      <header className="border-b-2 border-black bg-[#A5B4FC] p-4 sticky top-0 z-40">
        <div className="max-w-7xl mx-auto flex flex-col md:flex-row justify-between items-center gap-4">
          <div className="flex items-center gap-3">
            <h1 
              onClick={() => fetchBoards()}
              className="text-3xl font-black tracking-tight bg-[#FFDE4D] border-2 border-black px-3 py-1 shadow-[2px_2px_0px_rgba(0,0,0,1)] hover:-translate-x-[1px] hover:-translate-y-[1px] hover:shadow-[3px_3px_0px_rgba(0,0,0,1)] active:translate-x-[1px] active:translate-y-[1px] cursor-pointer"
            >
              SYNKAN
            </h1>
            <Badge color="pink">KANBAN DASHBOARD</Badge>
          </div>

          <div className="flex items-center gap-4 w-full md:w-auto justify-end">
            <div className="flex items-center gap-2 border-2 border-black px-3 py-1.5 bg-[#FFFDF6] shadow-[2px_2px_0px_rgba(0,0,0,1)] font-mono text-xs font-bold">
              <span className="w-2 h-2 rounded-full bg-[#6EE7B7] inline-block animate-pulse" />
              <span>USER: <strong className="text-black">{currentUser?.username || 'GUEST'}</strong></span>
            </div>

            <Button
              variant="danger"
              size="sm"
              onClick={() => {
                logout();
                onLogout();
                showToast('Logged out successfully', 'success');
              }}
              className="flex items-center gap-2"
            >
              <LogOut className="w-3.5 h-3.5" />
              <span>LOGOUT</span>
            </Button>
          </div>
        </div>
      </header>

      {/* Main Container */}
      <main className="flex-1 p-6 max-w-7xl mx-auto w-full flex flex-col gap-8">
        
        {/* Welcome Section / Board Controls */}
        <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 bg-[#C3A6FF] p-5 border-2 border-black shadow-[4px_4px_0px_rgba(0,0,0,1)]">
          <div>
            <h2 className="text-2xl font-black tracking-wide">
              Greetings, {currentUser?.username}!
            </h2>
            <p className="font-mono text-xs font-bold text-black/80 mt-1">
              Manage your tasks, columns and boards here. Everything is saved to Synkan.API.
            </p>
          </div>
          
          <div className="flex gap-3 w-full md:w-auto">
            <Button
              variant="primary"
              onClick={() => setIsCreateOpen(true)}
              className="flex items-center gap-2 justify-center w-full md:w-auto"
            >
              <Plus className="w-4 h-4 stroke-[3]" />
              <span>CREATE NEW BOARD</span>
            </Button>
          </div>
        </div>

        {/* Search & Statistics Filter Bar */}
        <div className="flex flex-col sm:flex-row gap-4 items-center justify-between">
          <div className="w-full sm:max-w-md relative">
            <Input
              type="text"
              placeholder="Search boards by title..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-10 font-mono text-sm py-2 bg-white"
            />
            <Search className="absolute left-3.5 top-3.5 w-4 h-4 text-black stroke-[2.5]" />
          </div>

          <div className="flex items-center gap-3 self-end sm:self-auto">
            <button
              onClick={() => fetchBoards()}
              disabled={loading}
              className="p-2 border-2 border-black bg-white shadow-[2px_2px_0px_rgba(0,0,0,1)] hover:bg-[#FFFCEE] active:translate-x-[1px] active:translate-y-[1px] active:shadow-[1px_1px_0px_rgba(0,0,0,1)] transition-all font-mono text-xs font-bold flex items-center gap-2"
            >
              <RefreshCw className={`w-3.5 h-3.5 ${loading ? 'animate-spin' : ''}`} />
              <span>REFRESH</span>
            </button>
            <Badge color="purple">{filteredBoards.length} BOARDS FOUND</Badge>
          </div>
        </div>

        {/* Dashboard Grid */}
        {loading ? (
          <div className="flex-1 flex flex-col items-center justify-center py-20">
            <div className="animate-spin rounded-none border-2 border-black border-t-transparent w-12 h-12 mb-4 bg-[#FFDE4D]" />
            <p className="font-mono text-sm font-bold text-gray-700 animate-pulse">
              RETRIEVING BOARDS FROM SYNKAN.API...
            </p>
          </div>
        ) : filteredBoards.length === 0 ? (
          <div className="flex-1 border-2 border-dashed border-black bg-[#FFFDF6] p-12 text-center flex flex-col items-center justify-center gap-4 shadow-[4px_4px_0px_rgba(0,0,0,1)]">
            <ClipboardList className="w-16 h-16 text-black stroke-[1.5]" />
            <div>
              <h3 className="text-xl font-bold">No boards discovered</h3>
              <p className="font-mono text-xs text-gray-600 mt-1 max-w-md mx-auto">
                {searchQuery ? "No boards match your filter criteria. Try searching for something else." : "You don't have any boards yet. Create your very first board to start managing your Trello columns!"}
              </p>
            </div>
            {!searchQuery && (
              <Button variant="primary" onClick={() => setIsCreateOpen(true)} className="mt-2">
                CREATE BOARD NOW
              </Button>
            )}
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            
            {/* Boards Cards */}
            {filteredBoards.map((board) => {
              const isOwner = board.ownerId === currentUser?.id;
              const memberCount = board.memberCount || 0;
              const columnCount = board.columnCount || 0;
              
              return (
                <Card 
                  key={board.id} 
                  hoverable 
                  bgColor={isOwner ? 'bg-white' : 'bg-[#FFFCEE]'}
                  className="flex flex-col justify-between min-h-[220px] relative group"
                >
                  <div className="flex flex-col gap-2">
                    {/* Board Meta details */}
                    <div className="flex justify-between items-start mb-1 gap-2 flex-wrap">
                      <div className="flex gap-1 flex-wrap">
                        <Badge color={isOwner ? 'yellow' : 'pink'}>
                          {isOwner ? 'OWNER' : 'COLLABORATOR'}
                        </Badge>
                        <Badge color={board.isPublic ? 'green' : 'blue'}>
                          {board.isPublic ? (
                            <>
                              <Globe className="w-2.5 h-2.5 stroke-[2.5]" />
                              <span>PUBLIC</span>
                            </>
                          ) : (
                            <>
                              <Lock className="w-2.5 h-2.5 stroke-[2.5]" />
                              <span>PRIVATE</span>
                            </>
                          )}
                        </Badge>
                      </div>
                      
                      <div className="flex items-center gap-1.5 text-xs font-mono font-bold text-gray-600">
                        <Users className="w-3.5 h-3.5 text-black" />
                        <span>{memberCount} {memberCount === 1 ? 'user' : 'users'}</span>
                      </div>
                    </div>

                    {/* Title */}
                    <h3 className="text-2xl font-black tracking-wide text-black group-hover:text-amber-600 transition-colors line-clamp-2">
                      {board.title || 'UNTITLED BOARD'}
                    </h3>
                  </div>

                  {/* Actions / Stats bottom bar */}
                  <div className="mt-6 pt-4 border-t-2 border-black flex items-center justify-between">
                    <div className="flex flex-col">
                      <span className="font-mono text-[10px] font-bold text-gray-500">COLUMNS</span>
                      <span className="font-mono text-sm font-black">{columnCount} COLUMNS</span>
                    </div>

                    <div className="flex items-center gap-2">
                      {/* Edit Title Button */}
                      <button
                        onClick={(e) => {
                          e.stopPropagation();
                          setEditingBoard(board);
                          setEditBoardTitle(board.title || '');
                          setEditBoardIsPublic(board.isPublic || false);
                          setIsEditOpen(true);
                        }}
                        className="p-1.5 border-2 border-black bg-[#A5B4FC] hover:bg-indigo-300 active:translate-x-[1px] active:translate-y-[1px] shadow-[2px_2px_0px_rgba(0,0,0,1)] transition-colors"
                        title="Rename Board"
                      >
                        <Edit3 className="w-3.5 h-3.5" />
                      </button>

                      {/* Delete Board Button */}
                      <button
                        onClick={(e) => {
                          e.stopPropagation();
                          setDeletingBoard(board);
                          setIsDeleteOpen(true);
                        }}
                        className="p-1.5 border-2 border-black bg-red-400 hover:bg-red-300 active:translate-x-[1px] active:translate-y-[1px] shadow-[2px_2px_0px_rgba(0,0,0,1)] transition-colors"
                        title="Delete Board"
                      >
                        <Trash2 className="w-3.5 h-3.5" />
                      </button>

                      {/* Select/Enter Board Button */}
                      <Button
                        variant="primary"
                        size="sm"
                        onClick={() => onSelectBoard(board.id)}
                        className="flex items-center gap-1 px-3 py-1 font-mono text-xs shadow-[2px_2px_0px_rgba(0,0,0,1)] active:shadow-[1px_1px_0px_rgba(0,0,0,1)] group-hover:bg-[#6EE7B7] group-hover:shadow-[3px_3px_0px_rgba(0,0,0,1)]"
                      >
                        <span>OPEN</span>
                        <ArrowRight className="w-3 h-3 stroke-[3]" />
                      </Button>
                    </div>
                  </div>
                </Card>
              );
            })}

            {/* Quick Create Board Card */}
            <div 
              onClick={() => setIsCreateOpen(true)}
              className="border-4 border-dashed border-black/50 hover:border-black bg-black/5 hover:bg-black/10 p-6 shadow-[4px_4px_0px_rgba(0,0,0,1)] hover:shadow-[8px_8px_0px_rgba(0,0,0,1)] transition-all cursor-pointer flex flex-col items-center justify-center text-center gap-3 min-h-[220px] hover:-translate-x-[2px] hover:-translate-y-[2px]"
            >
              <div className="p-3 bg-white border-2 border-black shadow-[2px_2px_0px_rgba(0,0,0,1)]">
                <Plus className="w-8 h-8 text-black stroke-[3]" />
              </div>
              <div>
                <span className="font-black tracking-wider text-sm block">CREATE NEW BOARD</span>
                <span className="font-mono text-[10px] text-gray-500 block mt-1">START A FRESH PROJECT</span>
              </div>
            </div>

          </div>
        )}
      </main>

      {/* CREATE BOARD MODAL */}
      <Modal
        isOpen={isCreateOpen}
        onClose={() => {
          setIsCreateOpen(false);
          setNewBoardTitle('');
          setNewBoardIsPublic(false);
        }}
        title="CREATE NEW BOARD 📌"
      >
        <form onSubmit={handleCreateBoard} className="flex flex-col gap-4">
          <Input
            label="Board Title"
            type="text"
            placeholder="e.g. Project Launch, Summer Vacation"
            value={newBoardTitle}
            onChange={(e) => setNewBoardTitle(e.target.value)}
            disabled={createLoading}
            autoFocus
          />

          <div className="flex flex-col gap-1 w-full">
            <label className="font-mono text-xs font-bold text-black tracking-wide">
              Board Visibility
            </label>
            <div className="flex gap-3 mt-1">
              <button
                type="button"
                onClick={() => setNewBoardIsPublic(false)}
                className={`flex items-center justify-center gap-1.5 flex-1 py-2 px-3 border-2 border-black font-mono text-xs font-bold transition-all shadow-[2px_2px_0px_rgba(0,0,0,1)] active:translate-x-[1px] active:translate-y-[1px] ${
                  !newBoardIsPublic ? 'bg-[#FF8FAB] text-black shadow-none translate-x-[1px] translate-y-[1px]' : 'bg-white text-gray-500'
                }`}
              >
                <Lock className="w-3.5 h-3.5" />
                <span>Private</span>
              </button>
              <button
                type="button"
                onClick={() => setNewBoardIsPublic(true)}
                className={`flex items-center justify-center gap-1.5 flex-1 py-2 px-3 border-2 border-black font-mono text-xs font-bold transition-all shadow-[2px_2px_0px_rgba(0,0,0,1)] active:translate-x-[1px] active:translate-y-[1px] ${
                  newBoardIsPublic ? 'bg-[#6EE7B7] text-black shadow-none translate-x-[1px] translate-y-[1px]' : 'bg-white text-gray-500'
                }`}
              >
                <Globe className="w-3.5 h-3.5" />
                <span>Public</span>
              </button>
            </div>
            <p className="font-mono text-[9px] text-gray-500 mt-1">
              {newBoardIsPublic ? 'Public boards can be seen by anyone visiting this URL.' : 'Private boards are only accessible to added members.'}
            </p>
          </div>

          <div className="flex justify-end gap-3 mt-2">
            <Button
              type="button"
              variant="info"
              onClick={() => {
                setIsCreateOpen(false);
                setNewBoardTitle('');
                setNewBoardIsPublic(false);
              }}
              disabled={createLoading}
            >
              CANCEL
            </Button>
            <Button
              type="submit"
              variant="success"
              disabled={createLoading}
            >
              {createLoading ? 'CREATING...' : 'CREATE BOARD'}
            </Button>
          </div>
        </form>
      </Modal>

      {/* EDIT BOARD MODAL */}
      <Modal
        isOpen={isEditOpen}
        onClose={() => {
          setIsEditOpen(false);
          setEditingBoard(null);
        }}
        title={
          <>
            <Settings className="w-5 h-5 text-indigo-600" />
            <span>BOARD SETTINGS</span>
          </>
        }
      >
        <form onSubmit={handleEditBoard} className="flex flex-col gap-4">
          <Input
            label="Board Title"
            type="text"
            placeholder="e.g. Marketing Board"
            value={editBoardTitle}
            onChange={(e) => setEditBoardTitle(e.target.value)}
            disabled={editLoading}
            autoFocus
          />

          <div className="flex flex-col gap-1 w-full">
            <label className="font-mono text-xs font-bold text-black tracking-wide">
              Board Visibility
            </label>
            <div className="flex gap-3 mt-1">
              <button
                type="button"
                onClick={() => setEditBoardIsPublic(false)}
                className={`flex items-center justify-center gap-1.5 flex-1 py-2 px-3 border-2 border-black font-mono text-xs font-bold transition-all shadow-[2px_2px_0px_rgba(0,0,0,1)] active:translate-x-[1px] active:translate-y-[1px] ${
                  !editBoardIsPublic ? 'bg-[#FF8FAB] text-black shadow-none translate-x-[1px] translate-y-[1px]' : 'bg-white text-gray-500'
                }`}
              >
                <Lock className="w-3.5 h-3.5" />
                <span>Private</span>
              </button>
              <button
                type="button"
                onClick={() => setEditBoardIsPublic(true)}
                className={`flex items-center justify-center gap-1.5 flex-1 py-2 px-3 border-2 border-black font-mono text-xs font-bold transition-all shadow-[2px_2px_0px_rgba(0,0,0,1)] active:translate-x-[1px] active:translate-y-[1px] ${
                  editBoardIsPublic ? 'bg-[#6EE7B7] text-black shadow-none translate-x-[1px] translate-y-[1px]' : 'bg-white text-gray-500'
                }`}
              >
                <Globe className="w-3.5 h-3.5" />
                <span>Public</span>
              </button>
            </div>
          </div>

          <div className="flex justify-end gap-3 mt-2">
            <Button
              type="button"
              variant="info"
              onClick={() => {
                setIsEditOpen(false);
                setEditingBoard(null);
              }}
              disabled={editLoading}
            >
              CANCEL
            </Button>
            <Button
              type="submit"
              variant="success"
              disabled={editLoading}
            >
              {editLoading ? 'SAVING...' : 'SAVE CHANGES'}
            </Button>
          </div>
        </form>
      </Modal>

      {/* DELETE CONFIRMATION MODAL */}
      <Modal
        isOpen={isDeleteOpen}
        onClose={() => {
          setIsDeleteOpen(false);
          setDeletingBoard(null);
        }}
        title={
          <>
            <AlertTriangle className="w-5 h-5 text-[#FF6B6B]" />
            <span>DANGER ZONE</span>
          </>
        }
      >
        <div className="flex flex-col gap-4">
          <div className="flex gap-3 items-start border-2 border-black bg-red-50 p-4 shadow-[3px_3px_0px_rgba(0,0,0,1)]">
            <ShieldAlert className="w-8 h-8 text-[#FF6B6B] shrink-0 stroke-[2.5]" />
            <div>
              <h4 className="font-black text-sm">Delete Board "{deletingBoard?.title}"?</h4>
              <p className="font-mono text-[11px] text-gray-700 mt-1 leading-relaxed">
                This action is <strong>permanent</strong> and cannot be undone! This will delete the board along with all its columns, lists, cards, and associated metadata.
              </p>
            </div>
          </div>
          
          <div className="flex justify-end gap-3 mt-2">
            <Button
              type="button"
              variant="info"
              onClick={() => {
                setIsDeleteOpen(false);
                setDeletingBoard(null);
              }}
              disabled={deleteLoading}
            >
              CANCEL
            </Button>
            <Button
              type="button"
              variant="danger"
              onClick={handleDeleteBoard}
              disabled={deleteLoading}
              className="flex items-center gap-1.5"
            >
              <Trash2 className="w-4 h-4" />
              <span>{deleteLoading ? 'DELETING...' : 'YES, PERMANENTLY DELETE'}</span>
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  );
};
