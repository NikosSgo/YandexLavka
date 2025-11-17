import { useState } from 'react';
import { useOrders } from '../../../hooks/useOrders';
import type { Order } from '../../../models/Warehouse';

export default function OrdersPage() {
  const [statusFilter, setStatusFilter] = useState<string>('');
  const { orders, isLoading, error, reload } = useOrders(statusFilter || undefined);

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-6xl mx-auto px-4">
        <div className="mb-6 flex justify-between items-center">
          <h1 className="text-3xl font-bold text-gray-900">Мои заказы</h1>
          <div className="flex gap-2">
            <button
              onClick={() => setStatusFilter('')}
              className={`px-4 py-2 rounded ${statusFilter === '' ? 'bg-indigo-600 text-white' : 'bg-white border'}`}
            >
              Все
            </button>
            <button
              onClick={() => setStatusFilter('Created')}
              className={`px-4 py-2 rounded ${statusFilter === 'Created' ? 'bg-indigo-600 text-white' : 'bg-white border'}`}
            >
              Созданные
            </button>
            <button
              onClick={() => setStatusFilter('InProgress')}
              className={`px-4 py-2 rounded ${statusFilter === 'InProgress' ? 'bg-indigo-600 text-white' : 'bg-white border'}`}
            >
              В процессе
            </button>
            <button
              onClick={() => setStatusFilter('Completed')}
              className={`px-4 py-2 rounded ${statusFilter === 'Completed' ? 'bg-indigo-600 text-white' : 'bg-white border'}`}
            >
              Завершенные
            </button>
          </div>
        </div>

        {isLoading ? (
          <div className="text-center py-8">
            <div className="text-lg text-gray-500">Загрузка заказов...</div>
          </div>
        ) : error ? (
          <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
            Ошибка: {error}
          </div>
        ) : orders.length === 0 ? (
          <div className="bg-white rounded-lg shadow-md p-8 text-center">
            <p className="text-gray-500">Заказы не найдены</p>
          </div>
        ) : (
          <div className="space-y-4">
            {orders.map((order) => (
              <OrderCard key={order.orderId} order={order} />
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

function OrderCard({ order }: { order: Order }) {
  return (
    <div className="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow">
      <div className="flex justify-between items-start mb-4">
        <div>
          <h3 className="text-lg font-semibold">Заказ #{order.orderId.slice(0, 8)}</h3>
          <p className="text-sm text-gray-500">
            Создан: {new Date(order.createdAt).toLocaleString('ru-RU')}
          </p>
          {order.pickingStartedAt && (
            <p className="text-sm text-gray-500">
              Начат сбор: {new Date(order.pickingStartedAt).toLocaleString('ru-RU')}
            </p>
          )}
          {order.pickingCompletedAt && (
            <p className="text-sm text-gray-500">
              Завершен: {new Date(order.pickingCompletedAt).toLocaleString('ru-RU')}
            </p>
          )}
        </div>
        <div className="text-right">
          <span className={`inline-block px-3 py-1 text-sm font-semibold rounded ${order.status === 'Completed' ? 'bg-green-100 text-green-800' :
            order.status === 'InProgress' ? 'bg-blue-100 text-blue-800' :
              order.status === 'Picking' ? 'bg-yellow-100 text-yellow-800' :
                'bg-gray-100 text-gray-800'
            }`}>
            {order.status}
          </span>
          <div className="mt-2 text-xl font-bold">
            {order.totalAmount.toFixed(2)} ₽
          </div>
        </div>
      </div>

      <div className="border-t pt-4">
        <h4 className="font-semibold mb-2">Товары ({order.lines.length}):</h4>
        <div className="space-y-2">
          {order.lines.map((line) => (
            <div key={line.productId} className="flex justify-between text-sm">
              <div>
                <span className="font-medium">{line.productName}</span>
                <span className="text-gray-500 ml-2">({line.sku})</span>
              </div>
              <div className="text-right">
                <div>
                  {line.quantityOrdered} шт. × {line.unitPrice.toFixed(2)} ₽
                </div>
                {line.quantityPicked > 0 && (
                  <div className="text-xs text-gray-500">
                    Собрано: {line.quantityPicked} шт.
                  </div>
                )}
                <div className="font-semibold">
                  {line.totalPrice.toFixed(2)} ₽
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}


