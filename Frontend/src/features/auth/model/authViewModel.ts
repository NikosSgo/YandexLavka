import { makeAutoObservable, runInAction } from 'mobx';
import { AuthRepository } from '@entities/auth/model/authRepository';
import type {
  Account,
  LoginRequest,
  RegisterRequest,
  TokenResponse,
} from '@entities/auth/model/types';
import { authStorage } from '@shared/lib/storage/authStorage';

export class AuthViewModel {
  account: Account | null = authStorage.getAccount();
  token: string | null = authStorage.getToken();
  isLoading = false;
  error: string | null = null;
  private readonly repository: AuthRepository;

  constructor(repository = new AuthRepository()) {
    this.repository = repository;
    makeAutoObservable(this, {}, { autoBind: true });
  }

  get isAuthenticated(): boolean {
    return authStorage.isTokenValid(this.token);
  }

  hydrate(): void {
    this.account = authStorage.getAccount();
    this.token = authStorage.getToken();
  }

  private handleAuthResponse(response: TokenResponse): void {
    authStorage.saveAuthResponse(response);
    runInAction(() => {
      this.account = response.account;
      this.token = response.accessToken;
    });
  }

  async login(payload: LoginRequest): Promise<void> {
    this.isLoading = true;
    this.error = null;

    try {
      const response = await this.repository.login(payload);
      this.handleAuthResponse(response);
    } catch (error) {
      const message =
        error instanceof Error ? error.message : 'Не удалось выполнить вход';
      runInAction(() => {
        this.error = message;
      });
      throw error;
    } finally {
      runInAction(() => {
        this.isLoading = false;
      });
    }
  }

  async register(payload: RegisterRequest): Promise<void> {
    this.isLoading = true;
    this.error = null;

    try {
      const response = await this.repository.register(payload);
      this.handleAuthResponse(response);
    } catch (error) {
      const message =
        error instanceof Error ? error.message : 'Не удалось создать аккаунт';
      runInAction(() => {
        this.error = message;
      });
      throw error;
    } finally {
      runInAction(() => {
        this.isLoading = false;
      });
    }
  }

  async refreshAccount(): Promise<void> {
    if (!this.account?.id) {
      return;
    }

    try {
      const account = await this.repository.getAccount(this.account.id);
      runInAction(() => {
        this.account = account;
      });
      localStorage.setItem('account', JSON.stringify(account));
    } catch (error) {
      console.warn('Не удалось обновить аккаунт', error);
    }
  }

  logout(): void {
    authStorage.clear();
    runInAction(() => {
      this.account = null;
      this.token = null;
    });
  }
}


