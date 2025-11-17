export interface Order {
  orderId: string;
  customerId: string;
  status: string;
  createdAt: string;
  pickingStartedAt?: string;
  pickingCompletedAt?: string;
  lines: OrderLine[];
  totalAmount: number;
}

export interface OrderLine {
  productId: string;
  productName: string;
  sku: string;
  quantityOrdered: number;
  quantityPicked: number;
  unitPrice: number;
  totalPrice: number;
  isFullyPicked: boolean;
}

export interface ProductStock {
  productId: string;
  productName: string;
  sku: string;
  totalQuantity: number;
  availableQuantity: number;
  reservedQuantity: number;
  locations: string[];
  isLowStock: boolean;
  isOutOfStock: boolean;
  storageLocations: StorageLocation[];
}

export interface StockLevel {
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

export interface StorageLocation {
  location: string;
  zone: string;
  quantity: number;
  reservedQuantity: number;
  availableQuantity: number;
}

export interface PickingTask {
  taskId: string;
  orderId: string;
  assignedPicker: string;
  status: string;
  zone: string;
  zones: string[];
  createdAt: string;
  completedAt?: string;
  progress: number;
  items: PickingItem[];
}

export interface PickingItem {
  productId: string;
  productName: string;
  sku: string;
  quantity: number;
  quantityPicked: number;
  storageLocation: string;
  barcode: string;
  isPicked: boolean;
}


