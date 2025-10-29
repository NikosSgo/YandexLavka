# WareHouse Service

Микросервис для управления складскими операциями в системе Яндекс Лавки.

## Архитектура

Проект построен по принципам чистой архитектуры (Clean Architecture) с разделением на слои:

- **API** - Веб-API с контроллерами и middleware
- **Application** - Бизнес-логика приложения (Commands, Queries, DTOs)
- **Domain** - Бизнес-модели и правила (Entities, Value Objects, Domain Events)
- **Infrastructure** - Внешние зависимости (Database, Kafka, Redis)
- **Tests** - Unit и интеграционные тесты

## Технологии

- .NET 8.0
- Entity Framework Core 8.0
- PostgreSQL
- Kafka
- Redis
- xUnit + Moq + FluentAssertions
- Serilog

## Запуск

1. Убедитесь, что установлены .NET 8.0 SDK и Docker
2. Клонируйте репозиторий
3. Запустите инфраструктуру: `docker-compose up -d`
4. Запустите приложение: `dotnet run --project src/WareHouse.API`

## API Documentation

После запуска документация доступна по адресу: `https://localhost:7001/api-docs`