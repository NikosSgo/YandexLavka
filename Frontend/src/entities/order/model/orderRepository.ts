import { orderApi } from '@entities/order/api/orderApi';
import type { Order } from '@entities/order/model/types';

export class OrderRepository {
  private readonly api;

  constructor(api = orderApi) {
    this.api = api;
  }

  getOrders(status?: string): Promise<Order[]> {
    return this.api.getOrders(status);
  }
}


