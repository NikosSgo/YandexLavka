export interface StorageLocation {
  location: string;
  quantity: number;
  availableQuantity: number;
}

export interface WarehouseProduct {
  productId: string;
  productName: string;
  sku: string;
  quantity: number;
  reservedQuantity: number;
  availableQuantity: number;
  location: string;
  zone: string;
  lastRestocked: string;
  isLowStock: boolean;
  isOutOfStock: boolean;
  storageLocations: StorageLocation[];
}

