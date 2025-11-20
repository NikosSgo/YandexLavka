import { useEffect, useMemo, useState } from 'react';
import { observer } from 'mobx-react-lite';
import { useCatalogStore } from '@app/providers/AppStoreProvider';
import { Loader } from '@shared/ui/Loader';
import { ErrorState } from '@shared/ui/ErrorState';
import type { WarehouseProduct } from '@entities/product/model/types';

interface ProductGridProps {
  className?: string;
}

type Filter = 'all' | 'low' | 'out';

const FILTER_LABELS: Record<Filter, string> = {
  all: 'Все',
  low: 'Мало',
  out: 'Нет в наличии',
};

export const ProductGrid = observer(({ className }: ProductGridProps) => {
  const catalogStore = useCatalogStore();
  const [filter, setFilter] = useState<Filter>('all');

  useEffect(() => {
    catalogStore.loadProducts();
  }, [catalogStore]);

  const combinedClassName = `p-6 ${className ?? ''}`.trim();
  const filteredProducts = useMemo(() => filterProducts(catalogStore.products, filter), [
    catalogStore.products,
    filter,
  ]);

  if (catalogStore.isLoading && catalogStore.products.length === 0) {
    return (
      <section className={combinedClassName}>
        <Loader label="Загружаем товары склада..." />
      </section>
    );
  }

  if (catalogStore.error) {
    return (
      <section className={combinedClassName}>
        <ErrorState message={catalogStore.error} />
        <button
          type="button"
          onClick={() => catalogStore.loadProducts()}
          className="mt-4 px-4 py-2 bg-indigo-600 text-white rounded hover:bg-indigo-700"
        >
          Повторить попытку
        </button>
      </section>
    );
  }

  if (catalogStore.products.length === 0) {
    return (
      <section className={combinedClassName}>
        <p className="text-gray-500">Склад пока не вернул товары</p>
        <button
          type="button"
          onClick={() => catalogStore.loadProducts()}
          className="mt-4 px-4 py-2 border border-zinc-300 rounded hover:bg-zinc-50"
        >
          Обновить
        </button>
      </section>
    );
  }

  const summary = catalogStore.summary;

  return (
    <section className={combinedClassName}>
      <header className="flex flex-col gap-2 md:flex-row md:items-start md:justify-between">
        <div>
          <h2 className="text-2xl font-semibold">Каталог склада</h2>
          <p className="text-sm text-gray-500">Данные напрямую из Warehouse Service</p>
        </div>
        <button
          type="button"
          onClick={() => catalogStore.loadProducts()}
          className="px-4 py-2 border border-zinc-300 rounded hover:bg-zinc-50 text-sm font-medium"
          disabled={catalogStore.isLoading}
        >
          {catalogStore.isLoading ? 'Обновляем...' : 'Обновить данные'}
        </button>
      </header>

      <StatsBar summary={summary} />

      <div className="mt-4 flex flex-wrap gap-2">
        {(Object.keys(FILTER_LABELS) as Filter[]).map((value) => (
          <button
            key={value}
            type="button"
            onClick={() => setFilter(value)}
            className={`px-3 py-1.5 text-sm rounded-full border transition-colors ${
              filter === value
                ? 'bg-indigo-600 text-white border-indigo-600'
                : 'border-zinc-300 text-zinc-700 hover:bg-zinc-50'
            }`}
          >
            {FILTER_LABELS[value]}
          </button>
        ))}
      </div>

      <div className="mt-2 text-xs text-gray-500">
        Обновлено:{' '}
        {catalogStore.lastUpdated
          ? new Date(catalogStore.lastUpdated).toLocaleString('ru-RU')
          : '—'}
      </div>

      {catalogStore.isLoading && (
        <div className="mt-4">
          <Loader label="Синхронизируем остатки..." />
        </div>
      )}

      {filteredProducts.length === 0 ? (
        <div className="mt-6 text-gray-500 text-sm">
          По выбранному фильтру товары не найдены
        </div>
      ) : (
        <div className="mt-6 grid grid-cols-1 md:grid-cols-2 gap-4">
          {filteredProducts.map((product) => (
            <ProductCard key={product.productId} product={product} />
          ))}
        </div>
      )}
    </section>
  );
});

function StatsBar({
  summary,
}: {
  summary: { total: number; inStock: number; lowStock: number; outOfStock: number };
}) {
  const stats = [
    { label: 'Всего SKU', value: summary.total },
    { label: 'В наличии', value: summary.inStock },
    { label: 'Мало на складе', value: summary.lowStock },
    { label: 'Нет на складе', value: summary.outOfStock },
  ];

  return (
    <div className="mt-4 grid grid-cols-2 gap-3 text-sm text-gray-600 md:grid-cols-4">
      {stats.map((item) => (
        <div key={item.label} className="rounded-lg border border-zinc-200 p-3">
          <div className="text-xs uppercase tracking-wide text-gray-400">{item.label}</div>
          <div className="text-xl font-semibold text-gray-900">{item.value}</div>
        </div>
      ))}
    </div>
  );
}

function ProductCard({ product }: { product: WarehouseProduct }) {
  return (
    <article className="border border-zinc-200 rounded-lg p-4 shadow-sm hover:shadow-md transition-shadow">
      <div className="flex items-start justify-between gap-3 mb-2">
        <div>
          <h3 className="text-lg font-semibold text-gray-900">{product.productName}</h3>
          <p className="text-sm text-gray-500">SKU: {product.sku}</p>
        </div>
        <StockBadge isLowStock={product.isLowStock} isOutOfStock={product.isOutOfStock} />
      </div>
      <dl className="grid grid-cols-2 gap-3 text-sm">
        <Stat label="Доступно" value={product.availableQuantity} accent="text-emerald-600" />
        <Stat label="На складе" value={product.quantity} />
        <Stat label="Забронировано" value={product.reservedQuantity} accent="text-amber-600" />
        <div>
          <dt className="text-gray-500">Локация</dt>
          <dd className="font-medium text-gray-900">
            {product.location} · зона {product.zone}
          </dd>
        </div>
      </dl>
      {product.storageLocations.length > 0 && (
        <div className="mt-3">
          <div className="text-xs font-semibold text-gray-500 uppercase mb-1">Ячейки</div>
          <ul className="text-xs text-gray-600 space-y-1">
            {product.storageLocations.map((location) => (
              <li key={`${product.productId}-${location.location}`}>
                {location.location} · {location.availableQuantity} из {location.quantity} шт.
              </li>
            ))}
          </ul>
        </div>
      )}
      <div className="mt-3 text-xs text-gray-400">
        Последнее пополнение: {new Date(product.lastRestocked).toLocaleString('ru-RU')}
      </div>
    </article>
  );
}

function Stat({ label, value, accent }: { label: string; value: number; accent?: string }) {
  return (
    <div>
      <dt className="text-gray-500">{label}</dt>
      <dd className={`text-xl font-bold ${accent ?? 'text-gray-900'}`}>{value}</dd>
    </div>
  );
}

function StockBadge({ isLowStock, isOutOfStock }: { isLowStock: boolean; isOutOfStock: boolean }) {
  if (isOutOfStock) {
    return (
      <span className="px-2 py-1 text-xs font-semibold rounded bg-red-100 text-red-700">
        Нет на складе
      </span>
    );
  }

  if (isLowStock) {
    return (
      <span className="px-2 py-1 text-xs font-semibold rounded bg-amber-100 text-amber-700">
        Мало на складе
      </span>
    );
  }

  return (
    <span className="px-2 py-1 text-xs font-semibold rounded bg-emerald-100 text-emerald-700">
      В наличии
    </span>
  );
}

function filterProducts(products: WarehouseProduct[], filter: Filter): WarehouseProduct[] {
  if (filter === 'low') {
    return products.filter((product) => product.isLowStock && !product.isOutOfStock);
  }

  if (filter === 'out') {
    return products.filter((product) => product.isOutOfStock);
  }

  return products;
}

