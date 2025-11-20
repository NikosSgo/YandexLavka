import { makeAutoObservable, runInAction } from 'mobx';
import type { WarehouseProduct } from '@entities/product/model/types';
import { ProductRepository } from '@entities/product/model/productRepository';

export class CatalogViewModel {
  products: WarehouseProduct[] = [];
  isLoading = false;
  error: string | null = null;
  lastUpdated: string | null = null;
  private readonly repository: ProductRepository;

  constructor(repository: ProductRepository) {
    this.repository = repository;
    makeAutoObservable(this, {}, { autoBind: true });
  }

  get inStockCount(): number {
    return this.products.filter((product) => !product.isOutOfStock).length;
  }

  get lowStockCount(): number {
    return this.products.filter((product) => product.isLowStock && !product.isOutOfStock).length;
  }

  get outOfStockCount(): number {
    return this.products.filter((product) => product.isOutOfStock).length;
  }

  get summary() {
    return {
      total: this.products.length,
      inStock: this.inStockCount,
      lowStock: this.lowStockCount,
      outOfStock: this.outOfStockCount,
    };
  }

  async loadProducts(): Promise<void> {
    this.isLoading = true;
    this.error = null;

    try {
      const products = await this.repository.getCatalogProducts();
      runInAction(() => {
        this.products = products;
        this.lastUpdated = new Date().toISOString();
      });
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Не удалось загрузить товары';
      runInAction(() => {
        this.error = message;
        this.products = [];
      });
    } finally {
      runInAction(() => {
        this.isLoading = false;
      });
    }
  }
}

