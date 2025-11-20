import { createContext, useContext, useMemo, type ReactNode } from 'react';
import { AuthViewModel } from '@features/auth/model/authViewModel';
import { ProfileViewModel } from '@features/profile/model/profileViewModel';
import { UserRepository } from '@entities/user/model/userRepository';
import { OrderListViewModel } from '@features/orders/model/orderListViewModel';
import { CatalogViewModel } from '@features/catalog/model/catalogViewModel';
import { ProductRepository } from '@entities/product/model/productRepository';

class AppStore {
  readonly auth = new AuthViewModel();
  readonly profile = new ProfileViewModel(new UserRepository(), this.auth);
  readonly orders = new OrderListViewModel();
  readonly catalog = new CatalogViewModel(new ProductRepository());
}

const AppStoreContext = createContext<AppStore | null>(null);

export function AppStoreProvider({ children }: { children: ReactNode }) {
  const store = useMemo(() => new AppStore(), []);

  return (
    <AppStoreContext.Provider value={store}>
      {children}
    </AppStoreContext.Provider>
  );
}

function useAppStore(): AppStore {
  const store = useContext(AppStoreContext);
  if (!store) {
    throw new Error('AppStoreProvider is missing in component tree');
  }
  return store;
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuthStore(): AuthViewModel {
  return useAppStore().auth;
}

// eslint-disable-next-line react-refresh/only-export-components
export function useProfileStore(): ProfileViewModel {
  return useAppStore().profile;
}

// eslint-disable-next-line react-refresh/only-export-components
export function useOrdersStore(): OrderListViewModel {
  return useAppStore().orders;
}

// eslint-disable-next-line react-refresh/only-export-components
export function useCatalogStore(): CatalogViewModel {
  return useAppStore().catalog;
}


