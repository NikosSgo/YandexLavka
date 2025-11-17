import { apiClient } from './apiClient';
import type { RegisterRequest, LoginRequest, TokenResponse, Account } from '../models/Auth';

class AuthService {
  async register(data: RegisterRequest): Promise<TokenResponse> {
    const response = await apiClient.post<TokenResponse>('/api/auth/register', data);
    this.setAuthData(response);
    return response;
  }

  async login(data: LoginRequest): Promise<TokenResponse> {
    const response = await apiClient.post<TokenResponse>('/api/auth/login', data);
    this.setAuthData(response);
    return response;
  }

  async getAccount(accountId: string): Promise<Account> {
    return await apiClient.get<Account>(`/api/auth/account/${accountId}`);
  }

  logout(): void {
    localStorage.removeItem('auth_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('account');
  }

  getToken(): string | null {
    return localStorage.getItem('auth_token');
  }

  getAccountFromStorage(): Account | null {
    const accountStr = localStorage.getItem('account');
    if (!accountStr) return null;
    try {
      return JSON.parse(accountStr);
    } catch {
      return null;
    }
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    if (!token) return false;

    // Проверяем, не истек ли токен (базовая проверка)
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const exp = payload.exp * 1000; // конвертируем в миллисекунды
      return Date.now() < exp;
    } catch {
      return false;
    }
  }

  private setAuthData(response: TokenResponse): void {
    localStorage.setItem('auth_token', response.accessToken);
    localStorage.setItem('refresh_token', response.refreshToken);
    localStorage.setItem('account', JSON.stringify(response.account));
  }
}

export const authService = new AuthService();


