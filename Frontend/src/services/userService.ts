import { apiClient } from './apiClient';
import type { User, Address, CreateUserRequest, UpdateUserRequest, CreateAddressRequest, UpdateAddressRequest } from '../models/User';

class UserService {
  // Получить профиль по email (используется для получения своего профиля)
  async getMyProfile(email: string): Promise<User> {
    return await apiClient.get<User>(`/api/users/by-email/${encodeURIComponent(email)}`);
  }

  async getUserById(userId: string): Promise<User> {
    return await apiClient.get<User>(`/api/users/${userId}`);
  }

  async getUserByEmail(email: string): Promise<User> {
    return await apiClient.get<User>(`/api/users/by-email/${encodeURIComponent(email)}`);
  }

  async createProfile(data: CreateUserRequest): Promise<User> {
    return await apiClient.post<User>('/api/users', data);
  }

  async updateProfile(userId: string, data: UpdateUserRequest): Promise<User> {
    return await apiClient.put<User>(`/api/users/${userId}`, data);
  }

  async getAddresses(userId: string): Promise<Address[]> {
    return await apiClient.get<Address[]>(`/api/users/${userId}/addresses`);
  }

  async getAddressById(userId: string, addressId: string): Promise<Address> {
    return await apiClient.get<Address>(`/api/users/${userId}/addresses/${addressId}`);
  }

  async getPrimaryAddress(userId: string): Promise<Address | null> {
    try {
      return await apiClient.get<Address>(`/api/users/${userId}/addresses/primary`);
    } catch {
      return null;
    }
  }

  async addAddress(userId: string, data: CreateAddressRequest): Promise<Address> {
    return await apiClient.post<Address>(`/api/users/${userId}/addresses`, data);
  }

  async updateAddress(userId: string, addressId: string, data: UpdateAddressRequest): Promise<Address> {
    return await apiClient.put<Address>(`/api/users/${userId}/addresses/${addressId}`, data);
  }

  async deleteAddress(userId: string, addressId: string): Promise<void> {
    await apiClient.delete(`/api/users/${userId}/addresses/${addressId}`);
  }

  async setPrimaryAddress(userId: string, addressId: string): Promise<void> {
    await apiClient.patch(`/api/users/${userId}/addresses/${addressId}/set-primary`, {});
  }
}

export const userService = new UserService();

