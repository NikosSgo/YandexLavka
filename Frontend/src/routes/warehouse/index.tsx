import type { RouteObject } from "react-router-dom";
import WareHousePage from "../../pages/warehouse";

export const wareHouseRoutes: RouteObject[] = [
  {
    path: "/warehouse",
    element: <WareHousePage />,
    children: [
    ],
  },
];


