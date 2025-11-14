import { createBrowserRouter, type RouteObject } from "react-router-dom";
import App from "../App";
import { userRoutes } from "./users";
import { wareHouseRoutes } from "./warehouse";
import { deliveryRoutes } from "./delivery";


const routes: RouteObject[] = [
  {
    path: "/",
    element: <App />,
    children: [
      ...userRoutes,
      ...wareHouseRoutes,
      ...deliveryRoutes
    ],
  },
];

export const router = createBrowserRouter(routes);
