export interface RegisterRequest {
  email: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface Account {
  id: string;
  email: string;
  isActive: boolean;
  lastLoginAt?: string;
  createdAt: string;
}

export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  account: Account;
}

export interface AuthState {
  isAuthenticated: boolean;
  token: string | null;
  account: Account | null;
}


