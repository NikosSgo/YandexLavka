export interface Address {
  id: string;
  userId: string;
  street: string;
  city: string;
  state: string;
  country: string;
  zipCode: string;
  description: string;
  isPrimary: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface User {
  id: string;
  firstName: string;
  lastName: string;
  phone: string;
  email: string;
  createdAt: string;
  updatedAt?: string;
  addresses: Address[];
}

export interface CreateUserRequest {
  firstName: string;
  lastName: string;
  phone: string;
  email: string;
}

export interface UpdateUserRequest {
  firstName?: string;
  lastName?: string;
  phone?: string;
  email?: string;
}

export interface CreateAddressRequest {
  street: string;
  city: string;
  state: string;
  country: string;
  zipCode: string;
  description: string;
  isPrimary?: boolean;
}

export interface UpdateAddressRequest {
  street?: string;
  city?: string;
  state?: string;
  country?: string;
  zipCode?: string;
  description?: string;
  isPrimary?: boolean;
}


