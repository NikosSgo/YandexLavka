import { httpClient } from '@shared/api/httpClient';
import type {
  Account,
  LoginRequest,
  RegisterRequest,
  TokenResponse,
} from '@entities/auth/model/types';

class AuthApi {
  async register(payload: RegisterRequest): Promise<TokenResponse> {
    const { data } = await httpClient.post<TokenResponse>(
      '/api/auth/register',
      payload,
    );

    return data;
  }

  async login(payload: LoginRequest): Promise<TokenResponse> {
    const { data } = await httpClient.post<TokenResponse>(
      '/api/auth/login',
      payload,
    );

    return data;
  }

  async getAccount(accountId: string): Promise<Account> {
    const { data } = await httpClient.get<Account>(`/api/auth/account/${accountId}`);
    return data;
  }
}

export const authApi = new AuthApi();


