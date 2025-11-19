import type { Account, TokenResponse } from '@entities/auth/model/types';

class AuthStorage {
  private tokenKey = 'auth_token';
  private refreshKey = 'refresh_token';
  private accountKey = 'account';

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.refreshKey);
  }

  getAccount(): Account | null {
    const raw = localStorage.getItem(this.accountKey);
    if (!raw) {
      return null;
    }

    try {
      return JSON.parse(raw) as Account;
    } catch {
      return null;
    }
  }

  saveAuthResponse(response: TokenResponse): void {
    localStorage.setItem(this.tokenKey, response.accessToken);
    localStorage.setItem(this.refreshKey, response.refreshToken);
    localStorage.setItem(this.accountKey, JSON.stringify(response.account));
  }

  clear(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.refreshKey);
    localStorage.removeItem(this.accountKey);
  }

  isTokenValid(token: string | null): boolean {
    if (!token) {
      return false;
    }

    try {
      const payload = JSON.parse(atob(token.split('.')[1] ?? ''));
      const expirationMs = payload.exp ? payload.exp * 1000 : 0;
      return Date.now() < expirationMs;
    } catch {
      return false;
    }
  }
}

export const authStorage = new AuthStorage();


