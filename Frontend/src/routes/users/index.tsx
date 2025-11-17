import type { RouteObject } from "react-router-dom";
import UserPage from "../../pages/users";
import Assortment from "../../pages/users/main";
import ProfilePage from "../../pages/users/main/ProfilePage";
import { ProtectedRoute } from "../../components/ProtectedRoute";
import CreateProfilePage from "../../pages/users/main/CreateProfilePage";
import OrdersPage from "../../pages/users/main/OrdersPage";
import AddressesPage from "../../pages/users/main/AddressesPage";

export const userRoutes: RouteObject[] = [
  {
    path: "/",
    element: <UserPage />,
    children: [
      {
        path: "/",
        element: (
          <Assortment />
        ),
      },
      {
        path: "/profile",
        element: (
          <ProtectedRoute>
            <ProfilePage />
          </ProtectedRoute>
        ),
      },
      {
        path: "/profile/create",
        element: (
          <ProtectedRoute>
            <CreateProfilePage />
          </ProtectedRoute>
        ),
      },
      {
        path: "/profile/addresses",
        element: (
          <ProtectedRoute>
            <AddressesPage />
          </ProtectedRoute>
        ),
      },
      {
        path: "/orders",
        element: (
          <ProtectedRoute>
            <OrdersPage />
          </ProtectedRoute>
        ),
      },

    ],
  },
];

