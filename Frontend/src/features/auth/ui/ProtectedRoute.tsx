import { observer } from 'mobx-react-lite';
import { Navigate } from 'react-router-dom';
import { type ReactNode } from 'react';
import { useAuthStore } from '@app/providers/AppStoreProvider';
import { Loader } from '@shared/ui/Loader';

interface ProtectedRouteProps {
  children: ReactNode;
}

export const ProtectedRoute = observer(({ children }: ProtectedRouteProps) => {
  const authStore = useAuthStore();

  if (authStore.isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <Loader label="Проверяем авторизацию..." />
      </div>
    );
  }

  if (!authStore.isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
});


