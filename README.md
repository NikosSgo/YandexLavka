YandexLavka - проект с микросервисной архитектурой для системы пользователей, заказов, управления складом и доставкой.

Запуск проекта с помощью Docker Compose: docker compose up -d. 
Проверьте статус контейнеров: docker compose ps

После успешного запуска следующие сервисы будут доступны:

| Сервис | URL | Описание |
|--------|-----|----------|
| **Frontend** | http://localhost:3000 | Веб-интерфейс приложения |
| **API Gateway** | http://localhost:8080 | Единая точка входа для всех API |
| **API Gateway Swagger** | http://localhost:8080/swagger | Документация API |
| **Kafka UI** | http://localhost:8081 | Веб-интерфейс для управления Kafka |
| **PgAdmin** | http://localhost:8082 | Веб-интерфейс для управления базами данных |

Учетные данные для PgAdmin:
- Email: `admin@pgadmin.com`
- Password: `admin`

Подключение к базам данных через PgAdmin:

UserService Database:
- Host: `userservice-postgres`
- Port: `5432`
- Database: `UserServiceDb`
- Username: `postgres`
- Password: `postgres`

Warehouse Database:
- Host: `warehouse-postgres`
- Port: `5432`
- Database: `WareHouseDb`
- Username: `postgres`
- Password: `password`

Проект состоит из следующих микросервисов:

- ApiGateway - API Gateway на базе YARP, маршрутизирует запросы к микросервисам
- UserService - Сервис управления пользователями и адресами
- WareHouseService - Сервис управления складом, заказами и товарами
- Frontend - React приложение с TypeScript

Инфраструктурные компоненты

- PostgreSQL:
  - `userservice-postgres` - база данных для UserService (порт 5432)
  - `warehouse-postgres` - база данных для WarehouseService (порт 5433)
- Redis - кэширование для WarehouseService (порт 6379)
- Kafka - брокер сообщений (порт 9092)
  - Топики: `orders`, `warehouse-commands`, `warehouse-events`, `picking-tasks`, `stock-updates`
- Zookeeper - координация для Kafka (порт 2181)

Устранение неполадок:

Проблема: Контейнеры не запускаются
# Проверьте логи
docker compose logs
# Убедитесь, что порты не заняты
netstat -tuln | grep -E ':(3000|8080|8081|8082|5432|5433|6379|9092|2181)'

Проблема: База данных не инициализируется
docker compose down -v
docker compose up -d

Проблема: Kafka топики не создаются
docker compose logs kafka-topics-init
docker compose restart kafka-topics-init
