# Documentation Index

Все материалы платформы теперь лежат в папке `docs/` и поделены на логические блоки. Используйте этот файл как точку входа, чтобы не искать раскиданные `.md` по всему репозиторию.

## 1. Платформенный срез

- `docs/PROJECT_STATUS.md` – актуальное состояние сервисов (что готово, что в разработке, риски).
- `docs/NEXT_STEPS.md` – рекомендуемый план работ по направлениям (инфраструктура, backend, frontend, UX, delivery).

## 2. Архитектура

- `docs/architecture/overview.md` – текстовое описание доменов (пользователи, склад, доставка и т.д.).
- `docs/architecture/diagrams/` – блок-схема и последовательности (Draw.io, PNG, Mermaid).

## 3. Гайды и интеграции

| Документ | Назначение |
| --- | --- |
| `docs/guides/AUTH_USAGE.md` | Быстрый старт по использованию AuthService в других сервисах |
| `docs/guides/INTEGRATION_GUIDE.md` | Расширенное руководство по связке AuthService ↔ User/Warehouse/Delivery |
| `docs/guides/EXAMPLES.md` | Примеры кода (контроллеры, команды, curl-скрипты) |
| `docs/guides/SECURITY_GUIDE.md` | Как хранить секреты, настраивать JWT и переменные окружения |

## 4. README отдельных сервисов

- `AuthService/README.md`
- `UserService/README.md`
- `WareHouseService/README.md`
- `OrderService/` (пока без собственного README – см. корневой обзор)
- `Frontend/README.md`

## 5. Дополнительно

- Корневой `README.md` – быстрый старт, матрица сервисов, порты/URL, устранение неполадок.
- `docs` – всё, что требуется для продуктового/архитектурного контекста и безопасности.

> Обновляйте `docs/PROJECT_STATUS.md` и `docs/NEXT_STEPS.md` каждый раз, когда закрываете итерацию, чтобы документация оставалась консистентной.


