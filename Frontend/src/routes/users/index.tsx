import type { RouteObject } from "react-router-dom";
import UserPage from "../../pages/users";

export const userRoutes: RouteObject[] = [
  {
    path: "/",
    element: <UserPage />,
    children: [
    ],
  },
];

