/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
 */


export interface UserDto {
  id: string;
  username: string | null;
}

export interface RegisterRequest {
  username?: string | null;
  password?: string | null;
}

export interface LoginRequest {
  username?: string | null;
  password?: string | null;
}

export interface ChecklistItemDto {
  id: string;
  checklistId: string;
  text: string | null;
  isCompleted: boolean;
  position: number;
}

export interface ChecklistDto {
  id: string;
  cardId: string;
  title: string | null;
  items: ChecklistItemDto[] | null;
}

export interface LabelDto {
  id: string;
  name: string | null;
  color: string | null;
}

export interface CardDto {
  id: string;
  columnId: string;
  title: string | null;
  description: string | null;
  position?: number; // Added for drag and drop support
  coverColor: string | null;
  checklists: ChecklistDto[] | null;
  dueDate?: string | null;
  reminderDate?: string | null;
  labels?: LabelDto[] | null;
}

export interface ColumnDto {
  id: string;
  boardId: string;
  title?: string | null;
  Title?: string | null; // Support uppercase Title from API
  cards: CardDto[] | null;
  position?: number; // Added for drag and drop support
}

export enum AccessLevel {
  Viewer = 0,
  Member = 1,
  Admin = 2,
}

export interface BoardMemberDto {
  userId: string;
  accessLevel: AccessLevel;
  username?: string | null;
}

export interface BoardDto {
  id: string;
  ownerId: string;
  isPublic: boolean;
  title: string | null;
  members: BoardMemberDto[] | null;
  columns: ColumnDto[] | null;
  labels?: LabelDto[] | null;
}

export interface BoardLookupDto {
  id: string;
  ownerId: string;
  isPublic: boolean;
  title: string | null;
  memberCount: number;
  columnCount: number;
}

export interface CreateBoardRequest {
  title?: string | null;
  isPublic: boolean;
}

export interface UpdateBoardTitleRequest {
  title?: string | null;
}

export interface ChangeBoardVisibilityRequest {
  newIsPublic: boolean;
}

export interface UpdateColumnTitleRequest {
  title?: string | null;
}

export interface MoveColumnRequest {
  newPosition: number;
}

export interface UpdateCardContentRequest {
  title?: string | null;
  description?: string | null;
}

export interface UpdateCardCoverRequest {
  color: string | null;
}

export interface CreateChecklistRequest {
  title?: string | null;
}

export interface CreateChecklistItemRequest {
  text?: string | null;
}

export interface ToggleChecklistItemRequest {
  isCompleted: boolean;
}

export interface MoveCardRequest {
  newColumnId: string;
  newPosition: number;
}

export interface UpdateBoardRequest {
  newTitle?: string | null;
  newIsPublic: boolean;
}

export interface UpdateMemberAccessLevelRequest {
  newAccessLevel: AccessLevel;
}

export interface CreateColumnRequest {
  boardId: string;
  title?: string | null;
}

export interface UpdateColumnRequest {
  title?: string | null;
  Title?: string | null; // Support uppercase Title from API/DB
  position?: number | null;
}

export interface CreateCardRequest {
  columnId: string;
  title?: string | null;
}

export interface UpdateCardRequest {
  title?: string | null;
  description?: string | null;
  columnId?: string | null;
  position?: number | null;
}

export interface AddBoardMemberRequest {
  memberId: string;
}

export interface AuthResponse {
  token: string | null;
}

export interface CreationResponse {
  id: string;
}

export interface UserListResponse {
  users: UserDto[] | null;
}

export enum ChatMessageRole {
  Unknown = 0,
  User = 1,
  Ai = 2,
  System = 3,
}

export enum AiProvider {
  Unknown = 0,
  OpenAI = 1,
  OpenRouter = 2,
  Gemini = 3,
  Anthropic = 4,
}

export interface BoardAiSettingsDto {
  apiKey: string | null;
  provider: AiProvider;
  model: string | null;
}

export interface UpdateBoardAiSettingsRequest {
  apiKey: string | null;
  provider: AiProvider;
  model: string | null;
}

export interface MessageDto {
  messageId: string;
  role: ChatMessageRole;
  content: string | null;
}

export interface ChatMessagesDto {
  messages: MessageDto[] | null;
}


