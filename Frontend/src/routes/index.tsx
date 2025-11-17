import { createBrowserRouter, type RouteObject } from "react-router-dom";
import App from "../App";
import { userRoutes } from "./users";
import { wareHouseRoutes } from "./warehouse";
import { deliveryRoutes } from "./delivery";
import { authRoutes } from "./auth";


const routes: RouteObject[] = [
  {
    path: "/",
    element: <App />,
    children: [
      ...userRoutes,
      ...wareHouseRoutes,
      ...deliveryRoutes,
      ...authRoutes,
    ],
  },
];

export const router = createBrowserRouter(routes);
