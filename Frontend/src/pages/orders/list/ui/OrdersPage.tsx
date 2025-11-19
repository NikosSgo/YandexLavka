import { useEffect, useState } from 'react';
import { observer } from 'mobx-react-lite';
import { useOrdersStore } from '@app/providers/AppStoreProvider';
import { Loader } from '@shared/ui/Loader';
import { ErrorState } from '@shared/ui/ErrorState';

const STATUSES = [
  { label: 'Все', value: undefined },
  { label: 'InProgress', value: 'InProgress' },
  { label: 'Completed', value: 'Completed' },
  { label: 'Cancelled', value: 'Cancelled' },
];

export const OrdersPage = observer(() => {
  const ordersStore = useOrdersStore();
  const [statusFilter, setStatusFilter] = useState<string | undefined>(undefined);

  useEffect(() => {
    ordersStore.load(statusFilter);
  }, [ordersStore, statusFilter]);

  return (
    <div className="max-w-4xl mx-auto py-8 px-4 space-y-6">
      <header className="flex justify-between items-center">
        <h1 className="text-2xl font-bold">Мои заказы</h1>
        <div className="flex gap-2">
          {STATUSES.map((status) => (
            <button
              key={status.label}
              onClick={() => setStatusFilter(status.value)}
              className={`px-3 py-1 rounded text-sm ${
                statusFilter === status.value
                  ? 'bg-indigo-600 text-white'
                  : 'bg-gray-200 text-gray-700'
              }`}
            >
              {status.label}
            </button>
          ))}
        </div>
      </header>
      {ordersStore.isLoading && <Loader label="Загружаем заказы..." />}
      {ordersStore.error && <ErrorState message={ordersStore.error} />}
      {!ordersStore.isLoading && ordersStore.orders.length === 0 && (
        <p className="text-gray-500">Заказы отсутствуют</p>
      )}
      <div className="space-y-4">
        {ordersStore.orders.map((order) => (
          <div
            key={order.orderId}
            className="border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow"
          >
            <div className="flex justify-between items-start mb-2">
              <div>
                <div className="font-semibold">Заказ #{order.orderId.slice(0, 8)}</div>
                <div className="text-sm text-gray-500">
                  {new Date(order.createdAt).toLocaleDateString('ru-RU')}
                </div>
              </div>
              <span className="px-2 py-1 text-xs font-semibold rounded bg-gray-100 text-gray-800">
                {order.status}
              </span>
            </div>
            <div className="text-sm text-gray-600 mb-2">Товаров: {order.lines.length}</div>
            <div className="text-lg font-bold">{order.totalAmount.toFixed(2)} ₽</div>
          </div>
        ))}
      </div>
    </div>
  );
});


