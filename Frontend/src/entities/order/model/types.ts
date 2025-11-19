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


