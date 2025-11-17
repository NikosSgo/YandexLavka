import { useState, useEffect } from 'react';
import { warehouseService } from '../services/warehouseService';
import type { Order } from '../models/Warehouse';

export function useOrders(status?: string) {
  const [orders, setOrders] = useState<Order[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadOrders = async () => {
    setIsLoading(true);
    setError(null);

    try {
      const ordersData = await warehouseService.getOrdersByStatus(status);
      setOrders(ordersData);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Ошибка загрузки заказов');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    loadOrders();
  }, [status]);

  return {
    orders,
    isLoading,
    error,
    reload: loadOrders,
  };
}


