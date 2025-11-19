import { makeAutoObservable, reaction, runInAction } from 'mobx';
import type {
  Address,
  CreateAddressRequest,
  CreateUserRequest,
  UpdateAddressRequest,
  UpdateUserRequest,
  User,
} from '@entities/user/model/types';
import { UserRepository } from '@entities/user/model/userRepository';
import { AuthViewModel } from '@features/auth/model/authViewModel';

export class ProfileViewModel {
  user: User | null = null;
  addresses: Address[] = [];
  isLoading = false;
  error: string | null = null;
  private readonly repository: UserRepository;
  private readonly auth: AuthViewModel;

  constructor(repository: UserRepository, auth: AuthViewModel) {
    this.repository = repository;
    this.auth = auth;
    makeAutoObservable(this, {}, { autoBind: true });

    reaction(
      () => this.auth.account?.email,
      (email) => {
        if (email) {
          this.loadUserByEmail(email);
        } else {
          this.reset();
        }
      },
      { fireImmediately: true },
    );
  }

  get hasProfile(): boolean {
    return Boolean(this.user);
  }

  get primaryAddress(): Address | null {
    return this.addresses.find((address) => address.isPrimary) ?? null;
  }

  private reset(): void {
    this.user = null;
    this.addresses = [];
    this.error = null;
  }

  private async loadUserByEmail(email: string): Promise<void> {
    this.isLoading = true;
    this.error = null;

    try {
      const user = await this.repository.getProfileByEmail(email);
      runInAction(() => {
        this.user = user;
        this.addresses = user.addresses ?? [];
      });
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Профиль не найден';
      runInAction(() => {
        this.error = message;
        this.user = null;
        this.addresses = [];
      });
      throw error;
    } finally {
      runInAction(() => {
        this.isLoading = false;
      });
    }
  }

  async reload(): Promise<void> {
    const email = this.auth.account?.email;
    if (!email) {
      return;
    }
    await this.loadUserByEmail(email);
  }

  async createProfile(payload: CreateUserRequest): Promise<User> {
    this.isLoading = true;
    this.error = null;

    try {
      const user = await this.repository.createProfile(payload);
      runInAction(() => {
        this.user = user;
        this.addresses = user.addresses ?? [];
      });
      return user;
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Не удалось создать профиль';
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

  async updateProfile(payload: UpdateUserRequest): Promise<User> {
    if (!this.user) {
      throw new Error('Профиль не найден');
    }

    this.isLoading = true;
    this.error = null;

    try {
      const user = await this.repository.updateProfile(this.user.id, payload);
      runInAction(() => {
        this.user = user;
      });
      return user;
    } catch (error) {
      const message =
        error instanceof Error ? error.message : 'Не удалось обновить профиль';
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

  async addAddress(payload: CreateAddressRequest): Promise<Address> {
    if (!this.user) {
      throw new Error('Создайте профиль перед добавлением адреса');
    }

    this.isLoading = true;
    this.error = null;

    try {
      const address = await this.repository.addAddress(this.user.id, payload);
      await this.reload();
      return address;
    } catch (error) {
      const message =
        error instanceof Error ? error.message : 'Не удалось добавить адрес';
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

  async updateAddress(addressId: string, payload: UpdateAddressRequest): Promise<void> {
    if (!this.user) {
      throw new Error('Профиль не найден');
    }

    this.isLoading = true;
    this.error = null;

    try {
      await this.repository.updateAddress(this.user.id, addressId, payload);
      await this.reload();
    } catch (error) {
      const message =
        error instanceof Error ? error.message : 'Не удалось обновить адрес';
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

  async deleteAddress(addressId: string): Promise<void> {
    if (!this.user) {
      throw new Error('Профиль не найден');
    }

    this.isLoading = true;
    this.error = null;

    try {
      await this.repository.deleteAddress(this.user.id, addressId);
      await this.reload();
    } catch (error) {
      const message =
        error instanceof Error ? error.message : 'Не удалось удалить адрес';
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

  async setPrimaryAddress(addressId: string): Promise<void> {
    if (!this.user) {
      throw new Error('Профиль не найден');
    }

    try {
      await this.repository.setPrimaryAddress(this.user.id, addressId);
      await this.reload();
    } catch (error) {
      const message =
        error instanceof Error ? error.message : 'Не удалось обновить основной адрес';
      runInAction(() => {
        this.error = message;
      });
      throw error;
    }
  }
}


