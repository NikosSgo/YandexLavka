import { userApi } from '@entities/user/api/userApi';
import type {
  Address,
  CreateAddressRequest,
  CreateUserRequest,
  UpdateAddressRequest,
  UpdateUserRequest,
  User,
} from '@entities/user/model/types';

export class UserRepository {
  private readonly api;

  constructor(api = userApi) {
    this.api = api;
  }

  getProfileByEmail(email: string): Promise<User> {
    return this.api.getUserByEmail(email);
  }

  getProfileById(userId: string): Promise<User> {
    return this.api.getUserById(userId);
  }

  createProfile(payload: CreateUserRequest): Promise<User> {
    return this.api.createProfile(payload);
  }

  updateProfile(userId: string, payload: UpdateUserRequest): Promise<User> {
    return this.api.updateProfile(userId, payload);
  }

  getAddresses(userId: string): Promise<Address[]> {
    return this.api.getAddresses(userId);
  }

  addAddress(userId: string, payload: CreateAddressRequest): Promise<Address> {
    return this.api.addAddress(userId, payload);
  }

  updateAddress(
    userId: string,
    addressId: string,
    payload: UpdateAddressRequest,
  ): Promise<Address> {
    return this.api.updateAddress(userId, addressId, payload);
  }

  deleteAddress(userId: string, addressId: string): Promise<void> {
    return this.api.deleteAddress(userId, addressId);
  }

  setPrimaryAddress(userId: string, addressId: string): Promise<void> {
    return this.api.setPrimaryAddress(userId, addressId);
  }
}


