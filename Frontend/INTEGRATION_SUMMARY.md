# Интеграция UserService и WarehouseService во фронтенде

## Что реализовано

### Модели данных
- **`models/User.ts`** - интерфейсы для пользователей и адресов
- **`models/Warehouse.ts`** - интерфейсы для заказов, товаров, склада

### Сервисы
- **`services/userService.ts`** - работа с UserService API
  - Получение профиля по email
  - Создание/обновление профиля
  - Управление адресами (CRUD)
  - Установка основного адреса

- **`services/warehouseService.ts`** - работа с WarehouseService API
  - Получение заказов
  - Получение информации о товарах и складе
  - Управление заказами

### React хуки
- **`hooks/useUser.ts`** - управление данными пользователя
  - Автоматическая загрузка профиля по email из аккаунта
  - Управление адресами
  - Создание профиля

- **`hooks/useOrders.ts`** - управление заказами
  - Загрузка заказов с фильтрацией по статусу

### Страницы
- **`pages/users/ProfilePage.tsx`** - просмотр профиля
  - Информация об аккаунте
  - Информация о пользователе
  - Список адресов

- **`pages/users/CreateProfilePage.tsx`** - создание профиля пользователя

- **`pages/users/AddressesPage.tsx`** - управление адресами
  - Просмотр всех адресов
  - Добавление нового адреса
  - Редактирование адреса
  - Удаление адреса
  - Установка основного адреса

- **`pages/users/OrdersPage.tsx`** - просмотр заказов
  - Список всех заказов
  - Фильтрация по статусу
  - Детальная информация о заказе

### Обновленные компоненты
- **`pages/users/basket/index.tsx`** - корзина с отображением заказов
- **`pages/users/header/index.tsx`** - добавлены ссылки на адреса и заказы

## Маршруты

- `/profile` - профиль пользователя
- `/profile/create` - создание профиля
- `/profile/addresses` - управление адресами
- `/orders` - список заказов

## Использование

### Получение данных пользователя

```typescript
import { useUser } from '../hooks/useUser';

function MyComponent() {
  const { user, addresses, isLoading, createProfile } = useUser();

  if (isLoading) return <div>Загрузка...</div>;
  if (!user) return <div>Профиль не создан</div>;

  return <div>Привет, {user.firstName}!</div>;
}
```

### Работа с адресами

```typescript
const { addAddress, updateAddress, deleteAddress } = useUser();

// Добавить адрес
await addAddress({
  street: 'ул. Ленина, 1',
  city: 'Москва',
  state: 'Московская область',
  country: 'Россия',
  zipCode: '123456',
  description: 'Дом',
  isPrimary: true
});
```

### Работа с заказами

```typescript
import { useOrders } from '../hooks/useOrders';

function OrdersList() {
  const { orders, isLoading } = useOrders('Completed');

  return (
    <div>
      {orders.map(order => (
        <div key={order.orderId}>
          Заказ #{order.orderId} - {order.totalAmount} ₽
        </div>
      ))}
    </div>
  );
}
```

## Особенности интеграции

### Автоматическая загрузка профиля

Хук `useUser` автоматически пытается загрузить профиль пользователя по email из аккаунта AuthService. Если профиль не найден, это нормально - пользователь может создать его позже.

### Связь с AuthService

Профиль пользователя привязывается к аккаунту через email. При создании профиля используется email из JWT токена.

### Обработка ошибок

Все сервисы обрабатывают ошибки и показывают понятные сообщения пользователю.

## API Endpoints используемые

### UserService
- `GET /api/users/by-email/{email}` - получить профиль по email
- `POST /api/users` - создать профиль
- `PUT /api/users/{id}` - обновить профиль
- `GET /api/users/{userId}/addresses` - получить адреса
- `POST /api/users/{userId}/addresses` - добавить адрес
- `PUT /api/users/{userId}/addresses/{addressId}` - обновить адрес
- `DELETE /api/users/{userId}/addresses/{addressId}` - удалить адрес
- `PATCH /api/users/{userId}/addresses/{addressId}/set-primary` - установить основной адрес

### WarehouseService
- `GET /api/orders` - получить заказы (с фильтром по статусу)
- `GET /api/orders/{orderId}` - получить заказ по ID
- `GET /api/stock/products/{productId}` - получить информацию о товаре
- `GET /api/stock/low-stock` - получить товары с низким запасом

## Следующие шаги

Для полной интеграции можно добавить:
1. Страницу создания заказа
2. Страницу просмотра деталей заказа
3. Страницу управления товарами (для склада)
4. Страницу управления заказами (для склада)






