# AuthService

Сервис аутентификации и авторизации для системы YandexLavka.

## Описание

AuthService предоставляет функциональность для:
- Регистрации новых аккаунтов
- Аутентификации пользователей (вход в систему)
- Выдачи JWT токенов для доступа к другим микросервисам
- Управления аккаунтами

Аккаунты созданные в AuthService могут использоваться в других микросервисах (UserService, WarehouseService и т.д.) для создания профилей и работы с системой.

## Структура проекта

Проект следует Clean Architecture и состоит из следующих слоев:

- **AuthService.Domain** - Доменная модель: сущности, value objects, интерфейсы репозиториев
- **AuthService.Application** - Бизнес-логика: команды, запросы, сервисы
- **AuthService.Infrastructure** - Инфраструктура: DbContext, репозитории, JWT сервисы
- **AuthService.API** - API слой: контроллеры, middleware

## API Endpoints

### Регистрация
```
POST /api/auth/register
Body: { "email": "user@example.com", "password": "password123" }
Response: { "accessToken": "...", "refreshToken": "...", "expiresAt": "...", "account": {...} }
```

### Вход
```
POST /api/auth/login
Body: { "email": "user@example.com", "password": "password123" }
Response: { "accessToken": "...", "refreshToken": "...", "expiresAt": "...", "account": {...} }
```

### Получить информацию об аккаунте
```
GET /api/auth/account/{id}
Response: { "id": "...", "email": "...", "isActive": true, ... }
```

## База данных

Используется PostgreSQL. База данных создается автоматически при первом запуске.

### Миграции

Миграции применяются автоматически при запуске через `EnsureCreatedAsync()`.

## Конфигурация

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5434;Database=AuthServiceDb;Username=postgres;Password=postgres;"
  },
  "Authentication": {
    "Jwt": {
      "Issuer": "AuthService",
      "Audience": "AuthServiceClients",
      "SecretKey": "your-secret-key-at-least-32-characters-long",
      "ExpirationHours": "24"
    }
  }
}
```

## Запуск через Docker Compose

Сервис автоматически запускается через docker-compose.yml в корне проекта:

```bash
docker compose up -d
```

Сервис будет доступен через API Gateway на:
- `http://localhost:8080/api/auth/...`

Или напрямую на:
- `http://localhost:8080` (через API Gateway)
- Swagger UI: `http://localhost:8080/swagger/auth/v1/swagger.json`

## Использование в других сервисах

После регистрации аккаунта в AuthService, можно использовать `account_id` (который возвращается в JWT токене) для создания профилей в других сервисах:

1. **UserService** - создать User профиль с привязкой к account_id
2. **WarehouseService** - использовать account_id для ассоциации заказов
3. **DeliveryService** - создать профиль доставщика с привязкой к account_id

JWT токен содержит claim `account_id`, который можно извлечь в других сервисах для проверки аутентификации.

## Безопасность

- Пароли хешируются с использованием BCrypt
- JWT токены подписываются секретным ключом
- Email должны быть уникальными
- Пароли должны быть минимум 8 символов

## Swagger

Swagger UI доступен по адресу:
- `http://localhost:8080/swagger` (через API Gateway)


