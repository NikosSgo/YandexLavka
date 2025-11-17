# Интеграция аутентификации во фронтенде

## Что реализовано

### 1. Сервисы
- **`apiClient.ts`** - HTTP клиент с автоматической добавкой JWT токенов
- **`authService.ts`** - Сервис для работы с аутентификацией (регистрация, вход, выход)

### 2. Модели
- **`Auth.ts`** - TypeScript интерфейсы для аутентификации

### 3. Хуки
- **`useAuth.ts`** - React хук для управления состоянием аутентификации

### 4. Компоненты
- **`ProtectedRoute.tsx`** - Компонент для защиты маршрутов

### 5. Страницы
- **`LoginPage.tsx`** - Страница входа
- **`RegisterPage.tsx`** - Страница регистрации
- **`ProfilePage.tsx`** - Страница профиля пользователя

### 6. Обновления
- **Header** - Отображает статус пользователя и кнопки входа/выхода

## Использование

### Автоматическая аутентификация

Все запросы через `apiClient` автоматически включают JWT токен:

```typescript
import { apiClient } from './services/apiClient';

// Токен автоматически добавляется в заголовок Authorization
const data = await apiClient.get('/api/users/me');
```

### Использование хука useAuth

```typescript
import { useAuth } from './hooks/useAuth';

function MyComponent() {
  const { isAuthenticated, account, login, logout } = useAuth();

  if (isAuthenticated) {
    return <div>Привет, {account?.email}!</div>;
  }

  return <div>Пожалуйста, войдите</div>;
}
```

### Защита маршрутов

```typescript
import { ProtectedRoute } from './components/ProtectedRoute';

<Route
  path="/profile"
  element={
    <ProtectedRoute>
      <ProfilePage />
    </ProtectedRoute>
  }
/>
```

## Маршруты

- `/login` - Страница входа
- `/register` - Страница регистрации
- `/profile` - Профиль пользователя (требует аутентификации)

## Хранение токенов

Токены хранятся в `localStorage`:
- `auth_token` - JWT токен доступа
- `refresh_token` - Refresh токен (для будущей реализации)
- `account` - Данные аккаунта

## Настройка API URL

Создайте файл `.env` в корне проекта:

```env
VITE_API_URL=http://localhost:8080
```

Или используйте значение по умолчанию (http://localhost:8080).

## Примеры использования

### Создание защищенного API запроса

```typescript
import { apiClient } from '../services/apiClient';

async function getUserProfile() {
  try {
    const profile = await apiClient.get('/api/users/me');
    return profile;
  } catch (error) {
    console.error('Ошибка загрузки профиля:', error);
    throw error;
  }
}
```

### Проверка аутентификации в компоненте

```typescript
import { useAuth } from '../hooks/useAuth';

function MyComponent() {
  const { isAuthenticated, account, isLoading } = useAuth();

  if (isLoading) {
    return <div>Загрузка...</div>;
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" />;
  }

  return <div>Контент для аутентифицированных пользователей</div>;
}
```

## Обработка ошибок

При ошибках аутентификации (401, 403) можно автоматически перенаправлять на страницу входа:

```typescript
// В apiClient можно добавить обработку 401
if (response.status === 401) {
  authService.logout();
  window.location.href = '/login';
}
```


