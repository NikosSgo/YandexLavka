import type { RouteObject } from "react-router-dom";
import DeliveryPage from "../../pages/delivery";

export const deliveryRoutes: RouteObject[] = [
  {
    path: "/delivery",
    element: <DeliveryPage />,
    children: [
    ],
  },
];


