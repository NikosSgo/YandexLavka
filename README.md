## YandexLavka Platform

Много сервисная учебная платформа, моделирующая основные процессы «Лавки»: управление аккаунтами, профилями, складом, заказами и клиентским web-интерфейсом.

> **Новое:** добавлен отдельный OrderService на MongoDB с конечным автоматом стадий, обновлён стек до .NET 9.0, документация сведена в единый обзор (`docs/PROJECT_STATUS.md`) и план (`docs/NEXT_STEPS.md`).

---

## Архитектура

- **AuthService** – регистрация, логин, выдача JWT (PostgreSQL).
- **UserService** – профили, адреса, пользовательские данные (PostgreSQL).
- **WareHouseService** – каталог, корзины, заказы и интеграция с Kafka/Redis.
- **OrderService** – основной сервис заказов с MongoDB и FSM.
- **ApiGateway** – единая точка входа (YARP).
- **Frontend** – React + TypeScript клиент, работает поверх gateway.
- **Инфраструктура** – PostgreSQL, MongoDB, Redis, Kafka, Zookeeper, PgAdmin, Kafka UI.

Актуальные диаграммы – `information/Architecture.md` и `information/diagrams`.

---

## Сервисы и порты

| Сервис | Технологии | Порт | Примечание |
| --- | --- | --- | --- |
| Frontend | React + Vite | `http://localhost:3000` | Запускается после gateway |
| API Gateway | ASP.NET Core + YARP | `http://localhost:8080` | `http://localhost:8080/swagger` – сводная документация |
| AuthService | ASP.NET Core, PostgreSQL | через gateway `/api/auth` | Прямой доступ внутри сети: `authservice-api:8080` |
| UserService | ASP.NET Core, PostgreSQL | `userservice-api` (через gateway) | БД `userservice-postgres:5432` |
| WareHouseService | ASP.NET Core, PostgreSQL, Redis, Kafka | `warehouse-service-api` | БД `warehouse-postgres:5432`, Redis `6379` |
| OrderService | ASP.NET Core, MongoDB | `http://localhost:8085` (прямой) | Подключен к `orderservice-mongo:27017` |
| Kafka UI | provectuslabs/kafka-ui | `http://localhost:8081` | |
| PgAdmin | dpage/pgadmin4 | `http://localhost:8082` | Email `admin@pgadmin.com`, пароль `admin` |

Полная матрица статусов и ограничений – `docs/PROJECT_STATUS.md`.

---

## Быстрый старт (Docker Compose)

1. **Зависимости**: Docker, Docker Compose, 16+ GB RAM рекомендуется.
2. **Запуск**:

   ```bash
   docker compose up --build -d
   docker compose ps
   ```

3. **Остановка**:

   ```bash
   docker compose down
   # с очисткой volumes
   docker compose down -v
   ```

4. **Частичный запуск** (например, только OrderService + Mongo):

   ```bash
   cd OrderService
   docker compose up --build
   ```

---

## Локальная разработка

| Компонент | Требуется |
| --- | --- |
| .NET SDK | **9.0** (Order/Auth/User/ApiGateway/OrderService), **8.0** (WareHouse) |
| Node.js | 20.x (Frontend) |
| MongoDB | 7.x (OrderService dev) |
| PostgreSQL | 15+/16 (User/Auth/Warehouse) |

- При отсутствии .NET 9 SDK используйте docker build/publish (host сборка завершится ошибкой `NETSDK1045`).
- Каждый сервис содержит свой README и `docker-compose.yml` для одиночного запуска.

---

## Документация

- `docs/README.md` – индекс всех материалов.
- `docs/PROJECT_STATUS.md` – текущая версия, матрица сервисов, риски.
- `docs/NEXT_STEPS.md` – рекомендованный план работ.
- `docs/architecture/overview.md` + `docs/architecture/diagrams/` – архитектура и схемы.
- `docs/guides/AUTH_USAGE.md`, `docs/guides/INTEGRATION_GUIDE.md`, `docs/guides/EXAMPLES.md` – интеграция с AuthService.
- `docs/guides/SECURITY_GUIDE.md` – основы по управлению секретами.
- `Frontend/README.md`, `UserService/README.md`, … – детали конкретных сервисов.

---

## Доступ к БД

**UserService**

- Host: `userservice-postgres`
- Port: `5432`
- Database: `UserServiceDb`
- Username: `postgres`
- Password: `postgres`

**WarehouseService**

- Host: `warehouse-postgres`
- Port: `5432`
- Database: `WareHouseDb`
- Username: `postgres`
- Password: `password`

**AuthService**

- Host: `authservice-postgres`
- Port: `5432`
- Database: `AuthServiceDb`
- Username: `postgres`
- Password: `postgres`

**OrderService**

- Mongo connection string: `mongodb://orderservice-mongo:27017`
- Database: `lavka-orders`

Подключаться удобно через PgAdmin (см. таблицу сервисов).

---

## Устранение неполадок

### Контейнеры не стартуют

```bash
docker compose logs
netstat -ano | findstr "3000 8080 8081 8082 5432 5433 6379 9092 2181"
```

### Kafka не поднимается

```bash
docker compose stop kafka kafka-topics-init kafka-ui
docker compose rm kafka
docker volume rm yandexlavka_kafka-data yandexlavka_kafka-logs
docker compose up -d kafka
```

### База данных/инициализация

```bash
docker compose down -v
docker compose up -d
```

### Kafka-топики

```bash
docker compose logs kafka-topics-init
```

---

## Что дальше?

Перечень рекомендованных улучшений (SDK, observability, OrderService ↔ Warehouse интеграция, DeliveryService и т.д.) собран в `docs/NEXT_STEPS.md`. Обновляйте `docs/PROJECT_STATUS.md` при каждом релизе, чтобы команда видела актуальную картину.

