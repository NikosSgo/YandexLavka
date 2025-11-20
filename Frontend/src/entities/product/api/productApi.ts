import { httpClient } from '@shared/api/httpClient';
import type { WarehouseProduct } from '@entities/product/model/types';

class ProductApi {
  async getCatalogProducts(): Promise<WarehouseProduct[]> {
    const { data } = await httpClient.get<WarehouseProduct[]>('/api/stock/products');
    return data;
  }
}

export const productApi = new ProductApi();

