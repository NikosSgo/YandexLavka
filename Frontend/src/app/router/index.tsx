import { createBrowserRouter } from 'react-router-dom';
import { UserLayout } from '@widgets/layout/user-layout';
import { HomePage } from '@pages/home';
import { LoginPage } from '@pages/auth/login';
import { RegisterPage } from '@pages/auth/register';
import { ProfilePage } from '@pages/profile/view';
import { CreateProfilePage } from '@pages/profile/create';
import { AddressesPage } from '@pages/profile/addresses';
import { OrdersPage } from '@pages/orders/list';
import { DeliveryPage } from '@pages/delivery';
import { WarehousePage } from '@pages/warehouse';
import { ProtectedRoute } from '@features/auth/ui/ProtectedRoute';

export const appRouter = createBrowserRouter([
  {
    path: '/',
    element: <UserLayout />,
    children: [
      {
        index: true,
        element: <HomePage />,
      },
      {
        path: 'orders',
        element: (
          <ProtectedRoute>
            <OrdersPage />
          </ProtectedRoute>
        ),
      },
      {
        path: 'delivery',
        element: <DeliveryPage />,
      },
      {
        path: 'warehouse',
        element: <WarehousePage />,
      },
      {
        path: 'profile',
        children: [
          {
            index: true,
            element: (
              <ProtectedRoute>
                <ProfilePage />
              </ProtectedRoute>
            ),
          },
          {
            path: 'create',
            element: (
              <ProtectedRoute>
                <CreateProfilePage />
              </ProtectedRoute>
            ),
          },
          {
            path: 'addresses',
            element: (
              <ProtectedRoute>
                <AddressesPage />
              </ProtectedRoute>
            ),
          },
        ],
      },
    ],
  },
  {
    path: '/login',
    element: <LoginPage />,
  },
  {
    path: '/register',
    element: <RegisterPage />,
  },
]);


