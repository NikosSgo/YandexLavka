import type {
  Account,
  LoginRequest,
  RegisterRequest,
  TokenResponse,
} from '@entities/auth/model/types';
import { authApi } from '@entities/auth/api/authApi';

export class AuthRepository {
  private readonly api;

  constructor(api = authApi) {
    this.api = api;
  }

  register(payload: RegisterRequest): Promise<TokenResponse> {
    return this.api.register(payload);
  }

  login(payload: LoginRequest): Promise<TokenResponse> {
    return this.api.login(payload);
  }

  getAccount(accountId: string): Promise<Account> {
    return this.api.getAccount(accountId);
  }
}


