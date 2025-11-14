export const ClientType = {
  User: "User",
  WareHouseWorker: "WareHouseWorker",
  DeliveryMan: "DeliveryMan"
} as const;

export type ClientType = typeof ClientType[keyof typeof ClientType];
