import axios from 'axios';
import type { Post, Category, PaginatedResult, CreatePostRequest, UpdatePostRequest, AuthResult, LoginRequest, RegisterRequest, User } from '@/types';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || '/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add auth token to requests
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Handle token refresh on 401
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      const refreshToken = localStorage.getItem('refreshToken');
      if (refreshToken) {
        try {
          const response = await axios.post('/api/auth/refresh', { refreshToken });
          const { accessToken, refreshToken: newRefreshToken } = response.data;

          localStorage.setItem('accessToken', accessToken);
          localStorage.setItem('refreshToken', newRefreshToken);

          originalRequest.headers.Authorization = `Bearer ${accessToken}`;
          return api(originalRequest);
        } catch {
          localStorage.removeItem('accessToken');
          localStorage.removeItem('refreshToken');
          window.location.href = '/login';
        }
      }
    }

    return Promise.reject(error);
  }
);

// Posts API
export const postsApi = {
  getAll: async (page = 1, pageSize = 20): Promise<PaginatedResult<Post>> => {
    const response = await api.get('/posts', { params: { page, pageSize } });
    return response.data;
  },

  getNearby: async (
    latitude: number,
    longitude: number,
    radiusKm = 10,
    page = 1,
    pageSize = 20
  ): Promise<PaginatedResult<Post>> => {
    const response = await api.get('/posts/nearby', {
      params: { latitude, longitude, radiusKm, page, pageSize },
    });
    return response.data;
  },

  search: async (query: string, page = 1, pageSize = 20): Promise<PaginatedResult<Post>> => {
    const response = await api.get('/posts/search', { params: { q: query, page, pageSize } });
    return response.data;
  },

  getById: async (id: string): Promise<Post> => {
    const response = await api.get(`/posts/${id}`);
    return response.data;
  },

  getByUser: async (userId: string): Promise<Post[]> => {
    const response = await api.get(`/posts/user/${userId}`);
    return response.data;
  },

  getMyPosts: async (): Promise<Post[]> => {
    const response = await api.get('/posts/my');
    return response.data;
  },

  create: async (data: CreatePostRequest): Promise<Post> => {
    const response = await api.post('/posts', data);
    return response.data;
  },

  update: async (id: string, data: UpdatePostRequest): Promise<Post> => {
    const response = await api.put(`/posts/${id}`, data);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/posts/${id}`);
  },
};

// Categories API
export const categoriesApi = {
  getAll: async (): Promise<Category[]> => {
    const response = await api.get('/categories');
    return response.data;
  },

  getPosts: async (categoryId: string): Promise<Post[]> => {
    const response = await api.get(`/categories/${categoryId}/posts`);
    return response.data;
  },
};

// Auth API
export const authApi = {
  register: async (data: RegisterRequest): Promise<AuthResult> => {
    const response = await api.post('/auth/register', data);
    return response.data;
  },

  login: async (data: LoginRequest): Promise<AuthResult> => {
    const response = await api.post('/auth/login', data);
    return response.data;
  },

  refresh: async (refreshToken: string): Promise<AuthResult> => {
    const response = await api.post('/auth/refresh', { refreshToken });
    return response.data;
  },

  logout: async (refreshToken: string): Promise<void> => {
    await api.post('/auth/logout', { refreshToken });
  },

  me: async (): Promise<User> => {
    const response = await api.get('/auth/me');
    return response.data;
  },
};

export default api;
