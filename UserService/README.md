# User Service API

API для управления пользователями и их адресами.

## Технологии

- .NET 9.0
- PostgreSQL 16
- Entity Framework Core
- MediatR (CQRS)
- FluentValidation
- Docker & Docker Compose

## Структура проекта

```
UserService/
├── UserService.API/           # API слой
├── UserService.Application/   # Application слой (CQRS, команды, запросы)
├── UserService.Domain/        # Domain слой (сущности, интерфейсы)
├── UserService.Infrastructure/# Infrastructure слой (репозитории, DbContext)
└── docker-compose.yml         # Docker Compose конфигурация
```

## Запуск через Docker Compose

### Требования

- Docker
- Docker Compose

### Запуск

1. Клонируйте репозиторий и перейдите в директорию проекта:
```bash
cd UserService
```

2. Запустите все сервисы через Docker Compose:
```bash
docker-compose up -d
```

3. Проверьте, что сервисы запущены:
```bash
docker-compose ps
```

4. Откройте Swagger UI в браузере:
```
http://localhost:8080
```

### Остановка

```bash
docker-compose down
```

### Остановка с удалением volumes (удаление данных БД)

```bash
docker-compose down -v
```

## API Endpoints

### Users

- `GET /api/users` - Получить всех пользователей
- `GET /api/users/{id}` - Получить пользователя по ID
- `GET /api/users/by-phone/{phone}` - Получить пользователя по телефону
- `GET /api/users/by-email/{email}` - Получить пользователя по email
- `POST /api/users` - Создать пользователя
- `PUT /api/users/{id}` - Обновить пользователя
- `PATCH /api/users/{id}/phone` - Изменить телефон
- `PATCH /api/users/{id}/email` - Изменить email
- `DELETE /api/users/{id}` - Удалить пользователя

### Addresses

- `GET /api/users/{userId}/addresses` - Получить все адреса пользователя
- `GET /api/users/{userId}/addresses/{addressId}` - Получить адрес по ID
- `GET /api/users/{userId}/addresses/primary` - Получить основной адрес
- `POST /api/users/{userId}/addresses` - Добавить адрес
- `PUT /api/users/{userId}/addresses/{addressId}` - Обновить адрес
- `DELETE /api/users/{userId}/addresses/{addressId}` - Удалить адрес
- `PATCH /api/users/{userId}/addresses/{addressId}/set-primary` - Установить адрес как основной

## База данных

PostgreSQL запускается в Docker контейнере с следующими настройками:

- **Host**: localhost
- **Port**: 5432
- **Database**: UserServiceDb
- **Username**: postgres
- **Password**: postgres

### Подключение к базе данных

```bash
docker exec -it userservice-postgres psql -U postgres -d UserServiceDb
```

## Локальная разработка

### Требования

- .NET 9.0 SDK
- PostgreSQL 16

### Настройка

1. Обновите connection string в `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=UserServiceDb;Username=postgres;Password=postgres"
  }
}
```

2. Запустите PostgreSQL локально или через Docker:
```bash
docker run -d --name postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=UserServiceDb -p 5432:5432 postgres:16-alpine
```

3. Запустите API:
```bash
cd UserService.API
dotnet run
```

4. Откройте Swagger UI:
```
http://localhost:5000
```

## Миграции

Для создания миграций:

```bash
cd UserService.Infrastructure
dotnet ef migrations add MigrationName --startup-project ../UserService.API/UserService.API.csproj
```

Для применения миграций:

```bash
dotnet ef database update --startup-project ../UserService.API/UserService.API.csproj
```

## Логи

Просмотр логов API:
```bash
docker-compose logs -f api
```

Просмотр логов PostgreSQL:
```bash
docker-compose logs -f postgres
```

## Очистка

Удалить все контейнеры, volumes и сети:
```bash
docker-compose down -v --remove-orphans
```

## Лицензия

MIT

