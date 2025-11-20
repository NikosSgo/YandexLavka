import { productApi } from '@entities/product/api/productApi';
import type { WarehouseProduct } from '@entities/product/model/types';

export class ProductRepository {
  private readonly api;

  constructor(api = productApi) {
    this.api = api;
  }

  getCatalogProducts(): Promise<WarehouseProduct[]> {
    return this.api.getCatalogProducts();
  }
}

