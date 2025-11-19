import { makeAutoObservable, runInAction } from 'mobx';
import type { Order } from '@entities/order/model/types';
import { OrderRepository } from '@entities/order/model/orderRepository';

export class OrderListViewModel {
  orders: Order[] = [];
  isLoading = false;
  error: string | null = null;
  private readonly repository: OrderRepository;

  constructor(repository = new OrderRepository()) {
    this.repository = repository;
    makeAutoObservable(this, {}, { autoBind: true });
  }

  async load(status?: string): Promise<void> {
    this.isLoading = true;
    this.error = null;

    try {
      const orders = await this.repository.getOrders(status);
      runInAction(() => {
        this.orders = orders;
      });
    } catch (error) {
      const message =
        error instanceof Error ? error.message : 'Не удалось загрузить заказы';
      runInAction(() => {
        this.error = message;
        this.orders = [];
      });
    } finally {
      runInAction(() => {
        this.isLoading = false;
      });
    }
  }
}


