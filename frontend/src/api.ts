/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
 */

import {
  RegisterRequest,
  LoginRequest,
  UserDto,
  BoardDto,
  BoardLookupDto,
  CreateBoardRequest,
  UpdateBoardRequest,
  CreateColumnRequest,
  UpdateColumnRequest,
  CreateCardRequest,
  UpdateCardRequest,
  AddBoardMemberRequest,
  CardDto,
  UpdateMemberAccessLevelRequest,
  AuthResponse,
  CreationResponse,
  UserListResponse,
  UpdateBoardTitleRequest,
  ChangeBoardVisibilityRequest,
  UpdateColumnTitleRequest,
  MoveColumnRequest,
  UpdateCardContentRequest,
  MoveCardRequest,
  UpdateCardCoverRequest,
  CreateChecklistRequest,
  CreateChecklistItemRequest,
  ToggleChecklistItemRequest,
  ChatMessagesDto,
  LabelDto,
  BoardAiSettingsDto,
  UpdateBoardAiSettingsRequest,
} from './types';

export function getApiBaseUrl(): string {
  return '';
  // const envUrl = (import.meta as any).env?.VITE_API_URL;
  // return envUrl ? envUrl.trim() : '';
}

export function getAuthToken(): string | null {
  return localStorage.getItem('synkan_auth_token');
}

export function setAuthToken(token: string | null) {
  if (token) {
    localStorage.setItem('synkan_auth_token', token);
  } else {
    localStorage.removeItem('synkan_auth_token');
  }
}

export function logout() {
  localStorage.removeItem('synkan_auth_token');
  localStorage.removeItem('synkan_user');
}

async function request<T>(
  path: string,
  method: 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH' = 'GET',
  body?: any,
  asPlainText = false
): Promise<T> {
  const baseUrl = getApiBaseUrl();
  const url = `${baseUrl}${path}`;
  const token = getAuthToken();

  const headers: HeadersInit = {};
  
  if (body !== undefined) {
    headers['Content-Type'] = 'application/json';
  }
  
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const options: RequestInit = {
    method,
    headers,
    credentials: 'include',
  };

  if (body !== undefined) {
    options.body = JSON.stringify(body);
  }

  let response: Response;
  try {
    response = await fetch(url, options);
  } catch (error) {
    console.error('Fetch error:', error);
    throw new Error(`Failed to connect to backend at ${baseUrl}. Ensure the server is running on localhost:5234 and CORS is enabled.`);
  }

  if (!response.ok) {
    let errorMsg = `Server error ${response.status}`;
    try {
      const errorText = await response.text();
      if (errorText) {
        errorMsg = errorText;
      }
    } catch (_) {}
    throw new Error(errorMsg);
  }

  // Handle DELETE or PUT with empty response or 204
  if (response.status === 204) {
    return {} as T;
  }

  if (asPlainText) {
    const text = await response.text();
    return text as unknown as T;
  }

  const contentType = response.headers.get('Content-Type');
  if (contentType && contentType.includes('application/json')) {
    return await response.json();
  }

  const text = await response.text();
  try {
    // Try to parse JSON anyway
    return JSON.parse(text) as T;
  } catch (_) {
    return text as unknown as T;
  }
}

export const api = {
  // Auth endpoints
  async register(data: RegisterRequest): Promise<AuthResponse> {
    return request<AuthResponse>('/api/auth/register', 'POST', data);
  },

  async login(data: LoginRequest): Promise<AuthResponse> {
    return request<AuthResponse>('/api/auth/login', 'POST', data);
  },

  async getMe(): Promise<UserDto> {
    return request<UserDto>('/api/auth/me', 'GET');
  },

  // Boards endpoints
  async getBoards(): Promise<BoardLookupDto[]> {
    return request<BoardLookupDto[]>('/api/boards', 'GET');
  },

  async createBoard(data: CreateBoardRequest): Promise<CreationResponse> {
    return request<CreationResponse>('/api/boards', 'POST', data);
  },

  async getBoardById(id: string): Promise<BoardDto> {
    return request<BoardDto>(`/api/boards/${id}`, 'GET');
  },

  async editBoardTitle(id: string, data: UpdateBoardTitleRequest): Promise<void> {
    return request<void>(`/api/boards/${id}`, 'PATCH', data);
  },

  async changeBoardVisibility(id: string, data: ChangeBoardVisibilityRequest): Promise<void> {
    return request<void>(`/api/boards/${id}/visibility`, 'POST', data);
  },

  async deleteBoard(id: string): Promise<void> {
    return request<void>(`/api/boards/${id}`, 'DELETE');
  },

  async addBoardMember(boardId: string, data: AddBoardMemberRequest): Promise<void> {
    return request<void>(`/api/boards/${boardId}/members`, 'POST', data);
  },

  async removeBoardMember(boardId: string, memberId: string): Promise<void> {
    return request<void>(`/api/boards/${boardId}/members/${memberId}`, 'DELETE');
  },

  async updateMemberAccessLevel(boardId: string, memberId: string, data: UpdateMemberAccessLevelRequest): Promise<void> {
    return request<void>(`/api/boards/${boardId}/members/${memberId}`, 'PUT', data);
  },

  // Columns endpoints
  async createColumn(data: CreateColumnRequest): Promise<CreationResponse> {
    return request<CreationResponse>('/api/columns', 'POST', data);
  },

  async editColumnTitle(id: string, data: UpdateColumnTitleRequest): Promise<void> {
    return request<void>(`/api/columns/${id}`, 'PATCH', data);
  },

  async moveColumn(id: string, data: MoveColumnRequest): Promise<void> {
    return request<void>(`/api/columns/${id}/move`, 'POST', data);
  },

  async deleteColumn(id: string): Promise<void> {
    return request<void>(`/api/columns/${id}`, 'DELETE');
  },

  // Cards endpoints
  async createCard(data: CreateCardRequest): Promise<CreationResponse> {
    return request<CreationResponse>('/api/cards', 'POST', data);
  },

  async getCard(id: string): Promise<CardDto> {
    return request<CardDto>(`/api/cards/${id}`, 'GET');
  },

  async updateCardContent(id: string, data: UpdateCardContentRequest): Promise<void> {
    return request<void>(`/api/cards/${id}`, 'PATCH', data);
  },

  async moveCard(id: string, data: MoveCardRequest): Promise<void> {
    return request<void>(`/api/cards/${id}/move`, 'POST', data);
  },

  async deleteCard(id: string): Promise<void> {
    return request<void>(`/api/cards/${id}`, 'DELETE');
  },
  
  async updateCardCover(id: string, data: UpdateCardCoverRequest): Promise<void> {
    return request<void>(`/api/cards/${id}/cover`, 'PUT', data);
  },

  async createChecklist(cardId: string, data: CreateChecklistRequest): Promise<CreationResponse> {
    return request<CreationResponse>(`/api/cards/${cardId}/checklists`, 'POST', data);
  },

  async deleteChecklist(cardId: string, checklistId: string): Promise<void> {
    return request<void>(`/api/cards/${cardId}/checklists/${checklistId}`, 'DELETE');
  },

  async createChecklistItem(cardId: string, checklistId: string, data: CreateChecklistItemRequest): Promise<void> {
    return request<void>(`/api/cards/${cardId}/checklists/${checklistId}/items`, 'POST', data);
  },

  async toggleChecklistItem(cardId: string, checklistId: string, itemId: string, data: ToggleChecklistItemRequest): Promise<void> {
    return request<void>(`/api/cards/${cardId}/checklists/${checklistId}/items/${itemId}`, 'PUT', data);
  },

  async deleteChecklistItem(cardId: string, checklistId: string, itemId: string): Promise<void> {
    return request<void>(`/api/cards/${cardId}/checklists/${checklistId}/items/${itemId}`, 'DELETE');
  },

  // Users endpoints
  async searchUsers(username: string): Promise<UserListResponse> {
    return request<UserListResponse>(`/api/users?username=${encodeURIComponent(username)}`, 'GET');
  },

  // Chat endpoints
  async getBoardChat(boardId: string): Promise<ChatMessagesDto> {
    return request<ChatMessagesDto>(`/api/boards/${boardId}/chat`, 'GET');
  },

  // Due Date endpoints
  async updateCardDueDate(id: string, data: { dueDate: string; reminderTime: string }): Promise<void> {
    return request<void>(`/api/cards/${id}/due`, 'PUT', data);
  },

  async deleteCardDueDate(id: string): Promise<void> {
    return request<void>(`/api/cards/${id}/due`, 'DELETE');
  },

  // Board Label endpoints
  async createBoardLabel(boardId: string, data: { name: string; color: string }): Promise<CreationResponse> {
    return request<CreationResponse>(`/api/boards/${boardId}/labels`, 'POST', data);
  },

  // Card Label endpoints
  async addCardLabel(cardId: string, labelId: string): Promise<void> {
    return request<void>(`/api/cards/${cardId}/labels/${labelId}`, 'POST');
  },

  async removeCardLabel(cardId: string, labelId: string): Promise<void> {
    return request<void>(`/api/cards/${cardId}/labels/${labelId}`, 'DELETE');
  },

  // AI Settings endpoints
  async getBoardAiSettings(boardId: string): Promise<BoardAiSettingsDto> {
    return request<BoardAiSettingsDto>(`/api/boards/${boardId}/settings`, 'GET');
  },

  async updateBoardAiSettings(boardId: string, data: UpdateBoardAiSettingsRequest): Promise<void> {
    return request<void>(`/api/boards/${boardId}/settings`, 'PUT', data);
  }
};

