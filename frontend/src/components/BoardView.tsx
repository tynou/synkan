/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
 */

import React, { useState, useEffect, useRef, useMemo } from 'react';
import { Card, Button, Modal, Input, Badge } from './NeobrutalistComponents';
import { api, getApiBaseUrl } from '../api';
import * as signalR from '@microsoft/signalr';
import { BoardDto, UserDto, ColumnDto, CardDto, ChecklistDto, AccessLevel, AiProvider, ChecklistItemDto, LabelDto, MessageDto } from '../types';
import { 
  ArrowLeft, 
  Plus, 
  Trash2, 
  UserPlus, 
  Users, 
  Edit3, 
  Check, 
  X, 
  Search,
  LayoutGrid,
  CreditCard,
  Crown,
  UserX,
  FileText,
  ShieldAlert,
  Lock,
  Globe,
  AlertTriangle,
  CheckSquare,
  ListTodo,
  Palette,
  AlignLeft,
  MessageSquare,
  Send,
  Sparkles,
  Square,
  Settings,
  ChevronLeft,
  ChevronRight,
  MoreHorizontal,
  Calendar,
  Tag,
  Clock
} from 'lucide-react';

interface BoardViewProps {
  boardId: string;
  onBack: () => void;
  showToast: (message: string, type: 'success' | 'error' | 'info') => void;
  currentUser: UserDto | null;
}

export const BoardView: React.FC<BoardViewProps> = ({
  boardId,
  onBack,
  showToast,
  currentUser,
}) => {
  const [board, setBoard] = useState<BoardDto | null>(null);
  const [loading, setLoading] = useState(true);

  // Permissions helpers
  const myAccessLevel = board && currentUser
    ? (board.ownerId === currentUser.id ? 2 : (board.members?.find(m => m.userId === currentUser.id)?.accessLevel ?? 0))
    : 0;

  const canEdit = myAccessLevel >= 1;
  const canManage = myAccessLevel === 2 || (board && currentUser && board.ownerId === currentUser.id);

  // Determine Owner Username (if owner matches current user, display "Me" or we can default to 'Owner')
  const isOwner = board && currentUser ? board.ownerId === currentUser.id : false;
  const ownerMember = board?.members?.find(m => m.userId === board.ownerId);
  const boardOwnerUsername = board
    ? (ownerMember?.username 
        ? ownerMember.username 
        : (isOwner ? (currentUser?.username || 'Owner') : `User (${board.ownerId.slice(0, 6)})`))
    : '';

  // Combine owner and members for display in the participants list
  const allParticipants = useMemo(() => {
    if (!board) return [];
    
    const list: { userId: string; username: string; accessLevel: number; isOwner: boolean }[] = [];
    
    // Add owner
    list.push({
      userId: board.ownerId,
      username: boardOwnerUsername || 'Owner',
      accessLevel: 2, // Owner is always Admin (2)
      isOwner: true
    });
    
    // Add members
    if (board.members) {
      board.members.forEach(member => {
        // Avoid duplicating owner if they are already in members
        if (member.userId !== board.ownerId) {
          list.push({
            userId: member.userId,
            username: member.username || `User (${member.userId.slice(0, 6)})`,
            accessLevel: member.accessLevel,
            isOwner: false
          });
        }
      });
    }
    
    return list;
  }, [board, boardOwnerUsername]);

  const getAccessLevelLabel = (level: number): string => {
    switch (level) {
      case 2: return 'Admin';
      case 1: return 'Member';
      case 0:
      default: return 'Viewer';
    }
  };

  // Edit Board Title State
  const [isEditingTitle, setIsEditingTitle] = useState(false);
  const [tempBoardTitle, setTempBoardTitle] = useState('');

  // Add Member Modal State
  const [isMemberModalOpen, setIsMemberModalOpen] = useState(false);
  const [userSearchQuery, setUserSearchQuery] = useState('');
  const [searchResults, setSearchResults] = useState<UserDto[]>([]);
  const [searchLoading, setSearchLoading] = useState(false);
  const [addMemberLoadingId, setAddMemberLoadingId] = useState<string | null>(null);

  // Remove Member Confirmation State
  const [isRemoveMemberOpen, setIsRemoveMemberOpen] = useState(false);
  const [memberToRemove, setMemberToRemove] = useState<UserDto | null>(null);
  const [removeMemberLoading, setRemoveMemberLoading] = useState(false);

  // Create Column State
  const [newColumnTitle, setNewColumnTitle] = useState('');
  const [columnLoading, setColumnLoading] = useState(false);

  // Column Inline Editing State
  const [editingColumnId, setEditingColumnId] = useState<string | null>(null);
  const [tempColumnTitle, setTempColumnTitle] = useState('');

  // Delete Column Modal State (replaces confirm alert)
  const [isDeleteColOpen, setIsDeleteColOpen] = useState(false);
  const [colToDelete, setColToDelete] = useState<{ id: string; title: string } | null>(null);
  const [deleteColLoading, setDeleteColLoading] = useState(false);

  // New Card Inputs State (Keyed by Column ID)
  const [newCardTitles, setNewCardTitles] = useState<Record<string, string>>({});
  const [activeCardInputColId, setActiveCardInputColId] = useState<string | null>(null);
  const [cardLoadingStates, setCardLoadingStates] = useState<Record<string, boolean>>({});

  // Card Detail & Edit Modal State
  const [isCardDetailOpen, setIsCardDetailOpen] = useState(false);
  const [activeCard, setActiveCard] = useState<CardDto | null>(null);
  const [editCardTitle, setEditCardTitle] = useState('');
  const [editCardDescription, setEditCardDescription] = useState('');
  const [cardUpdateLoading, setCardUpdateLoading] = useState(false);

  // Card Color Modal and Modes State
  const [isColorModalOpen, setIsColorModalOpen] = useState(false);
  const [cardColorModes, setCardColorModes] = useState<Record<string, 'accent' | 'full'>>(() => {
    const initial: Record<string, 'accent' | 'full'> = {};
    try {
      for (let i = 0; i < localStorage.length; i++) {
        const key = localStorage.key(i);
        if (key && key.startsWith('card_color_mode_')) {
          const cardId = key.replace('card_color_mode_', '');
          const val = localStorage.getItem(key);
          if (val === 'full' || val === 'accent') {
            initial[cardId] = val;
          }
        }
      }
    } catch (e) {
      console.error(e);
    }
    return initial;
  });

  // Checklist States
  const [newChecklistTitle, setNewChecklistTitle] = useState('');
  const [newChecklistItemTexts, setNewChecklistItemTexts] = useState<Record<string, string>>({});

  // Card Due Date & Reminder States
  const [isDueDateModalOpen, setIsDueDateModalOpen] = useState(false);
  const [dueDateInput, setDueDateInput] = useState('');
  const [dueTimeInput, setDueTimeInput] = useState('');
  const [reminderDateInput, setReminderDateInput] = useState('');
  const [reminderTimeInput, setReminderTimeInput] = useState('');
  const [dueDateLoading, setDueDateLoading] = useState(false);

  // Card Labels States
  const [isLabelsModalOpen, setIsLabelsModalOpen] = useState(false);
  const [newLabelName, setNewLabelName] = useState('');
  const [newLabelColor, setNewLabelColor] = useState('#6EE7B7'); // Default green
  const [labelsLoading, setLabelsLoading] = useState(false);

  // Board AI Settings States
  const [aiApiKey, setAiApiKey] = useState('');
  const [aiProvider, setAiProvider] = useState<AiProvider>(AiProvider.Unknown);
  const [aiModel, setAiModel] = useState('');
  const [aiSettingsLoading, setAiSettingsLoading] = useState(false);
  const [aiSettingsSaving, setAiSettingsSaving] = useState(false);

  // Drag and Drop State
  const [draggingCardId, setDraggingCardId] = useState<string | null>(null);
  const [draggingColumnId, setDraggingColumnId] = useState<string | null>(null);
  const [dragOverColumnId, setDragOverColumnId] = useState<string | null>(null);
  const [dragOverCardId, setDragOverCardId] = useState<string | null>(null);
  const [dragOverPosition, setDragOverPosition] = useState<'before' | 'after' | 'bottom'>('before');

  // Drag and Drop Refs for robust real-time tracking
  const draggingCardIdRef = useRef<string | null>(null);
  const draggingColumnIdRef = useRef<string | null>(null);

  // Chat Window States & Persistent Storage
  const [isChatOpen, setIsChatOpen] = useState(true);
  const [chatMessages, setChatMessages] = useState<any[]>(() => {
    try {
      const stored = localStorage.getItem(`chat_messages_${boardId}`);
      if (stored) {
        const parsed = JSON.parse(stored);
        return parsed.map((m: any) => ({
          ...m,
          timestamp: new Date(m.timestamp),
          isStreaming: false
        }));
      }
    } catch (e) {
      console.error('Failed to parse chat messages:', e);
    }
    return [
      {
        id: 'welcome',
        sender: 'system',
        text: 'Welcome to Board Chat! Send messages to talk with the board AI assistant.',
        timestamp: new Date()
      }
    ];
  });
  const [chatInput, setChatInput] = useState('');
  const [isChatStreaming, setIsChatStreaming] = useState(false);
  const [isAiProcessing, setIsAiProcessing] = useState(false);
  const chatEndRef = useRef<HTMLDivElement | null>(null);
  const hubConnectionRef = useRef<signalR.HubConnection | null>(null);

  const [isRightPanelOpen, setIsRightPanelOpen] = useState(true);
  const [rightPanelTab, setRightPanelTab] = useState<'chat' | 'members' | 'settings'>('chat');

  useEffect(() => {
    try {
      localStorage.setItem(`chat_messages_${boardId}`, JSON.stringify(chatMessages));
    } catch (e) {
      console.error(e);
    }
  }, [chatMessages, boardId]);

  // Real-time local state updates from SignalR events
  const moveCardLocally = (cardId: string, targetColumnId: string, targetPosition: number) => {
    setBoard(prevBoard => {
      if (!prevBoard || !prevBoard.columns) return prevBoard;

      // 1. Find the card and its source
      let sourceColIdx = -1;
      let cardIdx = -1;
      let draggedCard: CardDto | null = null;

      for (let i = 0; i < prevBoard.columns.length; i++) {
        const cIdx = prevBoard.columns[i].cards?.findIndex(c => c.id === cardId) ?? -1;
        if (cIdx !== -1) {
          sourceColIdx = i;
          cardIdx = cIdx;
          draggedCard = prevBoard.columns[i].cards![cIdx];
          break;
        }
      }

      if (sourceColIdx === -1 || cardIdx === -1 || !draggedCard) return prevBoard;

      const targetColIdx = prevBoard.columns.findIndex(c => c.id === targetColumnId);
      if (targetColIdx === -1) return prevBoard;

      // 2. Clone columns
      const newColumns = prevBoard.columns.map(col => ({
        ...col,
        cards: col.cards ? [...col.cards] : []
      }));

      // Remove from source
      newColumns[sourceColIdx].cards.splice(cardIdx, 1);

      // Update columnId
      const updatedCard = { ...draggedCard, columnId: targetColumnId };

      // Insert card into target column at targetPosition
      const targetCards = newColumns[targetColIdx].cards;
      const insertIdx = Math.max(0, Math.min(targetPosition, targetCards.length));
      targetCards.splice(insertIdx, 0, updatedCard);

      // Re-index position fields
      newColumns[targetColIdx].cards = targetCards.map((c, idx) => ({
        ...c,
        position: idx
      }));

      if (sourceColIdx !== targetColIdx) {
        newColumns[sourceColIdx].cards = newColumns[sourceColIdx].cards.map((c, idx) => ({
          ...c,
          position: idx
        }));
      }

      return {
        ...prevBoard,
        columns: newColumns
      };
    });
  };

  const createCardLocally = (card: CardDto) => {
    setBoard(prevBoard => {
      if (!prevBoard || !prevBoard.columns) return prevBoard;

      // Idempotency: skip if already present
      const exists = prevBoard.columns.some(col => col.cards?.some(c => c.id === card.id));
      if (exists) return prevBoard;

      const newColumns = prevBoard.columns.map(col => {
        if (col.id === card.columnId) {
          const cards = col.cards ? [...col.cards] : [];
          const insertIdx = Math.max(0, Math.min(card.position ?? cards.length, cards.length));
          cards.splice(insertIdx, 0, card);

          const reindexed = cards.map((c, idx) => ({
            ...c,
            position: idx
          }));

          return {
            ...col,
            cards: reindexed
          };
        }
        return col;
      });

      return {
        ...prevBoard,
        columns: newColumns
      };
    });
  };

  const updateCardContentLocally = (cardId: string, title: string, description: string) => {
    setBoard(prev => {
      if (!prev || !prev.columns) return prev;
      return {
        ...prev,
        columns: prev.columns.map(col => ({
          ...col,
          cards: col.cards?.map(card => {
            if (card.id === cardId) {
              return { ...card, title, description };
            }
            return card;
          }) ?? null
        }))
      };
    });
    // Update activeCard detail modal if currently open for this card
    setActiveCard(prev => {
      if (prev && prev.id === cardId) {
        return { ...prev, title, description };
      }
      return prev;
    });
  };

  const updateCardCoverLocally = (cardId: string, color: string | null) => {
    setBoard(prev => {
      if (!prev || !prev.columns) return prev;
      return {
        ...prev,
        columns: prev.columns.map(col => ({
          ...col,
          cards: col.cards?.map(card => {
            if (card.id === cardId) {
              return { ...card, coverColor: color };
            }
            return card;
          }) ?? null
        }))
      };
    });
    setActiveCard(prev => {
      if (prev && prev.id === cardId) {
        return { ...prev, coverColor: color };
      }
      return prev;
    });
  };

  const createChecklistLocally = (cardId: string, checklist: ChecklistDto) => {
    setBoard(prev => {
      if (!prev || !prev.columns) return prev;
      return {
        ...prev,
        columns: prev.columns.map(col => ({
          ...col,
          cards: col.cards?.map(card => {
            if (card.id === cardId) {
              const currentChecklists = card.checklists || [];
              return {
                ...card,
                checklists: [...currentChecklists, checklist]
              };
            }
            return card;
          }) ?? null
        }))
      };
    });
    setActiveCard(prev => {
      if (prev && prev.id === cardId) {
        const currentChecklists = prev.checklists || [];
        return {
          ...prev,
          checklists: [...currentChecklists, checklist]
        };
      }
      return prev;
    });
  };

  const deleteChecklistLocally = (cardId: string, checklistId: string) => {
    setBoard(prev => {
      if (!prev || !prev.columns) return prev;
      return {
        ...prev,
        columns: prev.columns.map(col => ({
          ...col,
          cards: col.cards?.map(card => {
            if (card.id === cardId) {
              const currentChecklists = card.checklists || [];
              return {
                ...card,
                checklists: currentChecklists.filter(c => c.id !== checklistId)
              };
            }
            return card;
          }) ?? null
        }))
      };
    });
    setActiveCard(prev => {
      if (prev && prev.id === cardId) {
        const currentChecklists = prev.checklists || [];
        return {
          ...prev,
          checklists: currentChecklists.filter(c => c.id !== checklistId)
        };
      }
      return prev;
    });
  };

  const toggleChecklistItemLocally = (cardId: string, checklistId: string, itemId: string, isCompleted?: boolean) => {
    setBoard(prev => {
      if (!prev || !prev.columns) return prev;
      return {
        ...prev,
        columns: prev.columns.map(col => ({
          ...col,
          cards: col.cards?.map(card => {
            if (card.id === cardId) {
              const currentChecklists = card.checklists || [];
              return {
                ...card,
                checklists: currentChecklists.map(c => {
                  if (c.id === checklistId) {
                    const currentItems = c.items || [];
                    return {
                      ...c,
                      items: currentItems.map(item => {
                        if (item.id === itemId) {
                          const nextVal = isCompleted !== undefined ? isCompleted : !item.isCompleted;
                          return { ...item, isCompleted: nextVal };
                        }
                        return item;
                      })
                    };
                  }
                  return c;
                })
              };
            }
            return card;
          }) ?? null
        }))
      };
    });
    setActiveCard(prev => {
      if (prev && prev.id === cardId) {
        const currentChecklists = prev.checklists || [];
        return {
          ...prev,
          checklists: currentChecklists.map(c => {
            if (c.id === checklistId) {
              const currentItems = c.items || [];
              return {
                ...c,
                items: currentItems.map(item => {
                  if (item.id === itemId) {
                    const nextVal = isCompleted !== undefined ? isCompleted : !item.isCompleted;
                    return { ...item, isCompleted: nextVal };
                  }
                  return item;
                })
              };
            }
            return c;
          })
        };
      }
      return prev;
    });
  };

  const deleteChecklistItemLocally = (cardId: string, checklistId: string, itemId: string) => {
    setBoard(prev => {
      if (!prev || !prev.columns) return prev;
      return {
        ...prev,
        columns: prev.columns.map(col => ({
          ...col,
          cards: col.cards?.map(card => {
            if (card.id === cardId) {
              const currentChecklists = card.checklists || [];
              return {
                ...card,
                checklists: currentChecklists.map(c => {
                  if (c.id === checklistId) {
                    const currentItems = c.items || [];
                    return {
                      ...c,
                      items: currentItems.filter(item => item.id !== itemId)
                    };
                  }
                  return c;
                })
              };
            }
            return card;
          }) ?? null
        }))
      };
    });
    setActiveCard(prev => {
      if (prev && prev.id === cardId) {
        const currentChecklists = prev.checklists || [];
        return {
          ...prev,
          checklists: currentChecklists.map(c => {
            if (c.id === checklistId) {
              const currentItems = c.items || [];
              return {
                ...c,
                items: currentItems.filter(item => item.id !== itemId)
              };
            }
            return c;
          })
        };
      }
      return prev;
    });
  };

  const deleteCardLocally = (cardId: string) => {
    setBoard(prev => {
      if (!prev || !prev.columns) return prev;
      return {
        ...prev,
        columns: prev.columns.map(col => ({
          ...col,
          cards: col.cards?.filter(card => card.id !== cardId) ?? null
        }))
      };
    });
    // Close the card detail modal if open for this card
    setActiveCard(prev => {
      if (prev && prev.id === cardId) {
        setIsCardDetailOpen(false);
        return null;
      }
      return prev;
    });
  };

  const updateColumnTitleLocally = (columnId: string, title: string) => {
    setBoard(prev => {
      if (!prev || !prev.columns) return prev;
      return {
        ...prev,
        columns: prev.columns.map(col => {
          if (col.id === columnId) {
            return { ...col, title, Title: title };
          }
          return col;
        })
      };
    });
  };

  const moveColumnLocally = (columnId: string, position: number) => {
    setBoard(prev => {
      if (!prev || !prev.columns) return prev;
      const cols = [...prev.columns];
      const colIdx = cols.findIndex(c => c.id === columnId);
      if (colIdx === -1) return prev;
      const [col] = cols.splice(colIdx, 1);
      const targetIdx = Math.max(0, Math.min(position, cols.length));
      cols.splice(targetIdx, 0, col);
      const reindexed = cols.map((c, idx) => ({
        ...c,
        position: idx
      }));
      return {
        ...prev,
        columns: reindexed
      };
    });
  };

  const deleteColumnLocally = (columnId: string) => {
    setBoard(prev => {
      if (!prev || !prev.columns) return prev;
      return {
        ...prev,
        columns: prev.columns.filter(col => col.id !== columnId)
      };
    });
  };

  const createColumnLocally = (column: ColumnDto) => {
    setBoard(prev => {
      if (!prev || !prev.columns) return prev;
      if (prev.columns.some(col => col.id === column.id)) return prev;
      return {
        ...prev,
        columns: [...prev.columns, { ...column, cards: column.cards || [] }]
      };
    });
  };

  const updateBoardTitleLocally = (targetBoardId: string, title: string) => {
    if (targetBoardId !== boardId) return;
    setBoard(prev => {
      if (!prev) return prev;
      return { ...prev, title };
    });
  };

  const changeBoardVisibilityLocally = (targetBoardId: string, isPublic: boolean) => {
    if (targetBoardId !== boardId) return;
    setBoard(prev => {
      if (!prev) return prev;
      return { ...prev, isPublic };
    });
  };

  const createChecklistItemLocally = (checklistId: string, item: ChecklistItemDto) => {
    setBoard(prev => {
      if (!prev || !prev.columns) return prev;
      return {
        ...prev,
        columns: prev.columns.map(col => ({
          ...col,
          cards: col.cards?.map(card => {
            const hasChecklist = card.checklists?.some(cl => cl.id === checklistId);
            if (hasChecklist) {
              return {
                ...card,
                checklists: card.checklists!.map(cl => {
                  if (cl.id === checklistId) {
                    const currentItems = cl.items || [];
                    if (currentItems.some(i => i.id === item.id)) return cl;
                    return {
                      ...cl,
                      items: [...currentItems, item]
                    };
                  }
                  return cl;
                })
              };
            }
            return card;
          }) ?? null
        }))
      };
    });
    setActiveCard(prev => {
      if (prev) {
        const hasChecklist = prev.checklists?.some(cl => cl.id === checklistId);
        if (hasChecklist) {
          return {
            ...prev,
            checklists: prev.checklists!.map(cl => {
              if (cl.id === checklistId) {
                const currentItems = cl.items || [];
                if (currentItems.some(i => i.id === item.id)) return cl;
                return {
                  ...cl,
                  items: [...currentItems, item]
                };
              }
              return cl;
            })
          };
        }
      }
      return prev;
    });
  };

  const assignCardLabelLocally = (cardId: string, labelId: string) => {
    setBoard(prev => {
      if (!prev || !prev.columns) return prev;
      const label = prev.labels?.find(lbl => lbl.id === labelId);
      if (!label) return prev;
      return {
        ...prev,
        columns: prev.columns.map(col => ({
          ...col,
          cards: col.cards?.map(card => {
            if (card.id === cardId) {
              const currentLabels = card.labels || [];
              if (currentLabels.some(l => l.id === labelId)) return card;
              return {
                ...card,
                labels: [...currentLabels, label]
              };
            }
            return card;
          }) ?? null
        }))
      };
    });
    setActiveCard(prev => {
      if (prev && prev.id === cardId) {
        const label = board?.labels?.find(lbl => lbl.id === labelId);
        if (label) {
          const currentLabels = prev.labels || [];
          if (currentLabels.some(l => l.id === labelId)) return prev;
          return {
            ...prev,
            labels: [...currentLabels, label]
          };
        }
      }
      return prev;
    });
  };

  const removeCardLabelLocally = (cardId: string, labelId: string) => {
    setBoard(prev => {
      if (!prev || !prev.columns) return prev;
      return {
        ...prev,
        columns: prev.columns.map(col => ({
          ...col,
          cards: col.cards?.map(card => {
            if (card.id === cardId) {
              const currentLabels = card.labels || [];
              return {
                ...card,
                labels: currentLabels.filter(l => l.id !== labelId)
              };
            }
            return card;
          }) ?? null
        }))
      };
    });
    setActiveCard(prev => {
      if (prev && prev.id === cardId) {
        const currentLabels = prev.labels || [];
        return {
          ...prev,
          labels: currentLabels.filter(l => l.id !== labelId)
        };
      }
      return prev;
    });
  };

  const createLabelLocally = (label: LabelDto) => {
    setBoard(prev => {
      if (!prev) return prev;
      const currentLabels = prev.labels || [];
      if (currentLabels.some(l => l.id === label.id)) return prev;
      return {
        ...prev,
        labels: [...currentLabels, label]
      };
    });
  };

  const streamingTimeoutRef = useRef<NodeJS.Timeout | null>(null);

  const handleIncomingChunk = (messageId: string, chunk: string) => {
    setIsChatStreaming(true);
    
    // Clear existing timeout
    if (streamingTimeoutRef.current) {
      clearTimeout(streamingTimeoutRef.current);
    }
    
    // Set a new timeout of 10 seconds of silence as fallback to prevent getting stuck
    streamingTimeoutRef.current = setTimeout(() => {
      setIsChatStreaming(false);
      setChatMessages(prev => {
        return prev.map(msg => {
          if (msg.isStreaming) {
            return { ...msg, isStreaming: false };
          }
          return msg;
        });
      });
    }, 10000);

    setChatMessages(prev => {
      const existingIndex = prev.findIndex(m => m.id === messageId);
      if (existingIndex !== -1) {
        const updated = [...prev];
        updated[existingIndex] = {
          ...updated[existingIndex],
          text: (updated[existingIndex].text || '') + chunk,
          isStreaming: true
        };
        return updated;
      } else {
        return [
          ...prev,
          {
            id: messageId,
            sender: 'assistant',
            text: chunk,
            timestamp: new Date(),
            isStreaming: true
          }
        ];
      }
    });
  };

  const handleMessageCompleted = (messageId: string) => {
    if (streamingTimeoutRef.current) {
      clearTimeout(streamingTimeoutRef.current);
    }
    setIsChatStreaming(false);
    setChatMessages(prev => {
      return prev.map(m => {
        if (m.id === messageId || m.isStreaming) {
          return { ...m, isStreaming: false };
        }
        return m;
      });
    });
  };

  // SignalR Hub Connection Setup
  useEffect(() => {
    let connection: signalR.HubConnection | null = null;
    let isMounted = true;

    async function startSignalR() {
      try {
        const baseUrl = getApiBaseUrl();
        const hubUrl = `${baseUrl}/hubs/board`;

        connection = new signalR.HubConnectionBuilder()
          .withUrl(hubUrl, {
            withCredentials: true // Support HttpOnly cookie JWT transmission
          })
          .withAutomaticReconnect()
          .build();

        // Register handlers for both AsyncAPI specification casing and user convenience casing
        // --- CHAT MESSAGE EVENTS ---
        connection.on('OnMessageChunk', (event: { boardId: string; messageId: string; chunk: string }) => {
          if (isMounted) {
            console.log('SignalR: OnMessageChunk received:', event);
            if (event.boardId === boardId) {
              handleIncomingChunk(event.messageId, event.chunk);
            }
          }
        });

        connection.on('MessageChunk', (event: { boardId: string; messageId: string; chunk: string }) => {
          if (isMounted) {
            console.log('SignalR: MessageChunk received:', event);
            if (event.boardId === boardId) {
              handleIncomingChunk(event.messageId, event.chunk);
            }
          }
        });

        connection.on('OnMessageCompleted', (event: { boardId: string; messageId: string }) => {
          if (isMounted) {
            console.log('SignalR: OnMessageCompleted received:', event);
            if (event.boardId === boardId) {
              handleMessageCompleted(event.messageId);
            }
          }
        });

        connection.on('MessageCompleted', (event: { boardId: string; messageId: string }) => {
          if (isMounted) {
            console.log('SignalR: MessageCompleted received:', event);
            if (event.boardId === boardId) {
              handleMessageCompleted(event.messageId);
            }
          }
        });

        // --- CARD EVENTS ---
        connection.on('OnCardMoved', (event: { cardId: string; columnId: string; position: number }) => {
          if (isMounted) {
            console.log('SignalR: OnCardMoved event received:', event);
            moveCardLocally(event.cardId, event.columnId, event.position);
          }
        });

        connection.on('CardMoved', (event: { cardId: string; columnId: string; position: number }) => {
          if (isMounted) {
            console.log('SignalR: CardMoved event received:', event);
            moveCardLocally(event.cardId, event.columnId, event.position);
          }
        });

        connection.on('OnCardCreated', (card: CardDto) => {
          if (isMounted) {
            console.log('SignalR: OnCardCreated event received:', card);
            createCardLocally(card);
          }
        });

        connection.on('CardCreated', (card: CardDto) => {
          if (isMounted) {
            console.log('SignalR: CardCreated event received:', card);
            createCardLocally(card);
          }
        });

        connection.on('OnCardContentUpdated', (event: { cardId: string; title: string; description: string }) => {
          if (isMounted) {
            console.log('SignalR: OnCardContentUpdated received:', event);
            updateCardContentLocally(event.cardId, event.title, event.description);
          }
        });

        connection.on('CardContentUpdated', (event: { cardId: string; title: string; description: string }) => {
          if (isMounted) {
            console.log('SignalR: CardContentUpdated received:', event);
            updateCardContentLocally(event.cardId, event.title, event.description);
          }
        });

        connection.on('OnCardCoverUpdated', (event: { cardId: string; color: string | null }) => {
          if (isMounted) {
            console.log('SignalR: OnCardCoverUpdated received:', event);
            updateCardCoverLocally(event.cardId, event.color);
          }
        });

        connection.on('CardCoverUpdated', (event: { cardId: string; color: string | null }) => {
          if (isMounted) {
            console.log('SignalR: CardCoverUpdated received:', event);
            updateCardCoverLocally(event.cardId, event.color);
          }
        });

        connection.on('OnCardDeleted', (event: { cardId: string }) => {
          if (isMounted) {
            console.log('SignalR: OnCardDeleted received:', event);
            deleteCardLocally(event.cardId);
          }
        });

        connection.on('CardDeleted', (event: { cardId: string }) => {
          if (isMounted) {
            console.log('SignalR: CardDeleted received:', event);
            deleteCardLocally(event.cardId);
          }
        });

        // --- COLUMN EVENTS ---
        connection.on('OnColumnTitleUpdated', (event: { columnId: string; title: string }) => {
          if (isMounted) {
            console.log('SignalR: OnColumnTitleUpdated received:', event);
            updateColumnTitleLocally(event.columnId, event.title);
          }
        });

        connection.on('ColumnTitleUpdated', (event: { columnId: string; title: string }) => {
          if (isMounted) {
            console.log('SignalR: ColumnTitleUpdated received:', event);
            updateColumnTitleLocally(event.columnId, event.title);
          }
        });

        connection.on('OnColumnMoved', (event: { columnId: string; position: number }) => {
          if (isMounted) {
            console.log('SignalR: OnColumnMoved received:', event);
            moveColumnLocally(event.columnId, event.position);
          }
        });

        connection.on('ColumnMoved', (event: { columnId: string; position: number }) => {
          if (isMounted) {
            console.log('SignalR: ColumnMoved received:', event);
            moveColumnLocally(event.columnId, event.position);
          }
        });

        connection.on('OnColumnDeleted', (event: { columnId: string }) => {
          if (isMounted) {
            console.log('SignalR: OnColumnDeleted received:', event);
            deleteColumnLocally(event.columnId);
          }
        });

        connection.on('ColumnDeleted', (event: { columnId: string }) => {
          if (isMounted) {
            console.log('SignalR: ColumnDeleted received:', event);
            deleteColumnLocally(event.columnId);
          }
        });

        connection.on('OnColumnCreated', (column: ColumnDto) => {
          if (isMounted) {
            console.log('SignalR: OnColumnCreated received:', column);
            createColumnLocally(column);
          }
        });

        connection.on('ColumnCreated', (column: ColumnDto) => {
          if (isMounted) {
            console.log('SignalR: ColumnCreated received:', column);
            createColumnLocally(column);
          }
        });

        // --- BOARD EVENTS ---
        connection.on('OnBoardTitleUpdated', (event: { boardId: string; title: string }) => {
          if (isMounted) {
            console.log('SignalR: OnBoardTitleUpdated received:', event);
            updateBoardTitleLocally(event.boardId, event.title);
          }
        });

        connection.on('BoardTitleUpdated', (event: { boardId: string; title: string }) => {
          if (isMounted) {
            console.log('SignalR: BoardTitleUpdated received:', event);
            updateBoardTitleLocally(event.boardId, event.title);
          }
        });

        connection.on('OnBoardVisibilityChanged', (event: { boardId: string; isPublic: boolean }) => {
          if (isMounted) {
            console.log('SignalR: OnBoardVisibilityChanged received:', event);
            changeBoardVisibilityLocally(event.boardId, event.isPublic);
          }
        });

        connection.on('BoardVisibilityChanged', (event: { boardId: string; isPublic: boolean }) => {
          if (isMounted) {
            console.log('SignalR: BoardVisibilityChanged received:', event);
            changeBoardVisibilityLocally(event.boardId, event.isPublic);
          }
        });

        connection.on('OnBoardDeleted', (event: { boardId: string }) => {
          if (isMounted && event.boardId === boardId) {
            console.log('SignalR: OnBoardDeleted received:', event);
            showToast('This board has been deleted by another user.', 'error');
            onBack();
          }
        });

        connection.on('BoardDeleted', (event: { boardId: string }) => {
          if (isMounted && event.boardId === boardId) {
            console.log('SignalR: BoardDeleted received:', event);
            showToast('This board has been deleted by another user.', 'error');
            onBack();
          }
        });

        // --- PROCESSING EVENTS ---
        connection.on('OnProcessingStarted', (event: { boardId: string }) => {
          if (isMounted && event.boardId === boardId) {
            console.log('SignalR: OnProcessingStarted received:', event);
            setIsAiProcessing(true);
            setIsChatStreaming(true);
          }
        });

        connection.on('ProcessingStarted', (event: { boardId: string }) => {
          if (isMounted && event.boardId === boardId) {
            console.log('SignalR: ProcessingStarted received:', event);
            setIsAiProcessing(true);
            setIsChatStreaming(true);
          }
        });

        connection.on('OnProcessingCompleted', (event: { boardId: string }) => {
          if (isMounted && event.boardId === boardId) {
            console.log('SignalR: OnProcessingCompleted received:', event);
            setIsAiProcessing(false);
            setIsChatStreaming(false);
            setChatMessages(prev => prev.map(m => m.isStreaming ? { ...m, isStreaming: false } : m));
          }
        });

        connection.on('ProcessingCompleted', (event: { boardId: string }) => {
          if (isMounted && event.boardId === boardId) {
            console.log('SignalR: ProcessingCompleted received:', event);
            setIsAiProcessing(false);
            setIsChatStreaming(false);
            setChatMessages(prev => prev.map(m => m.isStreaming ? { ...m, isStreaming: false } : m));
          }
        });

        connection.on('OnProcessingFailed', (event: { boardId: string }) => {
          if (isMounted && event.boardId === boardId) {
            console.log('SignalR: OnProcessingFailed received:', event);
            setIsAiProcessing(false);
            setIsChatStreaming(false);
            setChatMessages(prev => {
              const lastMsg = prev[prev.length - 1];
              if (lastMsg && lastMsg.sender === 'assistant' && lastMsg.isStreaming) {
                return [
                  ...prev.slice(0, -1),
                  {
                    ...lastMsg,
                    isStreaming: false,
                    text: lastMsg.text + '\n\n*(Failed to complete processing)*'
                  }
                ];
              }
              return prev.map(m => m.isStreaming ? { ...m, isStreaming: false } : m);
            });
            showToast('AI assistant failed to process your request.', 'error');
          }
        });

        connection.on('ProcessingFailed', (event: { boardId: string }) => {
          if (isMounted && event.boardId === boardId) {
            console.log('SignalR: ProcessingFailed received:', event);
            setIsAiProcessing(false);
            setIsChatStreaming(false);
            setChatMessages(prev => {
              const lastMsg = prev[prev.length - 1];
              if (lastMsg && lastMsg.sender === 'assistant' && lastMsg.isStreaming) {
                return [
                  ...prev.slice(0, -1),
                  {
                    ...lastMsg,
                    isStreaming: false,
                    text: lastMsg.text + '\n\n*(Failed to complete processing)*'
                  }
                ];
              }
              return prev.map(m => m.isStreaming ? { ...m, isStreaming: false } : m);
            });
            showToast('AI assistant failed to process your request.', 'error');
          }
        });

        // --- REAL-TIME CHECKLIST EVENTS ---
        connection.on('OnChecklistCreated', (checklist: ChecklistDto) => {
          if (isMounted) {
            console.log('SignalR: OnChecklistCreated received:', checklist);
            createChecklistLocally(checklist.cardId, checklist);
          }
        });

        connection.on('ChecklistCreated', (checklist: ChecklistDto) => {
          if (isMounted) {
            console.log('SignalR: ChecklistCreated received:', checklist);
            createChecklistLocally(checklist.cardId, checklist);
          }
        });

        connection.on('OnChecklistItemCreated', (event: any) => {
          if (isMounted) {
            console.log('SignalR: OnChecklistItemCreated received:', event);
            const item = event.item || event;
            const clId = item.checklistId || event.checklistId;
            if (clId && item) {
              createChecklistItemLocally(clId, item);
            }
          }
        });

        connection.on('ChecklistItemCreated', (event: any) => {
          if (isMounted) {
            console.log('SignalR: ChecklistItemCreated received:', event);
            const item = event.item || event;
            const clId = item.checklistId || event.checklistId;
            if (clId && item) {
              createChecklistItemLocally(clId, item);
            }
          }
        });

        connection.on('OnChecklistItemToggled', (event: { cardId: string; checklistId: string; itemId: string; isCompleted: boolean }) => {
          if (isMounted) {
            console.log('SignalR: OnChecklistItemToggled received:', event);
            toggleChecklistItemLocally(event.cardId, event.checklistId, event.itemId, event.isCompleted);
          }
        });

        connection.on('ChecklistItemToggled', (event: { cardId: string; checklistId: string; itemId: string; isCompleted: boolean }) => {
          if (isMounted) {
            console.log('SignalR: ChecklistItemToggled received:', event);
            toggleChecklistItemLocally(event.cardId, event.checklistId, event.itemId, event.isCompleted);
          }
        });

        connection.on('OnChecklistDeleted', (event: { cardId: string; checklistId: string }) => {
          if (isMounted) {
            console.log('SignalR: OnChecklistDeleted received:', event);
            deleteChecklistLocally(event.cardId, event.checklistId);
          }
        });

        connection.on('ChecklistDeleted', (event: { cardId: string; checklistId: string }) => {
          if (isMounted) {
            console.log('SignalR: ChecklistDeleted received:', event);
            deleteChecklistLocally(event.cardId, event.checklistId);
          }
        });

        connection.on('OnChecklistItemDeleted', (event: { cardId: string; checklistId: string; itemId: string }) => {
          if (isMounted) {
            console.log('SignalR: OnChecklistItemDeleted received:', event);
            deleteChecklistItemLocally(event.cardId, event.checklistId, event.itemId);
          }
        });

        connection.on('ChecklistItemDeleted', (event: { cardId: string; checklistId: string; itemId: string }) => {
          if (isMounted) {
            console.log('SignalR: ChecklistItemDeleted received:', event);
            deleteChecklistItemLocally(event.cardId, event.checklistId, event.itemId);
          }
        });

        // --- REAL-TIME LABEL EVENTS ---
        connection.on('OnCardLabelAssigned', (event: { cardId: string; labelId: string }) => {
          if (isMounted) {
            console.log('SignalR: OnCardLabelAssigned received:', event);
            assignCardLabelLocally(event.cardId, event.labelId);
          }
        });

        connection.on('CardLabelAssigned', (event: { cardId: string; labelId: string }) => {
          if (isMounted) {
            console.log('SignalR: CardLabelAssigned received:', event);
            assignCardLabelLocally(event.cardId, event.labelId);
          }
        });

        connection.on('OnCardLabelRemoved', (event: { cardId: string; labelId: string }) => {
          if (isMounted) {
            console.log('SignalR: OnCardLabelRemoved received:', event);
            removeCardLabelLocally(event.cardId, event.labelId);
          }
        });

        connection.on('CardLabelRemoved', (event: { cardId: string; labelId: string }) => {
          if (isMounted) {
            console.log('SignalR: CardLabelRemoved received:', event);
            removeCardLabelLocally(event.cardId, event.labelId);
          }
        });

        connection.on('OnLabelCreated', (label: LabelDto) => {
          if (isMounted) {
            console.log('SignalR: OnLabelCreated received:', label);
            createLabelLocally(label);
          }
        });

        connection.on('LabelCreated', (label: LabelDto) => {
          if (isMounted) {
            console.log('SignalR: LabelCreated received:', label);
            createLabelLocally(label);
          }
        });

        // --- REAL-TIME MESSAGE SENT EVENTS ---
        connection.on('OnMessageSent', (m: MessageDto) => {
          if (isMounted) {
            console.log('SignalR: OnMessageSent received:', m);
            setChatMessages(prev => {
              if (prev.some(msg => msg.id === m.messageId)) return prev;
              let sender: 'user' | 'assistant' | 'system' = 'assistant';
              if (m.role === 1) {
                sender = 'user';
              } else if (m.role === 2) {
                sender = 'assistant';
              } else if (m.role === 3) {
                sender = 'system';
              } else {
                sender = 'system';
              }

              if (sender === 'user') {
                const tempIndex = prev.findIndex(msg => msg.id.startsWith('user-') && msg.text.trim() === (m.content || '').trim());
                if (tempIndex !== -1) {
                  const nextMessages = [...prev];
                  nextMessages[tempIndex] = {
                    ...nextMessages[tempIndex],
                    id: m.messageId,
                    text: m.content || '',
                    isStreaming: false
                  };
                  return nextMessages;
                }
              }

              return [
                ...prev,
                {
                  id: m.messageId,
                  sender,
                  username: sender === 'user' ? 'You' : (sender === 'system' ? 'SYSTEM' : 'BOARD AI'),
                  text: m.content || '',
                  timestamp: new Date(),
                  isStreaming: false
                }
              ];
            });
          }
        });

        connection.on('MessageSent', (m: MessageDto) => {
          if (isMounted) {
            console.log('SignalR: MessageSent received:', m);
            setChatMessages(prev => {
              if (prev.some(msg => msg.id === m.messageId)) return prev;
              let sender: 'user' | 'assistant' | 'system' = 'assistant';
              if (m.role === 1) {
                sender = 'user';
              } else if (m.role === 2) {
                sender = 'assistant';
              } else if (m.role === 3) {
                sender = 'system';
              } else {
                sender = 'system';
              }

              if (sender === 'user') {
                const tempIndex = prev.findIndex(msg => msg.id.startsWith('user-') && msg.text.trim() === (m.content || '').trim());
                if (tempIndex !== -1) {
                  const nextMessages = [...prev];
                  nextMessages[tempIndex] = {
                    ...nextMessages[tempIndex],
                    id: m.messageId,
                    text: m.content || '',
                    isStreaming: false
                  };
                  return nextMessages;
                }
              }

              return [
                ...prev,
                {
                  id: m.messageId,
                  sender,
                  username: sender === 'user' ? 'You' : (sender === 'system' ? 'SYSTEM' : 'BOARD AI'),
                  text: m.content || '',
                  timestamp: new Date(),
                  isStreaming: false
                }
              ];
            });
          }
        });

        await connection.start();
        console.log('SignalR: Connection established successfully!');

        // Invoke JoinBoard with the active board's ID
        if (isMounted) {
          hubConnectionRef.current = connection;
          await connection.invoke('JoinBoard', boardId);
          console.log(`SignalR: Invited JoinBoard successfully for board ID ${boardId}`);
        }
      } catch (err) {
        console.error('SignalR Initialization Error:', err);
      }
    }

    startSignalR();

    return () => {
      isMounted = false;
      hubConnectionRef.current = null;
      if (connection) {
        const conn = connection;
        conn.invoke('LeaveBoard', boardId)
          .catch(err => console.warn('SignalR LeaveBoard failed:', err))
          .finally(() => {
            conn.stop()
              .then(() => console.log('SignalR: Hub connection stopped.'))
              .catch(err => console.error('SignalR stop failed:', err));
          });
      }
    };
  }, [boardId]);

  const handleSendMessage = async (e?: React.FormEvent) => {
    if (e) e.preventDefault();
    if (!chatInput.trim()) return;

    const userMessageText = chatInput.trim();
    setChatInput('');

    const userMsg = {
      id: `user-${Date.now()}`,
      sender: 'user',
      username: currentUser?.username || 'You',
      text: userMessageText,
      timestamp: new Date()
    };

    setChatMessages(prev => {
      const cleaned = prev.map(m => m.isStreaming ? { ...m, isStreaming: false } : m);
      return [...cleaned, userMsg];
    });

    if (hubConnectionRef.current) {
      try {
        setIsChatStreaming(true);
        console.log('SignalR: Sending message via SendMessage:', userMessageText);
        await hubConnectionRef.current.send('SendMessage', {
          boardId: boardId,
          message: userMessageText
        });
      } catch (err: any) {
        console.error('SignalR SendMessage failed:', err);
        showToast(err.message || 'Failed to send message over SignalR', 'error');
        setIsChatStreaming(false);
      }
    } else {
      showToast('Chat is not connected', 'error');
    }
  };

  const handleCancelProcessing = async () => {
    if (hubConnectionRef.current) {
      try {
        await hubConnectionRef.current.send('CancelProcessing', { boardId: boardId });
        setIsChatStreaming(false);
        setIsAiProcessing(false);
        setChatMessages(prev => {
          const lastMsg = prev[prev.length - 1];
          if (lastMsg && lastMsg.sender === 'assistant' && lastMsg.isStreaming) {
            return [
              ...prev.slice(0, -1),
              {
                ...lastMsg,
                isStreaming: false,
                text: lastMsg.text + ' (Cancelled)'
              }
            ];
          }
          return prev;
        });
        showToast('Processing cancelled', 'info');
      } catch (err) {
        console.error('Failed to cancel processing:', err);
      }
    }
  };

  const handleClearChat = () => {
    setChatMessages([
      {
        id: 'welcome',
        sender: 'system',
        text: 'Chat cleared. Ask me anything about your tasks, columns, or cards.',
        timestamp: new Date()
      }
    ]);
  };

  useEffect(() => {
    chatEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [chatMessages, isChatOpen]);

  const fetchBoardDetails = async (silent = false) => {
    if (!silent) setLoading(true);
    try {
      const data = await api.getBoardById(boardId);
      // Sort columns and cards by position if available in DTO
      if (data && data.columns) {
        data.columns.sort((a, b) => (a.position ?? 0) - (b.position ?? 0));
        data.columns.forEach(col => {
          if (col.cards) {
            col.cards.sort((a, b) => (a.position ?? 0) - (b.position ?? 0));
          }
        });
      }
      setBoard(data);
      setTempBoardTitle(data.title || '');
    } catch (err: any) {
      console.error(err);
      showToast(err.message || 'Failed to load board details', 'error');
    } finally {
      if (!silent) setLoading(false);
    }
  };

  const fetchChatHistory = async () => {
    try {
      const response = await api.getBoardChat(boardId);
      if (response && response.messages && response.messages.length > 0) {
        const mapped = response.messages.map((m) => {
          let sender: 'user' | 'assistant' | 'system' = 'assistant';
          // ChatMessageRole: 1 - User, 2 - Ai, 3 - System, 0 - Unknown
          if (m.role === 1) {
            sender = 'user';
          } else if (m.role === 2) {
            sender = 'assistant';
          } else if (m.role === 3) {
            sender = 'system';
          } else {
            sender = 'system';
          }
          return {
            id: m.messageId,
            sender,
            username: sender === 'user' ? (currentUser?.username || 'You') : (sender === 'system' ? 'SYSTEM' : 'BOARD AI'),
            text: m.content || '',
            timestamp: new Date(),
            isStreaming: false
          };
        });
        setChatMessages(mapped);
      } else {
        setChatMessages([
          {
            id: 'welcome',
            sender: 'system',
            text: 'Welcome to Board Chat! Ask me anything about your tasks, columns, or cards.',
            timestamp: new Date()
          }
        ]);
      }
    } catch (err) {
      console.error('Failed to fetch board chat history:', err);
    }
  };

  const fetchAiSettings = async () => {
    setAiSettingsLoading(true);
    try {
      const data = await api.getBoardAiSettings(boardId);
      if (data) {
        setAiApiKey(data.apiKey || '');
        setAiProvider(data.provider ?? AiProvider.Unknown);
        setAiModel(data.model || '');
      }
    } catch (err: any) {
      console.error('Failed to fetch board AI settings:', err);
    } finally {
      setAiSettingsLoading(false);
    }
  };

  useEffect(() => {
    fetchBoardDetails();
    fetchChatHistory();
    fetchAiSettings();
  }, [boardId]);

  // Rename Board Title
  const handleRenameBoardSubmit = async () => {
    if (!tempBoardTitle.trim()) {
      showToast('Board title cannot be empty', 'error');
      return;
    }
    if (tempBoardTitle.trim() === board?.title) {
      setIsEditingTitle(false);
      return;
    }

    try {
      await api.editBoardTitle(boardId, { 
        title: tempBoardTitle.trim()
      });
      showToast('Board renamed successfully!', 'success');
      setBoard(prev => {
        if (!prev) return prev;
        return {
          ...prev,
          title: tempBoardTitle.trim()
        };
      });
      setIsEditingTitle(false);
    } catch (err: any) {
      showToast(err.message || 'Failed to rename board', 'error');
    }
  };

  // Save AI Settings
  const handleSaveAiSettings = async (e: React.FormEvent) => {
    e.preventDefault();
    setAiSettingsSaving(true);
    try {
      await api.updateBoardAiSettings(boardId, {
        apiKey: aiApiKey.trim() || null,
        provider: Number(aiProvider) as AiProvider,
        model: aiModel.trim() || null,
      });
      showToast('AI settings updated successfully!', 'success');
      await fetchAiSettings();
    } catch (err: any) {
      showToast(err.message || 'Failed to save AI settings', 'error');
    } finally {
      setAiSettingsSaving(false);
    }
  };

  // Create Column
  const handleCreateColumn = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newColumnTitle.trim()) {
      showToast('Column title is required', 'error');
      return;
    }

    setColumnLoading(true);
    try {
      const res = await api.createColumn({
        boardId,
        title: newColumnTitle.trim(),
      });
      showToast(`Column "${newColumnTitle}" added!`, 'success');
      
      const newCol: ColumnDto = {
        id: res.id,
        boardId,
        title: newColumnTitle.trim(),
        cards: [],
        position: board?.columns?.length ?? 0
      };
      setBoard(prev => {
        if (!prev) return prev;
        const cols = prev.columns || [];
        if (cols.some(c => c.id === res.id)) return prev;
        return {
          ...prev,
          columns: [...cols, newCol]
        };
      });
      setNewColumnTitle('');
    } catch (err: any) {
      showToast(err.message || 'Failed to create column', 'error');
    } finally {
      setColumnLoading(false);
    }
  };

  // Open Delete Column modal instead of prompt
  const handleRequestDeleteColumn = (columnId: string, title: string) => {
    setColToDelete({ id: columnId, title });
    setIsDeleteColOpen(true);
  };

  // Delete Column Confirmed
  const handleDeleteColumnConfirm = async () => {
    if (!colToDelete) return;
    setDeleteColLoading(true);
    try {
      await api.deleteColumn(colToDelete.id);
      showToast(`Column "${colToDelete.title}" deleted`, 'success');
      setBoard(prev => {
        if (!prev) return prev;
        return {
          ...prev,
          columns: (prev.columns || []).filter(c => c.id !== colToDelete.id)
        };
      });
      setIsDeleteColOpen(false);
      setColToDelete(null);
    } catch (err: any) {
      showToast(err.message || 'Failed to delete column', 'error');
    } finally {
      setDeleteColLoading(false);
    }
  };

  // Rename Column
  const handleRenameColumnSubmit = async (columnId: string) => {
    if (!tempColumnTitle.trim()) {
      showToast('Column title is required', 'error');
      return;
    }

    const originalColumn = board?.columns?.find(c => c.id === columnId);
    const originalTitle = originalColumn?.title || originalColumn?.Title || '';
    if (tempColumnTitle.trim() === originalTitle.trim()) {
      setEditingColumnId(null);
      return;
    }

    try {
      await api.editColumnTitle(columnId, { 
        title: tempColumnTitle.trim()
      });
      showToast('Column renamed', 'success');
      setBoard(prev => {
        if (!prev) return prev;
        return {
          ...prev,
          columns: (prev.columns || []).map(c => 
            c.id === columnId 
              ? { ...c, title: tempColumnTitle.trim(), Title: tempColumnTitle.trim() } 
              : c
          )
        };
      });
      setEditingColumnId(null);
    } catch (err: any) {
      showToast(err.message || 'Failed to rename column', 'error');
    }
  };

  // Create Card inside Column
  const handleCreateCard = async (columnId: string) => {
    const cardTitle = newCardTitles[columnId]?.trim();
    if (!cardTitle) {
      showToast('Card title cannot be empty', 'error');
      return;
    }

    setCardLoadingStates(prev => ({ ...prev, [columnId]: true }));
    try {
      const res = await api.createCard({
        columnId,
        title: cardTitle,
      });
      showToast('Card added!', 'success');
      
      const newCard: CardDto = {
        id: res.id,
        columnId,
        title: cardTitle,
        description: '',
        position: (board?.columns?.find(c => c.id === columnId)?.cards?.length) ?? 0,
        coverColor: null,
        checklists: []
      };
      setBoard(prev => {
        if (!prev) return prev;
        
        // Idempotency check: verify if the card already exists in any column
        const exists = prev.columns?.some(col => col.cards?.some(c => c.id === res.id));
        if (exists) return prev;

        return {
          ...prev,
          columns: (prev.columns || []).map(col => {
            if (col.id === columnId) {
              const cards = col.cards || [];
              if (cards.some(c => c.id === res.id)) return col;
              return {
                ...col,
                cards: [...cards, newCard]
              };
            }
            return col;
          })
        };
      });

      setNewCardTitles(prev => ({ ...prev, [columnId]: '' }));
      setActiveCardInputColId(null);
    } catch (err: any) {
      showToast(err.message || 'Failed to create card', 'error');
    } finally {
      setCardLoadingStates(prev => ({ ...prev, [columnId]: false }));
    }
  };

  // Delete Card
  const handleDeleteCard = async (cardId: string) => {
    try {
      await api.deleteCard(cardId);
      showToast('Card removed', 'success');
      setBoard(prev => {
        if (!prev) return prev;
        return {
          ...prev,
          columns: (prev.columns || []).map(col => ({
            ...col,
            cards: (col.cards || []).filter(card => card.id !== cardId)
          }))
        };
      });
      // If the card details modal is currently open for this card, close it
      if (activeCard && activeCard.id === cardId) {
        setIsCardDetailOpen(false);
        setActiveCard(null);
      }
    } catch (err: any) {
      showToast(err.message || 'Failed to remove card', 'error');
    }
  };

  // Open Card Detail Modal
  const handleOpenCardDetail = async (card: CardDto) => {
    setActiveCard(card);
    setEditCardTitle(card.title || '');
    setEditCardDescription(card.description || '');
    setIsCardDetailOpen(true);

    try {
      const fullCard = await api.getCard(card.id);
      setActiveCard(fullCard);
      setEditCardTitle(fullCard.title || '');
      setEditCardDescription(fullCard.description || '');
    } catch (err) {
      console.error('Failed to load full card details', err);
    }
  };

  // Save Card Details
  const handleSaveCardDetail = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!activeCard) return;
    if (!editCardTitle.trim()) {
      showToast('Card title cannot be empty', 'error');
      return;
    }

    const titleChanged = editCardTitle.trim() !== (activeCard.title || '').trim();
    const descriptionChanged = editCardDescription.trim() !== (activeCard.description || '').trim();

    if (!titleChanged && !descriptionChanged) {
      setIsCardDetailOpen(false);
      setActiveCard(null);
      return;
    }

    setCardUpdateLoading(true);
    try {
      await api.updateCardContent(activeCard.id, {
        title: editCardTitle.trim(),
        description: editCardDescription.trim()
      });
      showToast('Card updated successfully', 'success');
      setBoard(prev => {
        if (!prev) return prev;
        return {
          ...prev,
          columns: (prev.columns || []).map(col => {
            if (col.id === activeCard.columnId) {
              return {
                ...col,
                cards: (col.cards || []).map(card => {
                  if (card.id === activeCard.id) {
                    return {
                      ...card,
                      title: editCardTitle.trim(),
                      description: editCardDescription.trim()
                    };
                  }
                  return card;
                })
              };
            }
            return col;
          })
        };
      });
      setIsCardDetailOpen(false);
      setActiveCard(null);
    } catch (err: any) {
      showToast(err.message || 'Failed to update card details', 'error');
    } finally {
      setCardUpdateLoading(false);
    }
  };

  const handleUpdateCover = async (color: string | null) => {
    if (!activeCard) return;
    try {
      await api.updateCardCover(activeCard.id, { color });
      updateCardCoverLocally(activeCard.id, color);
      showToast('Card cover updated!', 'success');
    } catch (err: any) {
      showToast(err.message || 'Failed to update cover', 'error');
    }
  };

  const updateCardColorMode = (cardId: string, mode: 'accent' | 'full') => {
    try {
      localStorage.setItem(`card_color_mode_${cardId}`, mode);
    } catch (e) {
      console.error(e);
    }
    setCardColorModes(prev => ({
      ...prev,
      [cardId]: mode
    }));
  };

  // Unified Refresher for Active Card and Board State
  const refreshActiveCard = async (cardId: string) => {
    try {
      const cardDetail = await api.getCard(cardId);
      setActiveCard(cardDetail);
      setBoard(prev => {
        if (!prev) return prev;
        return {
          ...prev,
          columns: prev.columns?.map(col => {
            if (col.id !== cardDetail.columnId) return col;
            return {
              ...col,
              cards: col.cards?.map(c => c.id === cardId ? cardDetail : c) ?? null
            };
          }) ?? null
        };
      });
    } catch (err) {
      console.error('Failed to refresh active card', err);
    }
  };

  // Due Date & Deadline Handlers
  const handleOpenDueDateModal = () => {
    if (!activeCard) return;
    if (activeCard.dueDate) {
      const d = new Date(activeCard.dueDate);
      const yyyy = d.getFullYear();
      const mm = String(d.getMonth() + 1).padStart(2, '0');
      const dd = String(d.getDate()).padStart(2, '0');
      setDueDateInput(`${yyyy}-${mm}-${dd}`);
      
      const hh = String(d.getHours()).padStart(2, '0');
      const min = String(d.getMinutes()).padStart(2, '0');
      setDueTimeInput(`${hh}:${min}`);
    } else {
      setDueDateInput('');
      setDueTimeInput('');
    }

    if (activeCard.reminderDate) {
      const d = new Date(activeCard.reminderDate);
      const yyyy = d.getFullYear();
      const mm = String(d.getMonth() + 1).padStart(2, '0');
      const dd = String(d.getDate()).padStart(2, '0');
      setReminderDateInput(`${yyyy}-${mm}-${dd}`);
      
      const hh = String(d.getHours()).padStart(2, '0');
      const min = String(d.getMinutes()).padStart(2, '0');
      setReminderTimeInput(`${hh}:${min}`);
    } else {
      setReminderDateInput('');
      setReminderTimeInput('');
    }
    
    setIsDueDateModalOpen(true);
  };

  const handleSaveDueDate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!activeCard) return;
    if (!dueDateInput) {
      showToast('Please select a due date', 'error');
      return;
    }

    setDueDateLoading(true);
    try {
      const dueTime = dueTimeInput || '00:00';
      const localDue = new Date(`${dueDateInput}T${dueTime}`);
      const dueDateTimeStr = localDue.toISOString();

      let reminderDateTimeStr = '';
      if (reminderDateInput) {
        const reminderTime = reminderTimeInput || '00:00';
        const localReminder = new Date(`${reminderDateInput}T${reminderTime}`);
        reminderDateTimeStr = localReminder.toISOString();
      } else {
        reminderDateTimeStr = dueDateTimeStr;
      }

      await api.updateCardDueDate(activeCard.id, {
        dueDate: dueDateTimeStr,
        reminderTime: reminderDateTimeStr,
      });

      showToast('Card deadline saved!', 'success');
      await refreshActiveCard(activeCard.id);
      setIsDueDateModalOpen(false);
    } catch (err: any) {
      showToast(err.message || 'Failed to update due date', 'error');
    } finally {
      setDueDateLoading(false);
    }
  };

  const handleClearDueDate = async () => {
    if (!activeCard) return;
    setDueDateLoading(true);
    try {
      await api.deleteCardDueDate(activeCard.id);
      showToast('Card deadline cleared!', 'success');
      setDueDateInput('');
      setDueTimeInput('');
      setReminderDateInput('');
      setReminderTimeInput('');
      await refreshActiveCard(activeCard.id);
      setIsDueDateModalOpen(false);
    } catch (err: any) {
      showToast(err.message || 'Failed to clear due date', 'error');
    } finally {
      setDueDateLoading(false);
    }
  };

  // Label Handlers
  const handleCreateBoardLabel = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newLabelName.trim()) {
      showToast('Label name cannot be empty', 'error');
      return;
    }
    setLabelsLoading(true);
    try {
      await api.createBoardLabel(boardId, {
        name: newLabelName.trim(),
        color: newLabelColor
      });
      showToast(`Label "${newLabelName}" created!`, 'success');
      setNewLabelName('');
      // Reload board so we have the new label in board.labels
      const updatedBoard = await api.getBoardById(boardId);
      setBoard(prev => prev ? { ...prev, labels: updatedBoard.labels } : null);
    } catch (err: any) {
      showToast(err.message || 'Failed to create label', 'error');
    } finally {
      setLabelsLoading(false);
    }
  };

  const handleToggleCardLabel = async (labelId: string, isCurrentlyAssigned: boolean) => {
    if (!activeCard) return;
    try {
      if (isCurrentlyAssigned) {
        await api.removeCardLabel(activeCard.id, labelId);
        showToast('Label removed from card', 'success');
      } else {
        await api.addCardLabel(activeCard.id, labelId);
        showToast('Label added to card', 'success');
      }
      await refreshActiveCard(activeCard.id);
    } catch (err: any) {
      showToast(err.message || 'Failed to toggle label', 'error');
    }
  };

  const handleCreateChecklist = async () => {
    if (!activeCard) return;
    if (!newChecklistTitle.trim()) {
      showToast('Checklist title cannot be empty', 'error');
      return;
    }
    try {
      const title = newChecklistTitle.trim();
      const res = await api.createChecklist(activeCard.id, { title });
      const newChecklist: ChecklistDto = {
        id: res.id,
        cardId: activeCard.id,
        title,
        items: []
      };
      createChecklistLocally(activeCard.id, newChecklist);
      setNewChecklistTitle('');
      showToast(`Checklist "${title}" created!`, 'success');
    } catch (err: any) {
      showToast(err.message || 'Failed to create checklist', 'error');
    }
  };

  const handleDeleteChecklist = async (checklistId: string) => {
    if (!activeCard) return;
    try {
      await api.deleteChecklist(activeCard.id, checklistId);
      deleteChecklistLocally(activeCard.id, checklistId);
      showToast('Checklist removed', 'success');
    } catch (err: any) {
      showToast(err.message || 'Failed to delete checklist', 'error');
    }
  };

  const handleCreateChecklistItem = async (checklistId: string) => {
    if (!activeCard) return;
    const text = newChecklistItemTexts[checklistId]?.trim();
    if (!text) {
      showToast('Checklist item text cannot be empty', 'error');
      return;
    }
    try {
      await api.createChecklistItem(activeCard.id, checklistId, { text });
      
      // Fetch fresh details of this card to sync item IDs properly
      const cardDetail = await api.getCard(activeCard.id);
      setBoard(prev => {
        if (!prev || !prev.columns) return prev;
        return {
          ...prev,
          columns: prev.columns.map(col => ({
            ...col,
            cards: col.cards?.map(c => c.id === activeCard.id ? cardDetail : c) ?? null
          }))
        };
      });
      setActiveCard(cardDetail);
      setNewChecklistItemTexts(prev => ({ ...prev, [checklistId]: '' }));
      showToast('Item added!', 'success');
    } catch (err: any) {
      showToast(err.message || 'Failed to create checklist item', 'error');
    }
  };

  const handleToggleChecklistItem = async (checklistId: string, itemId: string, isCompleted: boolean) => {
    if (!activeCard) return;
    try {
      toggleChecklistItemLocally(activeCard.id, checklistId, itemId, isCompleted);
      await api.toggleChecklistItem(activeCard.id, checklistId, itemId, { isCompleted });
    } catch (err: any) {
      showToast(err.message || 'Failed to toggle checklist item', 'error');
      toggleChecklistItemLocally(activeCard.id, checklistId, itemId, !isCompleted);
    }
  };

  const handleDeleteChecklistItem = async (checklistId: string, itemId: string) => {
    if (!activeCard) return;
    try {
      await api.deleteChecklistItem(activeCard.id, checklistId, itemId);
      deleteChecklistItemLocally(activeCard.id, checklistId, itemId);
      showToast('Item removed', 'success');
    } catch (err: any) {
      showToast(err.message || 'Failed to delete checklist item', 'error');
    }
  };

  // Search users to add as board members
  const handleSearchUsers = async () => {
    if (!userSearchQuery.trim()) {
      setSearchResults([]);
      return;
    }

    setSearchLoading(true);
    try {
      const res = await api.searchUsers(userSearchQuery.trim());
      const usersList = res.users || [];
      // Filter out users who are already members or the owner
      const filtered = usersList.filter(user => {
        if (user.id === board?.ownerId) return false;
        const isAlreadyMember = board?.members?.some(m => m.userId === user.id);
        return !isAlreadyMember;
      });
      setSearchResults(filtered);
    } catch (err: any) {
      showToast('Failed to search users', 'error');
    } finally {
      setSearchLoading(false);
    }
  };

  // Add board member
  const handleAddBoardMember = async (memberId: string, memberUsername: string) => {
    setAddMemberLoadingId(memberId);
    try {
      await api.addBoardMember(boardId, { memberId });
      showToast(`User "${memberUsername}" added to the board!`, 'success');
      setBoard(prev => {
        if (!prev) return prev;
        const currentMembers = prev.members || [];
        const isAlreadyAdded = currentMembers.some(m => m.userId === memberId);
        if (isAlreadyAdded) return prev;
        const newMember = {
          userId: memberId,
          username: memberUsername,
          accessLevel: 0 // Default Viewer access level
        };
        return {
          ...prev,
          members: [...currentMembers, newMember]
        };
      });
      // Refresh results to omit added user
      setSearchResults(prev => prev.filter(u => u.id !== memberId));
    } catch (err: any) {
      showToast(err.message || 'Failed to add member', 'error');
    } finally {
      setAddMemberLoadingId(null);
    }
  };

  // Request Remove Member Modal
  const handleRequestRemoveMember = (user: UserDto) => {
    setMemberToRemove(user);
    setIsRemoveMemberOpen(true);
  };

  // Confirm Remove Member
  const handleRemoveMemberConfirm = async () => {
    if (!memberToRemove) return;
    setRemoveMemberLoading(true);
    try {
      await api.removeBoardMember(boardId, memberToRemove.id);
      showToast(`User "${memberToRemove.username}" removed from the board`, 'success');
      setBoard(prev => {
        if (!prev) return prev;
        return {
          ...prev,
          members: (prev.members || []).filter(m => m.userId !== memberToRemove.id)
        };
      });
      setIsRemoveMemberOpen(false);
      setMemberToRemove(null);
    } catch (err: any) {
      showToast(err.message || 'Failed to remove participant', 'error');
    } finally {
      setRemoveMemberLoading(false);
    }
  };

  // Change access level
  const handleUpdateAccessLevel = async (memberId: string, newLevel: number) => {
    try {
      await api.updateMemberAccessLevel(boardId, memberId, { newAccessLevel: newLevel });
      showToast('Member access level updated successfully!', 'success');
      setBoard(prev => {
        if (!prev) return prev;
        return {
          ...prev,
          members: (prev.members || []).map(m => m.userId === memberId ? { ...m, accessLevel: newLevel } : m)
        };
      });
    } catch (err: any) {
      showToast(err.message || 'Failed to update access level', 'error');
    }
  };

  // Trigger search on typing (debounced or on Enter key)
  useEffect(() => {
    const delayDebounceFn = setTimeout(() => {
      if (userSearchQuery) {
        handleSearchUsers();
      } else {
        setSearchResults([]);
      }
    }, 300);

    return () => clearTimeout(delayDebounceFn);
  }, [userSearchQuery]);

  // CARD DRAG AND DROP HANDLERS
  const handleCardDragStart = (e: React.DragEvent, cardId: string, columnId: string) => {
    if (!canEdit) {
      e.preventDefault();
      return;
    }
    e.stopPropagation(); // Prevents column drag handler from intercepting card dragging
    draggingCardIdRef.current = cardId;
    setDraggingCardId(cardId);
    e.dataTransfer.setData('text/plain', cardId);
    e.dataTransfer.effectAllowed = 'move';
  };

  const handleCardDragEnd = () => {
    draggingCardIdRef.current = null;
    setDraggingCardId(null);
    setDragOverCardId(null);
    setDragOverColumnId(null);
  };

  const handleCardDragOver = (e: React.DragEvent, targetCardId: string, targetColumnId: string) => {
    e.preventDefault();
    e.stopPropagation();
    const dragId = draggingCardIdRef.current || draggingCardId;
    if (!dragId || dragId === targetCardId) return;

    // Determine if cursor is over upper half or lower half of target card
    const rect = e.currentTarget.getBoundingClientRect();
    const relativeY = e.clientY - rect.top;
    const isAfter = relativeY > rect.height / 2;

    setDragOverCardId(targetCardId);
    setDragOverColumnId(targetColumnId);
    setDragOverPosition(isAfter ? 'after' : 'before');
  };

  const handleColumnCardDragOver = (e: React.DragEvent, targetColumnId: string) => {
    e.preventDefault();
    const dragId = draggingCardIdRef.current || draggingCardId;
    if (!dragId) return;

    // Only set as 'bottom' if we are not hovering over a specific card
    setDragOverColumnId(targetColumnId);
    setDragOverCardId(null);
    setDragOverPosition('bottom');
  };

  const handleCardDrop = async (e: React.DragEvent, targetColumnId: string) => {
    e.preventDefault();
    e.stopPropagation();

    const cardId = draggingCardIdRef.current || draggingCardId;
    if (!cardId || !board?.columns) return;

    // Reset drag states immediately to give instant visual feedback
    draggingCardIdRef.current = null;
    setDraggingCardId(null);
    const savedDragOverCardId = dragOverCardId;
    const savedDragOverPosition = dragOverPosition;
    setDragOverCardId(null);
    setDragOverColumnId(null);

    // 1. Find the card and its source
    let sourceColIdx = -1;
    let cardIdx = -1;
    let draggedCard: CardDto | null = null;

    for (let i = 0; i < board.columns.length; i++) {
      const cIdx = board.columns[i].cards?.findIndex(c => c.id === cardId) ?? -1;
      if (cIdx !== -1) {
        sourceColIdx = i;
        cardIdx = cIdx;
        draggedCard = board.columns[i].cards![cIdx];
        break;
      }
    }

    if (sourceColIdx === -1 || cardIdx === -1 || !draggedCard) return;

    const targetColIdx = board.columns.findIndex(c => c.id === targetColumnId);
    if (targetColIdx === -1) return;

    // 2. Clone the column arrays
    const newColumns = board.columns.map(col => ({
      ...col,
      cards: col.cards ? [...col.cards] : []
    }));

    // Remove from source
    newColumns[sourceColIdx].cards.splice(cardIdx, 1);

    // Update columnId
    const updatedCard = { ...draggedCard, columnId: targetColumnId };

    // Find insertion index in target cards
    const targetCards = newColumns[targetColIdx].cards;
    let insertIdx = targetCards.length; // Default to bottom

    if (savedDragOverCardId) {
      const targetCardIdx = targetCards.findIndex(c => c.id === savedDragOverCardId);
      if (targetCardIdx !== -1) {
        insertIdx = savedDragOverPosition === 'after' ? targetCardIdx + 1 : targetCardIdx;
      }
    }

    // Insert card
    targetCards.splice(insertIdx, 0, updatedCard);

    // Re-index position fields
    newColumns[targetColIdx].cards = targetCards.map((c, idx) => ({
      ...c,
      position: idx
    }));

    if (sourceColIdx !== targetColIdx) {
      newColumns[sourceColIdx].cards = newColumns[sourceColIdx].cards.map((c, idx) => ({
        ...c,
        position: idx
      }));
    }

    // Get final position
    const finalPosition = newColumns[targetColIdx].cards.findIndex(c => c.id === cardId);

    // 3. Update local state
    setBoard({
      ...board,
      columns: newColumns
    });

    // 4. Save to backend
    try {
      await api.moveCard(cardId, {
        newColumnId: targetColumnId,
        newPosition: finalPosition !== -1 ? finalPosition : 0
      });
      showToast('Card moved successfully', 'success');
    } catch (err: any) {
      showToast(err.message || 'Failed to save card position', 'error');
      await fetchBoardDetails(); // Revert
    }
  };

  // COLUMN DRAG AND DROP HANDLERS
  const handleColumnDragStart = (e: React.DragEvent, columnId: string) => {
    if (!canEdit) {
      e.preventDefault();
      return;
    }
    const target = e.target as HTMLElement;
    if (target.closest('.group\\/card') || target.closest('button') || target.closest('input')) {
      e.preventDefault();
      return;
    }

    draggingColumnIdRef.current = columnId;
    setDraggingColumnId(columnId);
    e.dataTransfer.effectAllowed = 'move';
  };

  const handleColumnDragOver = (e: React.DragEvent, targetColumnId: string) => {
    e.preventDefault();
    const dragId = draggingColumnIdRef.current;
    if (!dragId || dragId === targetColumnId) return;

    // Real-time column reordering inside local state
    setBoard(prevBoard => {
      if (!prevBoard || !prevBoard.columns) return prevBoard;
      
      const cols = [...prevBoard.columns];
      const dragIdx = cols.findIndex(c => c.id === dragId);
      const hoverIdx = cols.findIndex(c => c.id === targetColumnId);
      
      if (dragIdx === -1 || hoverIdx === -1) return prevBoard;
      
      const [draggedCol] = cols.splice(dragIdx, 1);
      cols.splice(hoverIdx, 0, draggedCol);
      
      const updatedCols = cols.map((col, idx) => ({
        ...col,
        position: idx
      }));

      return {
        ...prevBoard,
        columns: updatedCols
      };
    });
  };

  const handleColumnDragEnd = async () => {
    const columnId = draggingColumnIdRef.current;
    if (!columnId) return;

    draggingColumnIdRef.current = null;
    setDraggingColumnId(null);
    setDragOverColumnId(null);

    const finalIndex = board?.columns?.findIndex(c => c.id === columnId);
    if (finalIndex === undefined || finalIndex === -1) return;

    try {
      await api.moveColumn(columnId, {
        newPosition: finalIndex
      });
      showToast('Column reordered successfully', 'success');
    } catch (err: any) {
      showToast(err.message || 'Failed to save column order', 'error');
      await fetchBoardDetails(); // Revert on failure
    }
  };

  const handleColumnDrop = (e: React.DragEvent) => {
    e.preventDefault();
  };


  if (loading) {
    return (
      <div className="min-h-screen bg-[#FFFBEB] flex flex-col items-center justify-center p-6 text-black">
        <div className="animate-spin rounded-none border-4 border-black border-t-transparent w-12 h-12 mb-4 bg-[#FF8FAB]" />
        <p className="font-mono text-sm font-bold text-gray-700 animate-pulse">
          Loading board collaboration stream...
        </p>
      </div>
    );
  }

  if (!board) {
    return (
      <div className="min-h-screen bg-[#FFFBEB] flex flex-col items-center justify-center p-6 text-black">
        <Card bgColor="bg-red-50" className="max-w-md w-full text-center flex flex-col gap-4">
          <h3 className="text-xl font-bold text-[#FF6B6B]">Board Not Found</h3>
          <p className="font-mono text-xs text-gray-600">
            The board you are looking for does not exist, or you lack the required authorization.
          </p>
          {!currentUser && (
            <p className="font-mono text-[11px] text-indigo-600 font-black">
              💡 Tip: This board might be private. Try logging in!
            </p>
          )}
          <Button variant="info" onClick={onBack}>
            {currentUser ? 'Go back to dashboard' : 'Go to login / register'}
          </Button>
        </Card>
      </div>
    );
  }

  const renderChatTab = () => {
    return (
      <div className="flex-1 flex flex-col h-full overflow-hidden">
        {/* Chat Messages List */}
        <div className="flex-1 overflow-y-auto p-4 flex flex-col gap-3.5 bg-[#FFFDF6]">
          {chatMessages.map((msg) => {
            if (msg.sender === 'system') {
              return (
                <div key={msg.id} className="text-center my-1.5 px-3 py-1.5 border border-black/20 bg-amber-50/50 text-black">
                  <p className="font-mono text-[10px] font-bold text-gray-600 leading-relaxed">
                    {msg.text}
                  </p>
                </div>
              );
            }

            const isUser = msg.sender === 'user';
            return (
              <div
                key={msg.id}
                className={`flex flex-col max-w-[85%] ${isUser ? 'self-end items-end' : 'self-start items-start'}`}
              >
                {/* Message Sender / Username */}
                <span className="font-mono text-[9px] font-bold text-black tracking-wider mb-1 px-1">
                  {isUser ? (msg.username || 'You') : 'Board AI'}
                </span>

                {/* Chat Bubble with Neobrutalist styling */}
                <div
                  className={`px-3 py-2.5 border-2 border-black transition-all text-xs font-semibold leading-normal shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)] ${
                    isUser 
                      ? 'bg-[#A5B4FC] text-black rounded-none' 
                      : 'bg-[#FFFCEE] text-black rounded-none'
                  }`}
                >
                  <p className="whitespace-pre-wrap font-sans font-bold select-text break-words leading-snug">
                    {msg.text}
                  </p>

                  {/* Display a small pulse cursor when streaming */}
                  {msg.isStreaming && (
                    <span className="inline-block w-1.5 h-3 ml-1 bg-black animate-pulse align-middle font-black">▋</span>
                  )}
                </div>
              </div>
            );
          })}
          
          {isAiProcessing && !chatMessages.some(m => m.isStreaming) && (
            <div className="flex flex-col max-w-[85%] self-start items-start">
              <span className="font-mono text-[9px] font-bold text-black tracking-wider mb-1 px-1">
                Board AI
              </span>
              <div className="px-3 py-2.5 border-2 border-black bg-[#FFFCEE] text-black transition-all text-xs font-semibold leading-normal shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)] rounded-none">
                <div className="flex items-center gap-2">
                  <span className="relative flex h-2 w-2">
                    <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-indigo-400 opacity-75"></span>
                    <span className="relative inline-flex rounded-full h-2 w-2 bg-indigo-500"></span>
                  </span>
                  <span className="font-mono text-[10px] font-bold animate-pulse text-indigo-600">AI is thinking/processing...</span>
                </div>
              </div>
            </div>
          )}
          
          <div ref={chatEndRef} />
        </div>

        {/* Chat Input form */}
        <div className="p-3 border-t-2 border-black bg-white shrink-0">
          <form onSubmit={handleSendMessage} className="flex gap-2">
            <input
              type="text"
              placeholder={
                isChatStreaming 
                  ? "AI is streaming..." 
                  : (isAiProcessing ? "AI is thinking..." : "Ask board assistant...")
              }
              value={chatInput}
              onChange={(e) => setChatInput(e.target.value)}
              className="flex-1 px-3 py-2 bg-[#FFFDF6] text-black font-semibold text-xs border-2 border-black shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)] focus:outline-none focus:bg-[#FFFCEE]"
              disabled={isChatStreaming || isAiProcessing}
            />
            
            {isChatStreaming || isAiProcessing ? (
              <button
                type="button"
                onClick={handleCancelProcessing}
                title="Stop Generating"
                className="p-2 border-2 border-black bg-red-400 hover:bg-red-300 shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)] active:translate-x-[0.5px] active:translate-y-[0.5px] transition-all flex items-center justify-center cursor-pointer"
              >
                <Square className="w-4 h-4 text-black fill-black" />
              </button>
            ) : (
              <button
                type="submit"
                disabled={!chatInput.trim()}
                title="Send Message"
                className={`p-2 border-2 border-black shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)] active:translate-x-[0.5px] active:translate-y-[0.5px] transition-all flex items-center justify-center cursor-pointer ${
                  chatInput.trim() 
                    ? 'bg-[#6EE7B7] hover:bg-[#A7F3D0] text-black' 
                    : 'bg-gray-100 text-gray-400 cursor-not-allowed shadow-[0.5px_0.5px_0px_rgba(0,0,0,1)]'
                }`}
              >
                <Send className="w-4 h-4" />
              </button>
            )}
          </form>
        </div>
      </div>
    );
  };

  const renderMembersTab = () => {
    return (
      <div className="flex-1 flex flex-col h-full overflow-hidden bg-[#FFFDF6]">
        {/* Members List Container */}
        <div className="flex-1 overflow-y-auto p-4 flex flex-col gap-4">
          {canManage && (
            <button
              onClick={() => {
                setIsMemberModalOpen(true);
                setUserSearchQuery('');
                setSearchResults([]);
              }}
              className="w-full py-2 border-2 border-black bg-[#FFDE4D] hover:bg-amber-300 text-black font-mono text-xs font-black cursor-pointer shadow-[2px_2px_0px_rgba(0,0,0,1)] active:translate-x-[0.5px] active:translate-y-[0.5px]"
            >
              + Add New Member
            </button>
          )}

          {/* Owner Details */}
          <div className="border-2 border-black p-3 bg-amber-50 shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)]">
            <div className="flex items-center gap-1.5 mb-1 font-mono text-xs font-bold text-gray-500">
              <Crown className="w-4 h-4 text-yellow-600 fill-yellow-400" />
              <span>Owner</span>
            </div>
            <p className="font-sans font-black text-sm truncate text-black bg-white px-2 py-1 border border-black inline-block">
              {boardOwnerUsername}
            </p>
          </div>

          {/* Visibility Section */}
          <div className="border-2 border-black p-3 bg-white shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)]">
            <div className="flex items-center justify-between">
              <div>
                <span className="font-mono text-[10px] font-bold text-gray-500 block">Board Visibility</span>
                <span className="font-mono text-xs font-black flex items-center gap-1 mt-1 text-black">
                  {board.isPublic ? (
                    <>
                      <Globe className="w-3.5 h-3.5 text-green-700 stroke-[2.5]" />
                      <span>Public (anyone can view)</span>
                    </>
                  ) : (
                    <>
                      <Lock className="w-3.5 h-3.5 text-indigo-700 stroke-[2.5]" />
                      <span>Private (members only)</span>
                    </>
                  )}
                </span>
              </div>
              {canManage && (
                <button
                  onClick={async () => {
                    try {
                      const targetVisibility = !board.isPublic;
                      await api.changeBoardVisibility(boardId, { newIsPublic: targetVisibility });
                      showToast(`Board is now ${targetVisibility ? 'PUBLIC' : 'PRIVATE'}!`, 'success');
                      setBoard(prev => {
                        if (!prev) return prev;
                        return { ...prev, isPublic: targetVisibility };
                      });
                    } catch (err: any) {
                      showToast(err.message || 'Failed to change visibility', 'error');
                    }
                  }}
                  className="px-2 py-1 border-2 border-black bg-[#FFDE4D] hover:bg-amber-300 text-[10px] font-black cursor-pointer shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)] active:translate-x-[0.5px] active:translate-y-[0.5px]"
                >
                  Change
                </button>
              )}
            </div>
          </div>

          {/* Collaborators / Members */}
          <div className="border-2 border-black p-3 bg-white shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)] flex-1 flex flex-col min-h-[180px]">
            <span className="font-mono text-[10px] font-bold text-gray-500 block mb-2">
              Collaborators ({board.members?.length || 0})
            </span>
            
            <div className="flex-1 overflow-y-auto flex flex-col gap-2 pr-1">
              {board.members && board.members.length > 0 ? (
                board.members.map((member) => (
                  <div 
                    key={member.userId}
                    className="p-2 border border-black bg-[#FFFDF6] flex items-center justify-between gap-2"
                  >
                    <div className="min-w-0 flex-1">
                      <p className="font-sans font-bold text-xs text-black truncate">
                        {member.username || member.userId.slice(0, 8)}
                      </p>
                      <span className="font-mono text-[9px] text-gray-500">
                        {getAccessLevelLabel(member.accessLevel)}
                      </span>
                    </div>

                    <div className="flex items-center gap-1.5 shrink-0">
                      {/* Access level editor for canManage */}
                      {canManage && (
                        <select
                          value={member.accessLevel}
                          onChange={async (e) => {
                            const newLevel = parseInt(e.target.value) as AccessLevel;
                            try {
                              await api.updateMemberAccessLevel(boardId, member.userId, { newAccessLevel: newLevel });
                              showToast('Access level updated!', 'success');
                              setBoard(prev => {
                                if (!prev || !prev.members) return prev;
                                return {
                                  ...prev,
                                  members: prev.members.map(m => m.userId === member.userId ? { ...m, accessLevel: newLevel } : m)
                                };
                              });
                            } catch (err: any) {
                              showToast(err.message || 'Failed to update access level', 'error');
                            }
                          }}
                          className="bg-white text-black text-[10px] font-bold border border-black px-1 py-0.5 outline-none font-mono"
                        >
                          <option value={AccessLevel.Viewer}>Viewer</option>
                          <option value={AccessLevel.Member}>Member</option>
                          <option value={AccessLevel.Admin}>Admin</option>
                        </select>
                      )}

                      {/* Remove member button */}
                      {canManage && (
                        <button
                          onClick={() => {
                            const userToRem: UserDto = { id: member.userId, username: member.username || 'Collaborator' };
                            handleRequestRemoveMember(userToRem);
                          }}
                          className="p-1 border border-black hover:bg-red-400 text-black transition-colors"
                          title="Remove from board"
                        >
                          <X className="w-3 h-3 stroke-[2.5]" />
                        </button>
                      )}
                    </div>
                  </div>
                ))
              ) : (
                <div className="text-center py-6">
                  <p className="font-mono text-[10px] text-gray-400 italic">No other members</p>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    );
  };

  const renderSettingsTab = () => {
    return (
      <div className="flex-1 flex flex-col h-full overflow-hidden bg-[#FFFDF6]">
        {/* Settings Content */}
        <div className="flex-1 overflow-y-auto p-4 flex flex-col gap-4">
          {/* Board Title Section */}
          <div className="border-2 border-black p-3 bg-white shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)]">
            <span className="font-mono text-[10px] font-bold text-gray-500 block mb-2">Rename Board</span>
            {canManage ? (
              <div className="flex flex-col gap-2">
                <input
                  type="text"
                  value={tempBoardTitle}
                  onChange={(e) => setTempBoardTitle(e.target.value)}
                  className="px-3 py-1.5 bg-white text-black font-bold text-sm border-2 border-black focus:outline-none shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)]"
                />
                <button
                  onClick={handleRenameBoardSubmit}
                  className="w-full py-1.5 border-2 border-black bg-[#6EE7B7] hover:bg-green-400 font-mono text-xs font-black shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)] active:translate-x-[0.5px] active:translate-y-[0.5px] transition-transform text-black cursor-pointer"
                >
                  Save Title
                </button>
              </div>
            ) : (
              <p className="font-sans font-bold text-sm text-black bg-gray-100 p-2 border border-black truncate">
                {board.title || 'Untitled Board'}
              </p>
            )}
          </div>

          {/* AI Configuration Section */}
          <div className="border-2 border-black p-3 bg-white shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)]">
            <span className="font-mono text-[10px] font-bold text-gray-500 block mb-2 flex items-center gap-1">
              <Sparkles className="w-3.5 h-3.5 text-indigo-500" />
              <span>AI CHAT ASSISTANT SETTINGS</span>
            </span>
            {aiSettingsLoading ? (
              <p className="font-mono text-xs text-gray-500 animate-pulse py-2">Loading settings...</p>
            ) : (
              <form onSubmit={handleSaveAiSettings} className="flex flex-col gap-3">
                <div className="flex flex-col gap-1">
                  <label className="font-mono text-[9px] font-black text-black">AI PROVIDER</label>
                  <select
                    value={aiProvider}
                    onChange={(e) => setAiProvider(Number(e.target.value) as AiProvider)}
                    className="px-2.5 py-1.5 bg-white text-black font-bold text-xs border-2 border-black focus:outline-none shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)] cursor-pointer"
                  >
                    <option value={AiProvider.Unknown}>Default / Not Set</option>
                    <option value={AiProvider.OpenAI}>OpenAI</option>
                    <option value={AiProvider.OpenRouter}>OpenRouter</option>
                    <option value={AiProvider.Gemini}>Gemini</option>
                    <option value={AiProvider.Anthropic}>Anthropic</option>
                  </select>
                </div>

                <div className="flex flex-col gap-1">
                  <label className="font-mono text-[9px] font-black text-black">API KEY</label>
                  <input
                    type="password"
                    placeholder="e.g. sk-..."
                    value={aiApiKey}
                    onChange={(e) => setAiApiKey(e.target.value)}
                    className="px-2.5 py-1.5 bg-white text-black font-bold text-xs border-2 border-black focus:outline-none shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)]"
                  />
                </div>

                <div className="flex flex-col gap-1">
                  <label className="font-mono text-[9px] font-black text-black">MODEL NAME</label>
                  <input
                    type="text"
                    placeholder="e.g. gpt-4o, gemini-2.5-pro..."
                    value={aiModel}
                    onChange={(e) => setAiModel(e.target.value)}
                    className="px-2.5 py-1.5 bg-white text-black font-bold text-xs border-2 border-black focus:outline-none shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)]"
                  />
                </div>

                <button
                  type="submit"
                  disabled={aiSettingsSaving}
                  className="w-full mt-1 py-2 border-2 border-black bg-[#A5B4FC] hover:bg-indigo-400 font-mono text-xs font-black shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)] active:translate-x-[0.5px] active:translate-y-[0.5px] transition-transform text-black cursor-pointer"
                >
                  {aiSettingsSaving ? 'Saving Settings...' : 'Save AI Settings'}
                </button>
              </form>
            )}
          </div>

          {/* Chat Clear Section */}
          <div className="border-2 border-black p-3 bg-white shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)]">
            <span className="font-mono text-[10px] font-bold text-gray-500 block mb-1">Clear Chat History</span>
            <p className="font-mono text-[9px] text-gray-400 mb-3">Clears current message history locally.</p>
            <button
              onClick={handleClearChat}
              className="py-1 px-3 border border-black bg-white hover:bg-gray-100 font-mono text-[10px] font-bold text-black cursor-pointer shadow-[1px_1px_0px_rgba(0,0,0,1)] active:translate-x-[0.5px] active:translate-y-[0.5px]"
            >
              Clear Chat List
            </button>
          </div>

          {/* Danger Zone */}
          {canManage && (
            <div className="border-2 border-red-500 p-3 bg-red-50/50 shadow-[1.5px_1.5px_0px_rgba(239,68,68,1)] mt-auto">
              <span className="font-mono text-[10px] font-bold text-red-700 block mb-1">Danger Zone</span>
              <p className="font-mono text-[9px] text-red-500 mb-3">Irreversible actions on this board.</p>
              
              <button
                onClick={async () => {
                  if (confirm('Are you absolutely sure you want to delete this board? This cannot be undone.')) {
                    try {
                      await api.deleteBoard(boardId);
                      showToast('Board deleted successfully!', 'success');
                      onBack();
                    } catch (err: any) {
                      showToast(err.message || 'Failed to delete board', 'error');
                    }
                  }
                }}
                className="w-full py-1.5 px-3 border-2 border-black bg-red-400 hover:bg-red-300 font-mono text-xs font-black text-black cursor-pointer shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)] active:translate-x-[0.5px] active:translate-y-[0.5px] transition-transform flex items-center justify-center gap-1.5"
              >
                <Trash2 className="w-4 h-4" />
                <span>Delete Board</span>
              </button>
            </div>
          )}
        </div>
      </div>
    );
  };

  // Array of lovely Neobrutalist background colors for columns
  const columnColorPresets = [
    'bg-[#FFDE4D]', // Yellow
    'bg-[#F9A8D4]', // Pink
    'bg-[#A5B4FC]', // Lavender-Indigo
    'bg-[#6EE7B7]', // Mint Green
    'bg-[#C3A6FF]', // Purple
    'bg-[#FFFDF6]', // Light Cream
  ];

  const renderCardPlaceholder = () => (
    <div className="border-2 border-dashed border-black/40 h-16 bg-amber-50/50 flex items-center justify-center pointer-events-none rounded-none transition-all my-1 animate-pulse">
      <span className="font-mono text-[9px] text-black/30 font-bold tracking-wider">DROP HERE</span>
    </div>
  );

  return (
    <div className="h-screen max-h-screen overflow-hidden bg-[#FAF9F6] flex flex-col selection:bg-[#F9A8D4] text-black">
      {/* Top Navigation */}
      <header className="border-b-2 border-black bg-[#A5B4FC] p-4 sticky top-0 z-40">
        <div className="max-w-7xl mx-auto flex flex-col md:flex-row items-stretch md:items-center justify-between gap-4">
          
          {/* Back button and Board Title */}
          <div className="flex items-center gap-3 flex-1">
            <button
              onClick={onBack}
              className="p-2 border-2 border-black bg-white hover:bg-[#FFFCEE] active:translate-x-[1px] active:translate-y-[1px] shadow-[2px_2px_0px_rgba(0,0,0,1)] transition-all font-mono text-xs font-bold flex items-center gap-1 shrink-0"
            >
              <ArrowLeft className="w-4 h-4 stroke-[2.5]" />
              <span>BACK</span>
            </button>

            {/* Editable Title */}
            <div className="flex items-center gap-2 flex-1 min-w-0">
              {isEditingTitle && canManage ? (
                <div className="flex items-center gap-2 w-full max-w-md">
                  <input
                    type="text"
                    value={tempBoardTitle}
                    onChange={(e) => setTempBoardTitle(e.target.value)}
                    className="px-3 py-1.5 bg-white text-black font-black text-xl md:text-2xl border-2 border-black focus:outline-none w-full shadow-[2px_2px_0px_rgba(0,0,0,1)]"
                    onKeyDown={(e) => {
                      if (e.key === 'Enter') handleRenameBoardSubmit();
                      if (e.key === 'Escape') {
                        setIsEditingTitle(false);
                        setTempBoardTitle(board.title || '');
                      }
                    }}
                    autoFocus
                  />
                  <button
                    onClick={handleRenameBoardSubmit}
                    className="p-1.5 border-2 border-black bg-[#6EE7B7] hover:bg-green-400 shadow-[2px_2px_0px_rgba(0,0,0,1)] text-black"
                  >
                    <Check className="w-5 h-5 stroke-[2.5]" />
                  </button>
                  <button
                    onClick={() => {
                      setIsEditingTitle(false);
                      setTempBoardTitle(board.title || '');
                    }}
                    className="p-1.5 border-2 border-black bg-red-400 hover:bg-red-300 shadow-[2px_2px_0px_rgba(0,0,0,1)] text-black"
                  >
                    <X className="w-5 h-5 stroke-[2.5]" />
                  </button>
                </div>
              ) : (
                <div className="flex items-center gap-2 group min-w-0">
                  <h1 className="text-xl md:text-2xl font-black tracking-tight truncate max-w-[200px] sm:max-w-md">
                    {board.title || 'UNTITLED BOARD'}
                  </h1>
                  {canManage && (
                    <button
                      onClick={() => setIsEditingTitle(true)}
                      className="p-1 border border-black bg-white group-hover:bg-[#FFDE4D] shadow-[1px_1px_0px_rgba(0,0,0,1)] text-black transition-colors shrink-0"
                      title="Rename Board"
                    >
                      <Edit3 className="w-3 h-3" />
                    </button>
                  )}
                </div>
              )}
            </div>
          </div>
        </div>
      </header>

      {/* Main Content Layout (Columns + Chat Side-By-Side) */}
      <div className="flex-1 flex flex-row overflow-hidden relative min-h-0 bg-[#FFFBEB]">
        {/* Kanban Layout / Columns Wrapper with Right Side Fade */}
        <div className="flex-1 h-full relative overflow-hidden">
          {/* Kanban Layout / Columns */}
          <div className="w-full h-full p-6 overflow-x-auto flex items-start gap-6 select-none">
        
        {/* Render columns */}
        {board.columns && board.columns.map((column, index) => {
          // Choose a neobrutalist color based on column index
          const colBgColor = columnColorPresets[index % columnColorPresets.length];
          const cards = column.cards || [];
          
          // Inline column title state
          const isRenamingCol = editingColumnId === column.id;

          // Safely determine column title, filtering out any UUIDs that might reside in column.title
          const columnTitle = (() => {
            if (column.Title && !/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(column.Title)) {
              return column.Title;
            }
            if (column.title && !/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(column.title)) {
              return column.title;
            }
            if (column.Title && column.Title !== column.id) return column.Title;
            if (column.title && column.title !== column.id) return column.title;
            return `Column ${column.id.slice(0, 4).toUpperCase()}`;
          })();

          // Drag target styling for columns (only highlighted when we are dragging a column)
          const isOverCol = dragOverColumnId === column.id && draggingColumnId !== null;

          return (
            <div 
              key={column.id}
              draggable
              onDragStart={(e) => handleColumnDragStart(e, column.id)}
              onDragOver={(e) => {
                if (draggingColumnId) {
                  handleColumnDragOver(e, column.id);
                } else if (draggingCardId || draggingCardIdRef.current) {
                  handleColumnCardDragOver(e, column.id);
                }
              }}
              onDragEnd={handleColumnDragEnd}
              onDrop={(e) => {
                if (draggingColumnId) {
                  handleColumnDrop(e);
                } else if (draggingCardId || draggingCardIdRef.current) {
                  handleCardDrop(e, column.id);
                }
              }}
              className={`w-80 shrink-0 border-2 border-black bg-white shadow-[6px_6px_0px_rgba(0,0,0,1)] flex flex-col max-h-full pb-4 transition-all duration-150 ${
                isOverCol ? 'border-dashed border-indigo-600 scale-102 bg-indigo-50/20 shadow-none' : ''
              }`}
            >
              {/* Column Header */}
              <div className={`px-3 py-2.5 border-b-2 border-black bg-slate-100 flex justify-between items-center gap-2 ${canEdit ? 'cursor-grab' : ''}`}>
                {isRenamingCol && canEdit ? (
                  <div className="flex items-center gap-1.5 w-full">
                    <input
                      type="text"
                      value={tempColumnTitle}
                      onChange={(e) => setTempColumnTitle(e.target.value)}
                      className="px-2 py-1 bg-white text-black font-black text-sm border-2 border-black focus:outline-none w-full shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)] font-sans"
                      onKeyDown={(e) => {
                        if (e.key === 'Enter') handleRenameColumnSubmit(column.id);
                        if (e.key === 'Escape') setEditingColumnId(null);
                      }}
                      autoFocus
                    />
                    <button
                      onClick={() => handleRenameColumnSubmit(column.id)}
                      className="p-1 border border-black bg-white hover:bg-green-300 text-black shadow-[1px_1px_0px_rgba(0,0,0,1)] shrink-0"
                    >
                      <Check className="w-3.5 h-3.5" />
                    </button>
                    <button
                      onClick={() => setEditingColumnId(null)}
                      className="p-1 border border-black bg-white hover:bg-red-300 text-black shadow-[1px_1px_0px_rgba(0,0,0,1)] shrink-0"
                    >
                      <X className="w-3.5 h-3.5" />
                    </button>
                  </div>
                ) : (
                  <div className="flex items-center gap-1.5 min-w-0 flex-1 group">
                    <h3 
                      onClick={() => {
                        if (canEdit) {
                          setEditingColumnId(column.id);
                          setTempColumnTitle(columnTitle);
                        }
                      }}
                      className={`text-base font-black tracking-wider truncate ${canEdit ? 'cursor-pointer hover:underline' : ''}`}
                      title={canEdit ? 'Click to rename' : undefined}
                    >
                      {columnTitle}
                    </h3>
                    {canEdit && (
                      <button
                        onClick={() => {
                          setEditingColumnId(column.id);
                          setTempColumnTitle(columnTitle);
                        }}
                        className="p-0.5 border border-black bg-white group-hover:bg-amber-300 opacity-60 group-hover:opacity-100 shadow-[1px_1px_0px_rgba(0,0,0,1)] text-black shrink-0"
                      >
                        <Edit3 className="w-2.5 h-2.5" />
                      </button>
                    )}
                    <Badge color="gray" className="text-[10px] py-0 px-1 border-0 shadow-none bg-black/10 font-bold ml-1">
                      {cards.length}
                    </Badge>
                  </div>
                )}

                {/* Delete Column trigger */}
                {canEdit && (
                  <button
                    onClick={() => handleRequestDeleteColumn(column.id, columnTitle)}
                    className="p-1 border border-black bg-white/50 hover:bg-red-400 active:translate-x-[1px] active:translate-y-[1px] shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)] shrink-0 text-black transition-colors"
                    title="Delete Column"
                  >
                    <Trash2 className="w-3.5 h-3.5 stroke-[2]" />
                  </button>
                )}
              </div>

              {/* Cards Container */}
              <div 
                onDragOver={(e) => {
                  e.preventDefault();
                  if (draggingCardId || draggingCardIdRef.current) {
                    handleColumnCardDragOver(e, column.id);
                  }
                }}
                onDrop={(e) => {
                  e.preventDefault();
                  if (draggingCardId || draggingCardIdRef.current) {
                    handleCardDrop(e, column.id);
                  }
                }}
                className="flex-1 overflow-y-auto p-4 flex flex-col gap-3 min-h-[100px] bg-[#FFFDF6]"
              >
                {cards.length === 0 ? (
                  <div className="py-8 text-center border border-dashed border-black/30 bg-black/[0.01] flex flex-col items-center justify-center min-h-[120px] transition-all">
                    {dragOverColumnId === column.id && (draggingCardId || draggingCardIdRef.current) ? (
                      <div className="w-full p-2">
                        {renderCardPlaceholder()}
                      </div>
                    ) : (
                      <span className="font-mono text-[10px] text-gray-500 font-bold">
                        EMPTY COLUMN
                      </span>
                    )}
                  </div>
                ) : (
                  <>
                    {cards.map((card, cardIdx) => {
                      const isOverCard = dragOverCardId === card.id && dragOverColumnId === column.id;
                      const isDraggingThisCard = draggingCardId === card.id;
                      const isLastCard = cardIdx === cards.length - 1;
                      const showBottomLine = isLastCard && dragOverCardId === null && dragOverColumnId === column.id && dragOverPosition === 'bottom';
                      
                      const totalChecklistItems = card.checklists?.reduce((sum, cl) => sum + (cl.items?.length || 0), 0) || 0;
                      const completedChecklistItems = card.checklists?.reduce((sum, cl) => sum + (cl.items?.filter(item => item.isCompleted).length || 0), 0) || 0;
                      const percent = totalChecklistItems > 0 ? Math.round((completedChecklistItems / totalChecklistItems) * 100) : 0;

                      const colorMode = cardColorModes[card.id] || 'accent';
                      const isFullyColor = colorMode === 'full';
                      const cardBgStyle = isFullyColor && card.coverColor && !isDraggingThisCard 
                        ? { backgroundColor: card.coverColor } 
                        : undefined;

                      return (
                        <div key={card.id} className="relative">
                          {/* Absolute, non-layout-shifting drop indicator line */}
                          {isOverCard && (draggingCardId || draggingCardIdRef.current) && (
                            <div 
                              className={`absolute left-0 right-0 h-1.5 bg-[#FFDE4D] border-y-2 border-black z-30 pointer-events-none ${
                                dragOverPosition === 'before' ? '-top-[9px]' : '-bottom-[9px]'
                              }`}
                            >
                              <div className="absolute left-1/2 -translate-x-1/2 -top-[14px] bg-[#FFDE4D] border-2 border-black px-2 py-0.5 text-[8px] font-mono font-bold tracking-wider shadow-[1px_1px_0px_rgba(0,0,0,1)] text-black">
                                DROP HERE
                              </div>
                            </div>
                          )}

                          {showBottomLine && (draggingCardId || draggingCardIdRef.current) && (
                            <div 
                              className="absolute left-0 right-0 h-1.5 bg-[#FFDE4D] border-y-2 border-black z-30 pointer-events-none -bottom-[9px]"
                            >
                              <div className="absolute left-1/2 -translate-x-1/2 -top-[14px] bg-[#FFDE4D] border-2 border-black px-2 py-0.5 text-[8px] font-mono font-bold tracking-wider shadow-[1px_1px_0px_rgba(0,0,0,1)] text-black">
                                DROP HERE
                              </div>
                            </div>
                          )}

                          <div 
                            draggable={canEdit}
                            onDragStart={(e) => handleCardDragStart(e, card.id, column.id)}
                            onDragEnd={handleCardDragEnd}
                            onDragOver={(e) => handleCardDragOver(e, card.id, column.id)}
                            onDrop={(e) => handleCardDrop(e, column.id)}
                            onClick={() => handleOpenCardDetail(card)}
                            style={cardBgStyle}
                            className={`border-2 border-black shadow-[3px_3px_0px_rgba(0,0,0,1)] hover:shadow-[5px_5px_0px_rgba(0,0,0,1)] hover:-translate-x-[1px] hover:-translate-y-[1px] transition-all group/card flex flex-col select-none overflow-hidden ${
                              canEdit ? 'cursor-grab' : 'cursor-pointer'
                            } ${
                              isDraggingThisCard ? 'opacity-40 border-dashed bg-gray-100 shadow-none scale-95' : (isFullyColor && card.coverColor ? '' : 'bg-white')
                            }`}
                          >
                            {card.coverColor && !isFullyColor && (
                              <div 
                                className="w-full h-3 border-b-2 border-black shrink-0 animate-fade-in" 
                                style={{ backgroundColor: card.coverColor }}
                              />
                            )}
                            
                            <div className="p-3 flex justify-between items-start gap-2 w-full flex-1">
                              <div className="flex flex-col gap-1 min-w-0 flex-1">
                                {card.labels && card.labels.length > 0 && (
                                  <div className="flex flex-wrap gap-1 mb-1.5">
                                    {card.labels.map(lbl => (
                                      <span 
                                        key={lbl.id} 
                                        className="text-[9px] font-mono font-black border border-black shadow-[1px_1px_0px_rgba(0,0,0,1)] px-1.5 py-0.5 uppercase tracking-wide inline-block text-black select-none"
                                        style={{ backgroundColor: lbl.color || '#E2E8F0' }}
                                      >
                                        {lbl.name}
                                      </span>
                                    ))}
                                  </div>
                                )}
                                <span className="font-sans font-bold text-sm text-black leading-tight break-words">
                                  {card.title || 'UNTITLED CARD'}
                                </span>
                                {card.description && (
                                  <div className="text-gray-500 mt-1" title="This card has a description">
                                    <AlignLeft className="w-3.5 h-3.5 stroke-[2.5]" />
                                  </div>
                                )}

                                {card.dueDate && (
                                  <div className={`mt-2 flex items-center gap-1 font-mono text-[9px] font-black border-2 border-black px-1.5 py-0.5 shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)] w-fit text-black select-none ${
                                    new Date(card.dueDate) < new Date() ? 'bg-[#FF8FAB] animate-pulse' : 'bg-[#FFDE4D]'
                                  }`}>
                                    <Calendar className="w-3 h-3 shrink-0" />
                                    <span>
                                      {new Date(card.dueDate).toLocaleDateString()}
                                    </span>
                                    {card.reminderDate && (
                                      <span className="text-[10px]" title={`Reminder set: ${new Date(card.reminderDate).toLocaleString()}`}>
                                        🔔
                                      </span>
                                    )}
                                  </div>
                                )}

                                {totalChecklistItems > 0 && (
                                  <div className="mt-2.5 flex flex-col gap-1 w-full border-t border-black/10 pt-2">
                                    <div className="flex justify-between items-center font-mono text-[8px] text-gray-500 font-bold tracking-wider">
                                      <span>Checklist</span>
                                      <span>{completedChecklistItems}/{totalChecklistItems}</span>
                                    </div>
                                    <div className="w-full h-1.5 border border-black bg-white shadow-[0.5px_0.5px_0px_rgba(0,0,0,1)] overflow-hidden">
                                      <div 
                                        className="h-full bg-emerald-400 transition-all duration-300" 
                                        style={{ width: `${percent}%` }}
                                      />
                                    </div>
                                  </div>
                                )}
                              </div>

                              {/* Card actions */}
                              {canEdit && (
                                <div className="flex items-center gap-1 shrink-0 opacity-20 group-hover/card:opacity-100 transition-opacity">
                                  <button
                                    onClick={(e) => {
                                      e.stopPropagation();
                                      handleDeleteCard(card.id);
                                    }}
                                    className="p-1 hover:text-red-500 border border-transparent hover:border-black hover:bg-red-50 text-black active:translate-x-[0.5px] active:translate-y-[0.5px] transition-all"
                                    title="Delete Card"
                                  >
                                    <Trash2 className="w-3.5 h-3.5 stroke-[2]" />
                                  </button>
                                </div>
                              )}
                            </div>
                          </div>
                        </div>
                      );
                    })}
                  </>
                )}
              </div>

              {/* Add Card Input Area */}
              {canEdit && (
                <div className="p-3 border-t-2 border-black bg-white">
                  {activeCardInputColId === column.id ? (
                    <div className="flex flex-col gap-2">
                      <input
                        type="text"
                        placeholder="Enter Card Title..."
                        value={newCardTitles[column.id] || ''}
                        onChange={(e) => setNewCardTitles(prev => ({ ...prev, [column.id]: e.target.value }))}
                        disabled={cardLoadingStates[column.id]}
                        autoFocus
                        className="w-full px-2.5 py-1.5 bg-[#FFFDF6] text-black font-semibold text-xs border-2 border-black shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)] focus:outline-none focus:bg-[#FFFCEE]"
                        onKeyDown={(e) => {
                          if (e.key === 'Enter') {
                            handleCreateCard(column.id);
                          } else if (e.key === 'Escape') {
                            setActiveCardInputColId(null);
                          }
                        }}
                      />
                      <div className="flex gap-2">
                        <Button
                          variant="success"
                          size="sm"
                          onClick={() => handleCreateCard(column.id)}
                          disabled={cardLoadingStates[column.id] || !newCardTitles[column.id]?.trim()}
                          className="flex-1 py-1 font-mono text-[10px] font-extrabold shadow-[2px_2px_0px_rgba(0,0,0,1)]"
                        >
                          {cardLoadingStates[column.id] ? 'ADDING...' : 'SAVE'}
                        </Button>
                        <Button
                          variant="info"
                          size="sm"
                          onClick={() => {
                            setActiveCardInputColId(null);
                            setNewCardTitles(prev => ({ ...prev, [column.id]: '' }));
                          }}
                          disabled={cardLoadingStates[column.id]}
                          className="py-1 font-mono text-[10px] font-extrabold border-2 border-black shadow-[2px_2px_0px_rgba(0,0,0,1)]"
                        >
                          CANCEL
                        </Button>
                      </div>
                    </div>
                  ) : (
                    <button
                      onClick={() => {
                        setActiveCardInputColId(column.id);
                      }}
                      className="w-full py-1 px-2 border-2 border-black bg-white hover:bg-gray-100 font-mono text-[9px] font-bold tracking-wide transition-all shadow-[1px_1px_0px_rgba(0,0,0,1)] active:translate-x-[0.5px] active:translate-y-[0.5px] flex items-center justify-center gap-1 cursor-pointer text-black"
                    >
                      <Plus className="w-2.5 h-2.5 stroke-[3]" />
                      <span>ADD CARD</span>
                    </button>
                  )}
                </div>
              )}

            </div>
          );
        })}

        {/* Create Column Column */}
        {canEdit && (
          <div className="w-80 shrink-0 border-2 border-dashed border-black/40 hover:border-black bg-black/[0.02] hover:bg-black/[0.04] p-5 shadow-[2px_2px_0px_rgba(0,0,0,1)] hover:shadow-[4px_4px_0px_rgba(0,0,0,1)] transition-all">
            <form onSubmit={handleCreateColumn} className="flex flex-col gap-3">
              <h3 className="font-black text-sm tracking-wider flex items-center gap-1 text-black/70">
                <Plus className="w-4 h-4 stroke-[3]" />
                <span>CREATE COLUMN</span>
              </h3>
              
              <input
                type="text"
                placeholder="e.g. IN PROGRESS, DONE"
                value={newColumnTitle}
                onChange={(e) => setNewColumnTitle(e.target.value)}
                disabled={columnLoading}
                className="w-full px-3 py-2 bg-white text-black font-bold text-xs border-2 border-black shadow-[2px_2px_0px_rgba(0,0,0,1)] focus:outline-none"
              />
              
              <Button
                type="submit"
                variant="success"
                size="sm"
                disabled={columnLoading || !newColumnTitle.trim()}
                className="w-full font-mono text-xs font-bold shadow-[3px_3px_0px_rgba(0,0,0,1)] active:shadow-[1px_1px_0px_rgba(0,0,0,1)]"
              >
                {columnLoading ? 'CREATING...' : 'ADD NEW COLUMN'}
              </Button>
            </form>
          </div>
        )}

          </div>
          {/* Right Side Fade Overlay to let columns fade gracefully into the background behind side panel tabs */}
          <div className="absolute right-0 top-0 bottom-0 w-64 bg-gradient-to-r from-transparent via-[#FFFBEB]/30 to-[#FFFBEB] pointer-events-none z-20" />
        </div>

      {/* Collapsible Bookmark Side Panel */}
      <div className={`transition-all duration-300 flex select-none shrink-0 h-full bg-white z-40 ${
        isRightPanelOpen 
          ? 'absolute lg:relative right-0 top-0 bottom-0 w-[calc(100vw-70px)] sm:w-96 border-l-2 border-black shadow-[-4px_0_12px_rgba(0,0,0,0.15)] pointer-events-auto' 
          : 'absolute lg:relative right-0 top-0 bottom-0 w-0 border-l-0 shadow-none pointer-events-none'
      }`}>
        {/* Bookmark tabs sticking out vertically to the left (aligned to right edge of bounding box) */}
        <div className="absolute top-12 right-full z-30 flex flex-col items-end gap-2 pointer-events-auto">
          {/* Settings Bookmark Tab */}
          <button
            onClick={() => {
              if (isRightPanelOpen && rightPanelTab === 'settings') {
                setIsRightPanelOpen(false);
              } else {
                setIsRightPanelOpen(true);
                setRightPanelTab('settings');
              }
            }}
            className={`h-12 border-2 border-black flex items-center justify-center transition-all shadow-[-2.5px_2.5px_0px_rgba(0,0,0,1)] active:translate-x-[1px] active:translate-y-[1px] cursor-pointer rounded-none ${
              isRightPanelOpen && rightPanelTab === 'settings'
                ? 'bg-[#A5B4FC] text-black border-r-transparent translate-x-[2px] px-4 gap-2'
                : 'bg-white hover:bg-gray-100 text-black w-12'
            }`}
            title="Board Settings"
          >
            <Settings className="w-5 h-5 stroke-[2.5]" />
            {isRightPanelOpen && rightPanelTab === 'settings' && (
              <span className="font-mono text-xs font-black tracking-wider text-black">
                Settings
              </span>
            )}
          </button>

          {/* Members Bookmark Tab */}
          <button
            onClick={() => {
              if (isRightPanelOpen && rightPanelTab === 'members') {
                setIsRightPanelOpen(false);
              } else {
                setIsRightPanelOpen(true);
                setRightPanelTab('members');
              }
            }}
            className={`h-12 border-2 border-black flex items-center justify-center transition-all shadow-[-2.5px_2.5px_0px_rgba(0,0,0,1)] active:translate-x-[1px] active:translate-y-[1px] cursor-pointer rounded-none ${
              isRightPanelOpen && rightPanelTab === 'members'
                ? 'bg-[#FFDE4D] text-black border-r-transparent translate-x-[2px] px-4 gap-2'
                : 'bg-white hover:bg-gray-100 text-black w-12'
            }`}
            title="Participants & Members"
          >
            <MoreHorizontal className="w-5 h-5 stroke-[2.5]" />
            {isRightPanelOpen && rightPanelTab === 'members' && (
              <span className="font-mono text-xs font-black tracking-wider text-black">
                Members
              </span>
            )}
          </button>

          {/* Chat Bookmark Tab (Wider, Horizontal text with Sparkles, matches hand-drawn mockup perfectly!) */}
          <button
            onClick={() => {
              if (isRightPanelOpen && rightPanelTab === 'chat') {
                setIsRightPanelOpen(false);
              } else {
                setIsRightPanelOpen(true);
                setRightPanelTab('chat');
              }
            }}
            className={`h-12 border-2 border-black flex items-center justify-center transition-all shadow-[-2.5px_2.5px_0px_rgba(0,0,0,1)] active:translate-x-[1px] active:translate-y-[1px] cursor-pointer rounded-none ${
              isRightPanelOpen && rightPanelTab === 'chat'
                ? 'bg-[#FF8FAB] text-black border-r-transparent translate-x-[2px] px-4 gap-2'
                : 'bg-white hover:bg-gray-100 text-black w-12'
            }`}
            title="Board Chat Assistant"
          >
            <Sparkles className="w-4 h-4 text-black stroke-[2.5]" />
            {isRightPanelOpen && rightPanelTab === 'chat' && (
              <span className="font-mono text-xs font-black tracking-wider text-black">
                Chat
              </span>
            )}
          </button>
        </div>

        {/* Panel Content Area */}
        {isRightPanelOpen && (
          <div className="w-full h-full flex flex-col overflow-hidden bg-white">
            {rightPanelTab === 'chat' && renderChatTab()}
            {rightPanelTab === 'members' && renderMembersTab()}
            {rightPanelTab === 'settings' && renderSettingsTab()}
          </div>
        )}
      </div>

      </div>

      {/* MEMBER COLLABORATION MODAL */}
      <Modal
        isOpen={isMemberModalOpen}
        onClose={() => setIsMemberModalOpen(false)}
        title={
          <>
            <Users className="w-5 h-5 text-indigo-600" />
            <span>MEMBERS & ACCESS LEVELS</span>
          </>
        }
      >
        <div className="flex flex-col gap-4">
          
          {/* Current Members list */}
          <div className="flex flex-col gap-2">
            <h4 className="font-mono text-xs font-bold text-black tracking-wide">
              Current Members
            </h4>
            <div className="border-2 border-black bg-[#FFFDF6] max-h-[180px] overflow-y-auto shadow-[3px_3px_0px_rgba(0,0,0,1)] divide-y-2 divide-black">
              {allParticipants && allParticipants.length > 0 ? (
                allParticipants.map((participant) => (
                  <div key={participant.userId} className="p-2.5 flex justify-between items-center hover:bg-[#FFFCEE] transition-colors gap-2">
                    <div className="flex flex-col">
                      <span className="font-sans font-bold text-sm flex items-center gap-1.5">
                        {participant.username}
                        {participant.isOwner && (
                          <span className="flex items-center gap-1 bg-[#FFDE4D] text-black border border-black text-[9px] px-1 py-0.5 font-bold rounded-none">
                            <Crown className="w-2.5 h-2.5 text-black" />
                            <span>OWNER</span>
                          </span>
                        )}
                      </span>
                      <span className="font-mono text-[9px] text-gray-500">
                        Role: {getAccessLevelLabel(participant.accessLevel)}
                      </span>
                    </div>

                    <div className="flex items-center gap-2">
                      {canManage && !participant.isOwner ? (
                        <>
                          <select
                            value={participant.accessLevel}
                            onChange={(e) => handleUpdateAccessLevel(participant.userId, parseInt(e.target.value))}
                            className="bg-[#FFFCEE] border-2 border-black text-xs font-bold px-1.5 py-1 focus:outline-none cursor-pointer"
                          >
                            <option value={0}>Viewer</option>
                            <option value={1}>Member</option>
                            <option value={2}>Admin</option>
                          </select>

                          <button
                            onClick={() => {
                              const userToRem: UserDto = { id: participant.userId, username: participant.username || 'Collaborator' };
                              handleRequestRemoveMember(userToRem);
                            }}
                            className="p-1 border-2 border-black bg-red-100 hover:bg-red-300 text-black shadow-[1px_1px_0px_rgba(0,0,0,1)] transition-transform active:translate-x-[0.5px] active:translate-y-[0.5px]"
                            title="Remove member"
                          >
                            <X className="w-3.5 h-3.5" />
                          </button>
                        </>
                      ) : (
                        <Badge color={participant.isOwner ? 'yellow' : participant.accessLevel === 2 ? 'yellow' : participant.accessLevel === 1 ? 'pink' : 'gray'}>
                          {participant.isOwner ? 'OWNER' : getAccessLevelLabel(participant.accessLevel).toUpperCase()}
                        </Badge>
                      )}
                    </div>
                  </div>
                ))
              ) : (
                <div className="p-4 text-center font-mono text-[10px] text-gray-400">
                  No participants
                </div>
              )}
            </div>
          </div>

          {canManage ? (
            <div className="flex flex-col gap-3 border-t-2 border-black pt-3">
              <p className="font-mono text-xs text-gray-700 leading-relaxed">
                Search for an existing user inside Synkan.API by their username to invite them to this board.
              </p>

              <div className="relative">
                <Input
                  label="Search User"
                  type="text"
                  placeholder="Type username..."
                  value={userSearchQuery}
                  onChange={(e) => setUserSearchQuery(e.target.value)}
                  autoFocus
                />
                <Search className="absolute right-3.5 bottom-3 w-5 h-5 text-black stroke-[2.5]" />
              </div>

              <div className="border-2 border-black bg-white max-h-[160px] overflow-y-auto shadow-[3px_3px_0px_rgba(0,0,0,1)]">
                {searchLoading ? (
                  <div className="p-4 text-center font-mono text-xs font-bold animate-pulse">
                    SEARCHING USER DATABASE...
                  </div>
                ) : userSearchQuery && searchResults.length === 0 ? (
                  <div className="p-4 text-center font-mono text-xs text-gray-500">
                    NO REGISTERED USER DISCOVERED
                  </div>
                ) : !userSearchQuery ? (
                  <div className="p-4 text-center font-mono text-[10px] text-gray-400">
                    Type something to begin searching
                  </div>
                ) : (
                  <div className="divide-y-2 divide-black">
                    {searchResults.map((user) => (
                      <div key={user.id} className="p-3 flex justify-between items-center hover:bg-[#FFFCEE] transition-colors">
                        <span className="font-sans font-bold text-sm flex items-center gap-2">
                          <div className="w-2.5 h-2.5 bg-[#FF8FAB] border border-black inline-block rounded-full" />
                          {user.username}
                        </span>
                        
                        <Button
                          variant="primary"
                          size="sm"
                          onClick={() => handleAddBoardMember(user.id, user.username || '')}
                          disabled={addMemberLoadingId !== null}
                          className="py-1 px-3 shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)] text-[10px] font-mono"
                        >
                          {addMemberLoadingId === user.id ? 'ADDING...' : 'ADD TO BOARD'}
                        </Button>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>
          ) : (
            <div className="border-t-2 border-black pt-3">
              <p className="font-mono text-[10px] text-gray-500 leading-relaxed text-center flex items-center justify-center gap-1.5">
                <Lock className="w-3.5 h-3.5" />
                <span>You must be an Admin to invite new members or change access roles.</span>
              </p>
            </div>
          )}

          <div className="flex justify-end gap-3 mt-2 border-t-2 border-black pt-3">
            <Button
              type="button"
              variant="info"
              onClick={() => setIsMemberModalOpen(false)}
            >
              CLOSE WINDOW
            </Button>
          </div>
        </div>
      </Modal>

      {/* REMOVE PARTICIPANT CONFIRMATION MODAL */}
      <Modal
        isOpen={isRemoveMemberOpen}
        onClose={() => {
          setIsRemoveMemberOpen(false);
          setMemberToRemove(null);
        }}
        title={
          <>
            <UserX className="w-5 h-5 text-indigo-600" />
            <span>REMOVE MEMBER?</span>
          </>
        }
      >
        <div className="flex flex-col gap-4">
          <div className="flex gap-3 items-start border-2 border-black bg-red-50 p-4 shadow-[3px_3px_0px_rgba(0,0,0,1)]">
            <UserX className="w-8 h-8 text-[#FF6B6B] shrink-0 stroke-[2.5]" />
            <div>
              <h4 className="font-black text-sm">Remove Participant "{memberToRemove?.username}"?</h4>
              <p className="font-mono text-[11px] text-gray-700 mt-1 leading-relaxed">
                Are you sure you want to remove this participant? They will immediately lose access to this board and all its tasks.
              </p>
            </div>
          </div>
          
          <div className="flex justify-end gap-3 mt-2">
            <Button
              type="button"
              variant="info"
              onClick={() => {
                setIsRemoveMemberOpen(false);
                setMemberToRemove(null);
              }}
              disabled={removeMemberLoading}
            >
              CANCEL
            </Button>
            <Button
              type="button"
              variant="danger"
              onClick={handleRemoveMemberConfirm}
              disabled={removeMemberLoading}
              className="flex items-center gap-1.5"
            >
              <UserX className="w-4 h-4" />
              <span>{removeMemberLoading ? 'REMOVING...' : 'YES, REMOVE PARTICIPANT'}</span>
            </Button>
          </div>
        </div>
      </Modal>

      {/* DELETE COLUMN CONFIRMATION MODAL */}
      <Modal
        isOpen={isDeleteColOpen}
        onClose={() => {
          setIsDeleteColOpen(false);
          setColToDelete(null);
        }}
        title={
          <>
            <Trash2 className="w-5 h-5 text-[#FF6B6B]" />
            <span>DELETE COLUMN</span>
          </>
        }
      >
        <div className="flex flex-col gap-4">
          <div className="flex gap-3 items-start border-2 border-black bg-red-50 p-4 shadow-[3px_3px_0px_rgba(0,0,0,1)]">
            <ShieldAlert className="w-8 h-8 text-[#FF6B6B] shrink-0 stroke-[2.5]" />
            <div>
              <h4 className="font-black text-sm">Delete Column "{colToDelete?.title}"?</h4>
              <p className="font-mono text-[11px] text-gray-700 mt-1 leading-relaxed">
                This action is <strong>permanent</strong> and cannot be undone! This will delete the column along with all cards inside it.
              </p>
            </div>
          </div>
          
          <div className="flex justify-end gap-3 mt-2">
            <Button
              type="button"
              variant="info"
              onClick={() => {
                setIsDeleteColOpen(false);
                setColToDelete(null);
              }}
              disabled={deleteColLoading}
            >
              CANCEL
            </Button>
            <Button
              type="button"
              variant="danger"
              onClick={handleDeleteColumnConfirm}
              disabled={deleteColLoading}
              className="flex items-center gap-1.5"
            >
              <Trash2 className="w-4 h-4" />
              <span>{deleteColLoading ? 'DELETING...' : 'YES, DELETE COLUMN'}</span>
            </Button>
          </div>
        </div>
      </Modal>

      {/* CARD DETAIL & EDIT DESCRIPTION MODAL */}
      {/* CARD DETAILS MODAL */}
      <Modal
        isOpen={isCardDetailOpen}
        onClose={() => {
          setIsCardDetailOpen(false);
          setActiveCard(null);
        }}
        title={
          <>
            <FileText className="w-5 h-5 text-indigo-600" />
            <span>CARD DETAILS</span>
          </>
        }
        headerAction={
          activeCard && canEdit && (
            <div className="flex items-center gap-1.5 shrink-0 flex-wrap sm:flex-nowrap">
              <button
                type="button"
                onClick={handleOpenDueDateModal}
                className="px-2 py-1 text-[10px] font-mono font-bold border-2 border-black bg-[#FFDE4D] hover:bg-yellow-300 text-black shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)] active:translate-x-[0.5px] active:translate-y-[0.5px] flex items-center gap-1 transition-colors shrink-0"
                title="Set Due Date & Reminder"
              >
                <Calendar className="w-3.5 h-3.5" />
                <span>DEADLINE</span>
              </button>
              <button
                type="button"
                onClick={() => setIsLabelsModalOpen(true)}
                className="px-2 py-1 text-[10px] font-mono font-bold border-2 border-black bg-[#6EE7B7] hover:bg-emerald-300 text-black shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)] active:translate-x-[0.5px] active:translate-y-[0.5px] flex items-center gap-1 transition-colors shrink-0"
                title="Assign Labels"
              >
                <Tag className="w-3.5 h-3.5" />
                <span>LABELS</span>
              </button>
              <button
                type="button"
                onClick={() => setIsColorModalOpen(true)}
                className="px-2 py-1 text-[10px] font-mono font-bold border-2 border-black bg-[#F9A8D4] hover:bg-pink-300 text-black shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)] active:translate-x-[0.5px] active:translate-y-[0.5px] flex items-center gap-1 transition-colors shrink-0"
                title="Set Card Color & Style"
              >
                <Palette className="w-3.5 h-3.5" />
                <span>COLOR</span>
              </button>
            </div>
          )
        }
      >
        <form onSubmit={handleSaveCardDetail} className="flex flex-col gap-4">
          <div className="flex flex-col gap-4">
            <Input
              label="Card Title"
              type="text"
              placeholder="Card Title"
              value={editCardTitle}
              onChange={(e) => setEditCardTitle(e.target.value)}
              disabled={cardUpdateLoading || !canEdit}
              autoFocus
            />

            <div className="flex flex-col gap-1 w-full">
              <label className="font-mono text-sm font-bold text-black tracking-wide">
                Card Description
              </label>
              <textarea
                value={editCardDescription}
                onChange={(e) => setEditCardDescription(e.target.value)}
                placeholder={canEdit ? "Enter a detailed description for this task..." : "No description provided."}
                disabled={cardUpdateLoading || !canEdit}
                rows={5}
                className="w-full px-4 py-3 bg-white text-black font-mono text-sm border-2 border-black shadow-[3px_3px_0px_rgba(0,0,0,1)] focus:outline-none focus:bg-[#FFFCEE] focus:shadow-[5px_5px_0px_rgba(0,0,0,1)] transition-all resize-none"
              />
            </div>
            
            {/* Active Card Labels & Due Date Display inside Card Details */}
            {activeCard && ((activeCard.labels && activeCard.labels.length > 0) || activeCard.dueDate) && (
              <div className="flex flex-wrap gap-4 border-t-2 border-dashed border-black pt-4">
                {activeCard.labels && activeCard.labels.length > 0 && (
                  <div className="flex flex-col gap-1.5 flex-1 min-w-[200px]">
                    <span className="font-sans font-black text-xs text-black uppercase tracking-wider flex items-center gap-1 select-none">
                      <Tag className="w-3.5 h-3.5 text-emerald-600" />
                      <span>Active Labels</span>
                    </span>
                    <div className="flex flex-wrap gap-1.5 mt-1">
                      {activeCard.labels.map(lbl => (
                        <div 
                          key={lbl.id} 
                          className="flex items-center gap-1 border border-black shadow-[1px_1px_0px_rgba(0,0,0,1)] px-2 py-1 text-[11px] font-mono font-black text-black select-none"
                          style={{ backgroundColor: lbl.color || '#E2E8F0' }}
                        >
                          <span>{lbl.name}</span>
                          {canEdit && (
                            <button
                              type="button"
                              onClick={() => handleToggleCardLabel(lbl.id, true)}
                              className="ml-1 bg-black/10 hover:bg-black/20 rounded-none w-4 h-4 flex items-center justify-center font-bold text-[10px] transition-colors"
                              title="Remove Label"
                            >
                              ✕
                            </button>
                          )}
                        </div>
                      ))}
                    </div>
                  </div>
                )}

                {activeCard.dueDate && (
                  <div className="flex flex-col gap-1.5 flex-1 min-w-[200px]">
                    <span className="font-sans font-black text-xs text-black uppercase tracking-wider flex items-center gap-1 select-none">
                      <Calendar className="w-3.5 h-3.5 text-yellow-600" />
                      <span>Task Deadline</span>
                    </span>
                    <div className={`mt-1 border-2 border-black p-2 shadow-[2px_2px_0px_rgba(0,0,0,1)] flex items-center justify-between gap-4 font-mono text-xs font-bold text-black ${
                      new Date(activeCard.dueDate) < new Date() ? 'bg-rose-50 border-rose-600' : 'bg-amber-50'
                    }`}>
                      <div className="flex flex-col gap-0.5">
                        <span className="font-black flex items-center gap-1">
                          <Clock className="w-3.5 h-3.5 text-black" />
                          Due: {new Date(activeCard.dueDate).toLocaleDateString()} {new Date(activeCard.dueDate).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                        </span>
                        {activeCard.reminderDate && (
                          <span className="text-[10px] text-gray-500 font-bold flex items-center gap-1">
                            🔔 Reminder: {new Date(activeCard.reminderDate).toLocaleDateString()} {new Date(activeCard.reminderDate).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                          </span>
                        )}
                      </div>
                      {canEdit && (
                        <button
                          type="button"
                          onClick={handleClearDueDate}
                          className="text-red-500 hover:text-red-700 text-[10px] font-black underline"
                        >
                          CLEAR
                        </button>
                      )}
                    </div>
                  </div>
                )}
              </div>
            )}

            {/* Checklists Section */}
            <div className="flex flex-col gap-4 border-t-2 border-dashed border-black pt-4">
              <div className="flex justify-between items-center gap-2">
                <label className="font-sans font-black text-sm text-black tracking-wide flex items-center gap-2">
                  <CheckSquare className="w-4 h-4 text-emerald-600" />
                  <span>Checklists</span>
                </label>
                
                {canEdit && (
                  <div className="flex items-center gap-2">
                    <input
                      type="text"
                      placeholder="New Checklist Title..."
                      value={newChecklistTitle}
                      onChange={(e) => setNewChecklistTitle(e.target.value)}
                      className="px-2 py-1 bg-white text-black font-mono text-xs border-2 border-black focus:outline-none focus:bg-[#FFFCEE] shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)] w-48"
                      onKeyDown={(e) => {
                        if (e.key === 'Enter') {
                          e.preventDefault();
                          handleCreateChecklist();
                        }
                      }}
                    />
                    <Button
                      type="button"
                      variant="success"
                      onClick={handleCreateChecklist}
                      className="py-1 px-3 text-[10px] font-mono shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)]"
                    >
                      ADD
                    </Button>
                  </div>
                )}
              </div>

              {/* Render lists */}
              <div className="flex flex-col gap-4">
                {activeCard?.checklists && activeCard.checklists.length > 0 ? (
                  activeCard.checklists.map((checklist) => {
                    const totalItems = checklist.items?.length || 0;
                    const completedItems = checklist.items?.filter(item => item.isCompleted).length || 0;
                    const checklistPercent = totalItems > 0 ? Math.round((completedItems / totalItems) * 100) : 0;

                    return (
                      <div key={checklist.id} className="border-2 border-black p-3 bg-slate-50/50 shadow-[3px_3px_0px_rgba(0,0,0,1)] flex flex-col gap-2">
                        <div className="flex justify-between items-center gap-2 border-b border-black/10 pb-1.5">
                          <h5 className="font-sans font-extrabold text-xs text-black tracking-wider flex items-center gap-1.5">
                            <ListTodo className="w-3.5 h-3.5 text-black" />
                            {checklist.title}
                          </h5>
                          {canEdit && (
                            <button
                              type="button"
                              onClick={() => handleDeleteChecklist(checklist.id)}
                              className="text-red-500 hover:text-red-700 p-0.5 border border-transparent hover:border-black hover:bg-red-50 transition-all font-mono text-[9px] font-bold"
                              title="Delete Checklist"
                            >
                              DELETE LIST
                            </button>
                          )}
                        </div>

                        {/* Progress Bar */}
                        <div className="flex flex-col gap-1">
                          <div className="flex justify-between items-center font-mono text-[9px] text-gray-500 font-bold">
                            <span>Progress</span>
                            <span>{completedItems}/{totalItems} ({checklistPercent}%)</span>
                          </div>
                          <div className="w-full h-2.5 border-2 border-black bg-white shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)] overflow-hidden relative">
                            <div 
                              className="h-full bg-emerald-400 transition-all duration-300" 
                              style={{ width: `${checklistPercent}%` }}
                            />
                          </div>
                        </div>

                        {/* Checklist items list */}
                        <div className="flex flex-col gap-1.5 mt-1.5">
                          {checklist.items && checklist.items.length > 0 ? (
                            checklist.items.map((item) => (
                              <div key={item.id} className="flex justify-between items-center gap-2 group/item hover:bg-black/[0.02] p-1 rounded-sm">
                                <label className="flex items-center gap-2.5 cursor-pointer select-none min-w-0 flex-1">
                                  <input
                                    type="checkbox"
                                    checked={item.isCompleted}
                                    onChange={(e) => handleToggleChecklistItem(checklist.id, item.id, e.target.checked)}
                                    disabled={!canEdit}
                                    className="w-4 h-4 rounded-none border-2 border-black text-emerald-600 focus:ring-0 focus:ring-offset-0 cursor-pointer accent-emerald-500 shrink-0"
                                  />
                                  <span className={`font-mono text-xs text-black tracking-tight truncate leading-none ${
                                    item.isCompleted ? 'line-through text-gray-400 font-medium' : 'font-bold'
                                  }`}>
                                    {item.text}
                                  </span>
                                </label>
                                {canEdit && (
                                  <button
                                    type="button"
                                    onClick={() => handleDeleteChecklistItem(checklist.id, item.id)}
                                    className="p-1 hover:text-red-500 border border-transparent hover:border-black hover:bg-red-50 text-black active:translate-x-[0.5px] active:translate-y-[0.5px] transition-all shrink-0 opacity-0 group-hover/item:opacity-100"
                                    title="Delete Item"
                                  >
                                    <Trash2 className="w-3 h-3 stroke-[2]" />
                                  </button>
                                )}
                              </div>
                            ))
                          ) : (
                            <div className="text-center font-mono text-[9px] text-gray-400 py-2 italic">
                              No items in this checklist
                            </div>
                          )}
                        </div>

                        {/* Add Item form */}
                        {canEdit && (
                          <div className="flex items-center gap-1.5 mt-2.5">
                            <input
                              type="text"
                              placeholder="Add checklist item..."
                              value={newChecklistItemTexts[checklist.id] || ''}
                              onChange={(e) => setNewChecklistItemTexts(prev => ({ ...prev, [checklist.id]: e.target.value }))}
                              className="px-2 py-1 bg-white text-black font-mono text-xs border-2 border-black focus:outline-none focus:bg-[#FFFCEE] shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)] flex-1"
                              onKeyDown={(e) => {
                                if (e.key === 'Enter') {
                                  e.preventDefault();
                                  handleCreateChecklistItem(checklist.id);
                                }
                              }}
                            />
                            <Button
                              type="button"
                              variant="info"
                              onClick={() => handleCreateChecklistItem(checklist.id)}
                              className="py-1 px-3 text-[10px] font-mono shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)] shrink-0"
                            >
                              ADD ITEM
                            </Button>
                          </div>
                        )}
                      </div>
                    );
                  })
                ) : (
                  <div className="text-center font-mono text-[10px] text-gray-400 py-4 border-2 border-dashed border-black/20 bg-black/[0.01]">
                    No checklists created yet
                  </div>
                )}
              </div>
            </div>

            <div className="flex items-center gap-2 font-mono text-xs text-gray-500">
              <CreditCard className="w-4 h-4 text-gray-700" />
              <span>Card ID: {activeCard?.id}</span>
            </div>
          </div>

          <div className="flex justify-between items-center gap-3 mt-4 border-t-2 border-black pt-4">
            {canEdit ? (
              <>
                <Button
                  type="button"
                  variant="danger"
                  onClick={() => {
                    if (activeCard) {
                      handleDeleteCard(activeCard.id);
                    }
                  }}
                  disabled={cardUpdateLoading}
                  className="flex items-center gap-1.5"
                >
                  <Trash2 className="w-4 h-4" />
                  <span>DELETE CARD</span>
                </Button>

                <div className="flex gap-3">
                  <Button
                    type="button"
                    variant="info"
                    onClick={() => {
                      setIsCardDetailOpen(false);
                      setActiveCard(null);
                    }}
                    disabled={cardUpdateLoading}
                  >
                    CANCEL
                  </Button>
                  <Button
                    type="submit"
                    variant="success"
                    disabled={cardUpdateLoading}
                    className="flex items-center gap-1.5"
                  >
                    <Check className="w-4 h-4" />
                    <span>{cardUpdateLoading ? 'SAVING...' : 'SAVE CHANGES'}</span>
                  </Button>
                </div>
              </>
            ) : (
              <div className="flex justify-end w-full">
                <Button
                  type="button"
                  variant="info"
                  onClick={() => {
                    setIsCardDetailOpen(false);
                    setActiveCard(null);
                  }}
                >
                  CLOSE
                </Button>
              </div>
            )}
          </div>
        </form>
      </Modal>

      {/* SEPARATE COLOR PICKING MODAL */}
      <Modal
        isOpen={isColorModalOpen}
        onClose={() => setIsColorModalOpen(false)}
        title={
          <>
            <Palette className="w-5 h-5 text-pink-500 animate-pulse-subtle" />
            <span>CARD COLOR & STYLE</span>
          </>
        }
      >
        {activeCard ? (
          <div className="flex flex-col gap-5">
            {/* Instruction */}
            <p className="font-mono text-xs text-gray-700 leading-relaxed bg-slate-50 p-2.5 border-2 border-dashed border-black">
              Customize the visual presence of card <strong>"{activeCard.title}"</strong> on your board.
            </p>

            {/* Presets - 10 Colors only, no custom picker */}
            <div className="flex flex-col gap-2.5">
              <div className="flex justify-between items-center">
                <label className="font-mono text-xs font-bold text-black tracking-wide">
                  Select Preset Color
                </label>
                {activeCard.coverColor && (
                  <button
                    type="button"
                    onClick={() => handleUpdateCover(null)}
                    className="px-2 py-0.5 text-[9px] font-mono font-bold border-2 border-black bg-red-100 hover:bg-red-200 active:translate-x-[0.5px] active:translate-y-[0.5px] shadow-[1px_1px_0px_rgba(0,0,0,1)] text-black font-extrabold transition-colors"
                  >
                    ✕ Clear Color
                  </button>
                )}
              </div>
              <div className="flex flex-wrap items-center gap-2.5 bg-white border-2 border-black p-3.5 shadow-[2px_2px_0px_rgba(0,0,0,1)]">
                {[
                  '#FF6B6B', // Coral Red
                  '#FF8E53', // Warm Orange
                  '#FFDE4D', // Vibrant Yellow
                  '#6EE7B7', // Mint Green
                  '#22D3EE', // Electric Cyan
                  '#A5B4FC', // Soft Indigo
                  '#C3A6FF', // Bright Lavender
                  '#F9A8D4', // Bubblegum Pink
                  '#F87171', // Rose Red
                  '#4ADE80'  // Bright Green
                ].map(presetColor => (
                  <button
                    key={presetColor}
                    type="button"
                    onClick={() => handleUpdateCover(presetColor)}
                    className="w-8 h-8 border-2 border-black shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)] active:translate-x-[0.5px] active:translate-y-[0.5px] relative shrink-0 transition-all hover:scale-110"
                    style={{ backgroundColor: presetColor }}
                  >
                    {activeCard.coverColor === presetColor && (
                      <span className="absolute inset-0 flex items-center justify-center text-black font-black text-sm">✓</span>
                    )}
                  </button>
                ))}
                {!activeCard.coverColor && (
                  <span className="font-mono text-[10px] text-gray-400 italic ml-1">No color selected</span>
                )}
              </div>
            </div>

            {/* Style Mode Selector */}
            <div className="flex flex-col gap-2.5 border-t-2 border-dashed border-black pt-4">
              <label className="font-mono text-xs font-bold text-black tracking-wide">
                Color Mode
              </label>
              <div className="grid grid-cols-2 gap-3">
                <button
                  type="button"
                  onClick={() => updateCardColorMode(activeCard.id, 'accent')}
                  className={`py-2 text-xs font-mono font-bold border-2 border-black ${
                    (cardColorModes[activeCard.id] || 'accent') === 'accent'
                      ? 'bg-[#FFDE4D] text-black shadow-[2px_2px_0px_rgba(0,0,0,1)]'
                      : 'bg-white text-black hover:bg-gray-50 shadow-[1px_1px_0px_rgba(0,0,0,1)]'
                  }`}
                >
                  Top Accent
                </button>
                <button
                  type="button"
                  onClick={() => updateCardColorMode(activeCard.id, 'full')}
                  className={`py-2 text-xs font-mono font-bold border-2 border-black ${
                    (cardColorModes[activeCard.id] || 'accent') === 'full'
                      ? 'bg-[#FFDE4D] text-black shadow-[2px_2px_0px_rgba(0,0,0,1)]'
                      : 'bg-white text-black hover:bg-gray-50 shadow-[1px_1px_0px_rgba(0,0,0,1)]'
                  }`}
                >
                  Full Card Fill
                </button>
              </div>
            </div>

            {/* Preview Section */}
            <div className="flex flex-col gap-2 border-t-2 border-dashed border-black pt-4">
              <span className="font-mono text-[10px] font-bold text-gray-500 tracking-wider">Preview on Board</span>
              <div className="bg-slate-100 p-4 border-2 border-black flex justify-center items-center">
                <div 
                  style={(cardColorModes[activeCard.id] || 'accent') === 'full' && activeCard.coverColor ? { backgroundColor: activeCard.coverColor } : undefined}
                  className={`border-2 border-black shadow-[3px_3px_0px_rgba(0,0,0,1)] w-full max-w-xs flex flex-col overflow-hidden ${
                    (cardColorModes[activeCard.id] || 'accent') === 'full' && activeCard.coverColor ? '' : 'bg-white'
                  }`}
                >
                  {activeCard.coverColor && (cardColorModes[activeCard.id] || 'accent') === 'accent' && (
                    <div className="w-full h-3 border-b-2 border-black" style={{ backgroundColor: activeCard.coverColor }} />
                  )}
                  <div className="p-3">
                    <span className="font-sans font-bold text-xs text-black leading-tight block truncate">
                      {activeCard.title || 'PREVIEW CARD'}
                    </span>
                    {activeCard.description && (
                      <div className="text-gray-500 mt-1" title="This card has a description">
                        <AlignLeft className="w-3.5 h-3.5 stroke-[2.5]" />
                      </div>
                    )}
                  </div>
                </div>
              </div>
            </div>

            {/* Footer buttons */}
            <div className="flex justify-end mt-2 border-t-2 border-black pt-4">
              <Button
                type="button"
                variant="info"
                onClick={() => setIsColorModalOpen(false)}
              >
                CLOSE SETTINGS
              </Button>
            </div>
          </div>
        ) : (
          <div className="text-center py-6 font-mono text-sm text-gray-400">
            No active card selected
          </div>
        )}
      </Modal>

      {/* SEPARATE DEADLINE MODAL */}
      <Modal
        isOpen={isDueDateModalOpen}
        onClose={() => setIsDueDateModalOpen(false)}
        title={
          <>
            <Calendar className="w-5 h-5 text-yellow-500" />
            <span>CARD DEADLINE</span>
          </>
        }
      >
        {activeCard ? (
          <form onSubmit={handleSaveDueDate} className="flex flex-col gap-5">
            <p className="font-mono text-xs text-gray-700 leading-relaxed bg-slate-50 p-2.5 border-2 border-dashed border-black">
              Set a hard due date and reminder notification date/time for card <strong>"{activeCard.title}"</strong>.
            </p>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div className="flex flex-col gap-1 w-full">
                <label className="font-mono text-xs font-bold text-black tracking-wide">
                  Due Date
                </label>
                <input
                  type="date"
                  value={dueDateInput}
                  onChange={(e) => setDueDateInput(e.target.value)}
                  disabled={dueDateLoading}
                  className="w-full px-3 py-2 bg-white text-black font-mono text-sm border-2 border-black shadow-[2px_2px_0px_rgba(0,0,0,1)] focus:outline-none focus:bg-[#FFFCEE] transition-all"
                  required
                />
              </div>

              <div className="flex flex-col gap-1 w-full">
                <label className="font-mono text-xs font-bold text-black tracking-wide">
                  Due Time
                </label>
                <input
                  type="time"
                  value={dueTimeInput}
                  onChange={(e) => setDueTimeInput(e.target.value)}
                  disabled={dueDateLoading}
                  className="w-full px-3 py-2 bg-white text-black font-mono text-sm border-2 border-black shadow-[2px_2px_0px_rgba(0,0,0,1)] focus:outline-none focus:bg-[#FFFCEE] transition-all"
                />
              </div>
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 border-t-2 border-dashed border-black pt-4">
              <div className="flex flex-col gap-1 w-full">
                <label className="font-mono text-xs font-bold text-black tracking-wide flex items-center gap-1">
                  <span>Reminder Date</span>
                  <span className="text-[9px] text-gray-500 font-normal">(Optional)</span>
                </label>
                <input
                  type="date"
                  value={reminderDateInput}
                  onChange={(e) => setReminderDateInput(e.target.value)}
                  disabled={dueDateLoading}
                  className="w-full px-3 py-2 bg-white text-black font-mono text-sm border-2 border-black shadow-[2px_2px_0px_rgba(0,0,0,1)] focus:outline-none focus:bg-[#FFFCEE] transition-all"
                />
              </div>

              <div className="flex flex-col gap-1 w-full">
                <label className="font-mono text-xs font-bold text-black tracking-wide">
                  Reminder Time
                </label>
                <input
                  type="time"
                  value={reminderTimeInput}
                  onChange={(e) => setReminderTimeInput(e.target.value)}
                  disabled={dueDateLoading}
                  className="w-full px-3 py-2 bg-white text-black font-mono text-sm border-2 border-black shadow-[2px_2px_0px_rgba(0,0,0,1)] focus:outline-none focus:bg-[#FFFCEE] transition-all"
                />
              </div>
            </div>

            <div className="flex justify-between items-center mt-2 border-t-2 border-black pt-4 gap-2">
              {activeCard.dueDate ? (
                <button
                  type="button"
                  onClick={handleClearDueDate}
                  disabled={dueDateLoading}
                  className="px-4 py-2 text-xs font-mono font-bold border-2 border-black bg-rose-100 hover:bg-rose-200 active:translate-x-[0.5px] active:translate-y-[0.5px] shadow-[2px_2px_0px_rgba(0,0,0,1)] text-red-600 transition-colors"
                >
                  {dueDateLoading ? 'CLEARING...' : 'CLEAR DEADLINE'}
                </button>
              ) : (
                <div />
              )}

              <div className="flex items-center gap-2">
                <button
                  type="button"
                  onClick={() => setIsDueDateModalOpen(false)}
                  disabled={dueDateLoading}
                  className="px-4 py-2 text-xs font-mono font-bold border-2 border-black bg-white hover:bg-gray-50 active:translate-x-[0.5px] active:translate-y-[0.5px] shadow-[2px_2px_0px_rgba(0,0,0,1)] text-black transition-colors"
                >
                  CANCEL
                </button>
                <Button
                  type="submit"
                  variant="warning"
                  disabled={dueDateLoading}
                >
                  {dueDateLoading ? 'SAVING...' : 'SAVE DEADLINE'}
                </Button>
              </div>
            </div>
          </form>
        ) : (
          <div className="text-center py-6 font-mono text-sm text-gray-400">
            No active card selected
          </div>
        )}
      </Modal>

      {/* SEPARATE LABELS MODAL */}
      <Modal
        isOpen={isLabelsModalOpen}
        onClose={() => setIsLabelsModalOpen(false)}
        title={
          <>
            <Tag className="w-5 h-5 text-emerald-500" />
            <span>CARD LABELS</span>
          </>
        }
      >
        {activeCard ? (
          <div className="flex flex-col gap-5">
            <p className="font-mono text-xs text-gray-700 leading-relaxed bg-slate-50 p-2.5 border-2 border-dashed border-black">
              Click a label below to toggle its assignment on card <strong>"{activeCard.title}"</strong>.
            </p>

            {/* Existing Board Labels to assign/remove */}
            <div className="flex flex-col gap-2.5">
              <label className="font-mono text-xs font-bold text-black tracking-wide uppercase">
                Available Labels (Toggle to Assign)
              </label>
              
              <div className="flex flex-wrap gap-2 bg-white border-2 border-black p-3.5 shadow-[2px_2px_0px_rgba(0,0,0,1)] min-h-[60px]">
                {board?.labels && board.labels.length > 0 ? (
                  board.labels.map(lbl => {
                    const isAssigned = activeCard.labels?.some(l => l.id === lbl.id) ?? false;
                    return (
                      <button
                        key={lbl.id}
                        type="button"
                        onClick={() => handleToggleCardLabel(lbl.id, isAssigned)}
                        className={`flex items-center gap-1.5 px-3 py-1.5 font-mono text-xs font-black border-2 border-black shadow-[2px_2px_0px_rgba(0,0,0,1)] active:translate-x-[0.5px] active:translate-y-[0.5px] transition-all select-none ${
                          isAssigned 
                            ? 'scale-105 opacity-100 ring-2 ring-black' 
                            : 'opacity-70 hover:opacity-100 hover:scale-102'
                        }`}
                        style={{ backgroundColor: lbl.color || '#E2E8F0' }}
                        title={isAssigned ? 'Click to unassign' : 'Click to assign'}
                      >
                        <span>{lbl.name}</span>
                        {isAssigned ? (
                          <span className="font-black text-[11px]">✓</span>
                        ) : (
                          <span className="font-black text-[11px] opacity-30">+</span>
                        )}
                      </button>
                    );
                  })
                ) : (
                  <div className="text-center w-full font-mono text-xs text-gray-400 py-2 italic">
                    No labels created for this board yet. Create one below!
                  </div>
                )}
              </div>
            </div>

            {/* Create New Board Label Section */}
            {canEdit && (
              <form onSubmit={handleCreateBoardLabel} className="flex flex-col gap-3 border-t-2 border-dashed border-black pt-4">
                <label className="font-mono text-xs font-bold text-black tracking-wide uppercase">
                  Create a New Board Label
                </label>

                <div className="flex flex-col gap-3 sm:flex-row items-end">
                  <div className="flex-1 w-full">
                    <Input
                      label="Label Name"
                      type="text"
                      placeholder="e.g. High Priority, Bug, Marketing..."
                      value={newLabelName}
                      onChange={(e) => setNewLabelName(e.target.value)}
                      disabled={labelsLoading}
                    />
                  </div>

                  <div className="flex flex-col gap-1 shrink-0">
                    <label className="font-mono text-[10px] font-bold text-black">
                      Color
                    </label>
                    <div className="flex items-center gap-2 border-2 border-black px-2.5 py-1.5 bg-white shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)]">
                      <input
                        type="color"
                        value={newLabelColor}
                        onChange={(e) => setNewLabelColor(e.target.value)}
                        disabled={labelsLoading}
                        className="w-8 h-8 border-2 border-black rounded-none cursor-pointer p-0 bg-transparent"
                      />
                      <span className="font-mono text-xs uppercase font-bold">{newLabelColor}</span>
                    </div>
                  </div>

                  <Button
                    type="submit"
                    variant="info"
                    disabled={labelsLoading}
                    className="w-full sm:w-auto h-fit shadow-[1.5px_1.5px_0px_rgba(0,0,0,1)]"
                  >
                    {labelsLoading ? 'CREATING...' : 'CREATE LABEL'}
                  </Button>
                </div>
              </form>
            )}

            {/* Close footer */}
            <div className="flex justify-end border-t-2 border-black pt-4 mt-2">
              <Button
                type="button"
                variant="info"
                onClick={() => setIsLabelsModalOpen(false)}
              >
                CLOSE
              </Button>
            </div>
          </div>
        ) : (
          <div className="text-center py-6 font-mono text-sm text-gray-400">
            No active card selected
          </div>
        )}
      </Modal>

    </div>
  );
};
