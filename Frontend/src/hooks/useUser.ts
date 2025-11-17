import { useState, useEffect } from 'react';
import { userService } from '../services/userService';
import { useAuth } from './useAuth';
import type { User, Address } from '../models/User';

export function useUser() {
  const { isAuthenticated, account } = useAuth();
  const [user, setUser] = useState<User | null>(null);
  const [addresses, setAddresses] = useState<Address[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadUser = async () => {
    if (!isAuthenticated || !account) {
      setUser(null);
      return;
    }

    setIsLoading(true);
    setError(null);

    try {
      // Пытаемся получить профиль по email из аккаунта
      const userData = await userService.getUserByEmail(account.email);
      setUser(userData);
      
      // Загружаем адреса
      if (userData.addresses && userData.addresses.length > 0) {
        setAddresses(userData.addresses);
      } else {
        // Если адреса не пришли с пользователем, загружаем отдельно
        try {
          const addressesData = await userService.getAddresses(userData.id);
          setAddresses(addressesData);
        } catch {
          // Адреса могут быть пустыми - это нормально
          setAddresses([]);
        }
      }
    } catch (err) {
      // Профиль может не существовать - это нормально
      if (err instanceof Error && !err.message.includes('not found')) {
        setError(err.message);
      }
      setUser(null);
      setAddresses([]);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    if (isAuthenticated) {
      loadUser();
    } else {
      setUser(null);
      setAddresses([]);
    }
  }, [isAuthenticated, account?.email]);

  const createProfile = async (data: {
    firstName: string;
    lastName: string;
    phone: string;
    email: string;
  }) => {
    setIsLoading(true);
    setError(null);

    try {
      const newUser = await userService.createProfile(data);
      setUser(newUser);
      return newUser;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Ошибка создания профиля';
      setError(errorMessage);
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const addAddress = async (data: {
    street: string;
    city: string;
    state: string;
    country: string;
    zipCode: string;
    description: string;
    isPrimary?: boolean;
  }) => {
    if (!user) throw new Error('Пользователь не найден');

    setIsLoading(true);
    setError(null);

    try {
      const newAddress = await userService.addAddress(user.id, data);
      // Перезагружаем данные пользователя для получения обновленного списка адресов
      await loadUser();
      return newAddress;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Ошибка добавления адреса';
      setError(errorMessage);
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const updateAddress = async (addressId: string, data: {
    street?: string;
    city?: string;
    state?: string;
    country?: string;
    zipCode?: string;
    description?: string;
  }) => {
    if (!user) throw new Error('Пользователь не найден');

    setIsLoading(true);
    setError(null);

    try {
      const updatedAddress = await userService.updateAddress(user.id, addressId, data);
      // Перезагружаем данные пользователя
      await loadUser();
      return updatedAddress;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Ошибка обновления адреса';
      setError(errorMessage);
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const deleteAddress = async (addressId: string) => {
    if (!user) throw new Error('Пользователь не найден');

    setIsLoading(true);
    setError(null);

    try {
      await userService.deleteAddress(user.id, addressId);
      // Перезагружаем данные пользователя
      await loadUser();
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Ошибка удаления адреса';
      setError(errorMessage);
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  return {
    user,
    addresses,
    isLoading,
    error,
    loadUser,
    createProfile,
    addAddress,
    updateAddress,
    deleteAddress,
  };
}

