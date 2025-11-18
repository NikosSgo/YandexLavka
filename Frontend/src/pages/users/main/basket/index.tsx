import { useOrders } from "../../../../hooks/useOrders";
import type { Order } from "../../../../models/Warehouse";

export default function Basket({ className }: { className?: string }) {
  const { orders, isLoading, error } = useOrders();
  const combinedClassName = `gap-4 p-5 ${className || ''}`.trim();

  return (
    <div className={combinedClassName}>
      <div className="text-3xl font-bold mb-4">Корзина</div>

      {isLoading ? (
        <div className="text-gray-500">Загрузка заказов...</div>
      ) : error ? (
        <div className="text-red-600">Ошибка: {error}</div>
      ) : orders.length === 0 ? (
        <div className="text-gray-500">Корзина пуста</div>
      ) : (
        <div className="space-y-4">
          {orders.map((order) => (
            <OrderCard key={order.orderId} order={order} />
          ))}
        </div>
      )}
    </div>
  );
}

function OrderCard({ order }: { order: Order }) {
  return (
    <div className="border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow">
      <div className="flex justify-between items-start mb-2">
        <div>
          <div className="font-semibold">Заказ #{order.orderId.slice(0, 8)}</div>
          <div className="text-sm text-gray-500">
            {new Date(order.createdAt).toLocaleDateString('ru-RU')}
          </div>
        </div>
        <span className={`px-2 py-1 text-xs font-semibold rounded ${order.status === 'Completed' ? 'bg-green-100 text-green-800' :
            order.status === 'InProgress' ? 'bg-blue-100 text-blue-800' :
              'bg-gray-100 text-gray-800'
          }`}>
          {order.status}
        </span>
      </div>

      <div className="text-sm text-gray-600 mb-2">
        Товаров: {order.lines.length}
      </div>

      <div className="text-lg font-bold">
        {order.totalAmount.toFixed(2)} ₽
      </div>
    </div>
  );
}
