import { apiClient } from './apiClient';
import type { Order, ProductStock, StockLevel, PickingTask } from '../models/Warehouse';

class WarehouseService {
  async getOrder(orderId: string): Promise<Order> {
    return await apiClient.get<Order>(`/api/orders/${orderId}`);
  }

  async getOrdersByStatus(status?: string): Promise<Order[]> {
    const url = status ? `/api/orders?status=${encodeURIComponent(status)}` : '/api/orders';
    return await apiClient.get<Order[]>(url);
  }

  async getProductStock(productId: string): Promise<ProductStock> {
    return await apiClient.get<ProductStock>(`/api/stock/products/${productId}`);
  }

  async getLowStockItems(): Promise<StockLevel[]> {
    return await apiClient.get<StockLevel[]>('/api/stock/low-stock');
  }

  async updateStock(
    productId: string,
    quantity: number,
    location: string,
    operation: string
  ): Promise<void> {
    await apiClient.put(`/api/stock/products/${productId}/stock`, {
      quantity,
      location,
      operation,
    });
  }

  async restockProduct(
    productId: string,
    quantity: number,
    location: string
  ): Promise<void> {
    await apiClient.post(`/api/stock/products/${productId}/restock`, {
      quantity,
      location,
    });
  }

  async startPicking(orderId: string, pickerId: string): Promise<PickingTask> {
    return await apiClient.post<PickingTask>(
      `/api/orders/${orderId}/start-picking`,
      { pickerId }
    );
  }

  async completePicking(
    orderId: string,
    pickedQuantities: Record<string, number>
  ): Promise<void> {
    await apiClient.post(`/api/orders/${orderId}/complete-picking`, {
      pickedQuantities,
    });
  }

  async cancelOrder(orderId: string, reason: string): Promise<void> {
    await apiClient.post(`/api/orders/${orderId}/cancel`, { reason });
  }
}

export const warehouseService = new WarehouseService();


