import axios from 'axios';
import { httpClient } from '@shared/api/httpClient';
import type {
  Address,
  CreateAddressRequest,
  CreateUserRequest,
  UpdateAddressRequest,
  UpdateUserRequest,
  User,
} from '@entities/user/model/types';

class UserApi {
  getMyProfile(email: string): Promise<User> {
    return this.getUserByEmail(email);
  }

  async getUserById(userId: string): Promise<User> {
    const { data } = await httpClient.get<User>(`/api/users/${userId}`);
    return data;
  }

  async getUserByEmail(email: string): Promise<User> {
    try {
      const { data } = await httpClient.get<User>(
        `/api/users/by-email/${encodeURIComponent(email)}`,
      );
      return data;
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 404) {
        throw new Error('Профиль не найден');
      }
      throw error;
    }
  }

  async createProfile(payload: CreateUserRequest): Promise<User> {
    const { data } = await httpClient.post<User>('/api/users', payload);
    return data;
  }

  async updateProfile(userId: string, payload: UpdateUserRequest): Promise<User> {
    const { data } = await httpClient.put<User>(`/api/users/${userId}`, payload);
    return data;
  }

  async getAddresses(userId: string): Promise<Address[]> {
    const { data } = await httpClient.get<Address[]>(`/api/users/${userId}/addresses`);
    return data;
  }

  async addAddress(userId: string, payload: CreateAddressRequest): Promise<Address> {
    const { data } = await httpClient.post<Address>(
      `/api/users/${userId}/addresses`,
      payload,
    );
    return data;
  }

  async updateAddress(
    userId: string,
    addressId: string,
    payload: UpdateAddressRequest,
  ): Promise<Address> {
    const { data } = await httpClient.put<Address>(
      `/api/users/${userId}/addresses/${addressId}`,
      payload,
    );
    return data;
  }

  async deleteAddress(userId: string, addressId: string): Promise<void> {
    await httpClient.delete(`/api/users/${userId}/addresses/${addressId}`);
  }

  async setPrimaryAddress(userId: string, addressId: string): Promise<void> {
    await httpClient.patch(`/api/users/${userId}/addresses/${addressId}/set-primary`, {});
  }
}

export const userApi = new UserApi();


