import { httpClient } from '@shared/api/httpClient';
import type { Order } from '@entities/order/model/types';

class OrderApi {
  async getOrders(status?: string): Promise<Order[]> {
    const endpoint = status ? `/api/orders?status=${encodeURIComponent(status)}` : '/api/orders';
    const { data } = await httpClient.get<Order[]>(endpoint);
    return data;
  }
}

export const orderApi = new OrderApi();


