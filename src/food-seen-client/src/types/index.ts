export interface User {
  id: string;
  email: string;
  username: string;
  firstName?: string;
  lastName?: string;
  avatarUrl?: string;
  defaultLatitude?: number;
  defaultLongitude?: number;
  defaultRadiusKm: number;
  createdAt: string;
}

export interface Post {
  id: string;
  userId: string;
  authorUsername: string;
  title: string;
  description: string;
  address: string;
  latitude: number;
  longitude: number;
  distanceKm?: number;
  eventDate: string;
  eventEndDate?: string;
  isActive: boolean;
  categories: string[];
  createdAt: string;
}

export interface Category {
  id: string;
  name: string;
  description?: string;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface AuthResult {
  success: boolean;
  accessToken?: string;
  refreshToken?: string;
  user?: User;
  error?: string;
}

export interface CreatePostRequest {
  title: string;
  description: string;
  address: string;
  latitude: number;
  longitude: number;
  eventDate: string;
  eventEndDate?: string;
  categoryIds?: string[];
}

export interface UpdatePostRequest {
  title?: string;
  description?: string;
  address?: string;
  latitude?: number;
  longitude?: number;
  eventDate?: string;
  eventEndDate?: string;
  isActive?: boolean;
  categoryIds?: string[];
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  username: string;
  firstName?: string;
  lastName?: string;
}

export interface LocationCoords {
  latitude: number;
  longitude: number;
}
