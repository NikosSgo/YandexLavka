import { useState, useEffect } from 'react';
import { authService } from '../services/authService';
import type { Account, RegisterRequest, LoginRequest } from '../models/Auth';

export function useAuth() {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [account, setAccount] = useState<Account | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    checkAuth();
  }, []);

  const checkAuth = () => {
    const authenticated = authService.isAuthenticated();
    const storedAccount = authService.getAccountFromStorage();
    
    setIsAuthenticated(authenticated);
    setAccount(storedAccount);
    setIsLoading(false);
  };

  const login = async (data: LoginRequest) => {
    try {
      const response = await authService.login(data);
      setAccount(response.account);
      setIsAuthenticated(true);
      return response;
    } catch (error) {
      throw error;
    }
  };

  const register = async (data: RegisterRequest) => {
    try {
      const response = await authService.register(data);
      setAccount(response.account);
      setIsAuthenticated(true);
      return response;
    } catch (error) {
      throw error;
    }
  };

  const logout = () => {
    authService.logout();
    setIsAuthenticated(false);
    setAccount(null);
  };

  return {
    isAuthenticated,
    account,
    isLoading,
    login,
    register,
    logout,
    checkAuth,
  };
}


