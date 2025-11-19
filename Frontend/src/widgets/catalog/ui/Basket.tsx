import { useEffect } from 'react';
import { observer } from 'mobx-react-lite';
import { useOrdersStore } from '@app/providers/AppStoreProvider';

interface BasketProps {
  className?: string;
}

export const Basket = observer(({ className }: BasketProps) => {
  const ordersStore = useOrdersStore();

  useEffect(() => {
    ordersStore.load();
  }, [ordersStore]);

  const combinedClassName = `gap-4 p-5 ${className ?? ''}`.trim();

  if (ordersStore.isLoading) {
    return (
      <div className={combinedClassName}>
        <div className="text-gray-500">Загрузка заказов...</div>
      </div>
    );
  }

  if (ordersStore.error) {
    return (
      <div className={combinedClassName}>
        <div className="text-red-600">Ошибка: {ordersStore.error}</div>
      </div>
    );
  }

  if (ordersStore.orders.length === 0) {
    return (
      <div className={combinedClassName}>
        <div className="text-gray-500">Корзина пуста</div>
      </div>
    );
  }

  return (
    <div className={combinedClassName}>
      <div className="text-3xl font-bold mb-4">Корзина</div>
      <div className="space-y-4">
        {ordersStore.orders.map((order) => (
          <OrderCard
            key={order.orderId}
            orderId={order.orderId}
            status={order.status}
            createdAt={order.createdAt}
            lines={order.lines.length}
            amount={order.totalAmount}
          />
        ))}
      </div>
    </div>
  );
});

interface OrderCardProps {
  orderId: string;
  status: string;
  createdAt: string;
  lines: number;
  amount: number;
}

function OrderCard({ orderId, status, createdAt, lines, amount }: OrderCardProps) {
  const badgeClass =
    status === 'Completed'
      ? 'bg-green-100 text-green-800'
      : status === 'InProgress'
        ? 'bg-blue-100 text-blue-800'
        : 'bg-gray-100 text-gray-800';

  return (
    <div className="border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow">
      <div className="flex justify-between items-start mb-2">
        <div>
          <div className="font-semibold">Заказ #{orderId.slice(0, 8)}</div>
          <div className="text-sm text-gray-500">
            {new Date(createdAt).toLocaleDateString('ru-RU')}
          </div>
        </div>
        <span className={`px-2 py-1 text-xs font-semibold rounded ${badgeClass}`}>
          {status}
        </span>
      </div>
      <div className="text-sm text-gray-600 mb-2">Товаров: {lines}</div>
      <div className="text-lg font-bold">{amount.toFixed(2)} ₽</div>
    </div>
  );
}


