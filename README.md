# finstat

Облачный трекер личных финансов (MVP). UI на украинском, код и коммиты на английском. Решения: `docs/`.

## Структура backend (`backend/`)

- `src/Finstat.Domain` — `Entities/`, `Results/`: сущности и инварианты, без внешних зависимостей.
- `src/Finstat.Application` — `Services/`, `DTOs/`, `Repositories/`, `Queries/`: сценарии использования (зависит от Domain).
- `src/Finstat.Infrastructure` — `Repositories/`, `Queries/`, `Configurations/`, EF Core (Npgsql), `FinstatDbContext`, миграции.
- `src/Finstat.Api` — `Controllers/`, `Helpers/`: контроллеры, Problem Details, `/health`.

## Запуск через Docker Compose

```
cp .env.example .env     # задайте POSTGRES_PASSWORD
docker compose up --build
```

Пароль не должен содержать `;` и `'`: он подставляется в строку подключения.

API: http://localhost:8080 (`/health`). PostgreSQL хранится в именованном томе `finstat-pgdata`.
Остановка: `docker compose down`. Флаг `-v` удаляет данные — только осознанно.
Порты хоста меняются через `API_PORT` и `DB_PORT` в `.env`.

## Локальная разработка

БД из Compose, API локально:

```
docker compose up -d db
cd backend/src/Finstat.Api
dotnet user-secrets set "ConnectionStrings:Finstat" "Host=localhost;Port=5432;Database=finstat;Username=finstat;Password=<пароль из .env>"
dotnet run
```

(порт в строке подключения — `DB_PORT` из `.env`.) Профиль `http` слушает http://localhost:8080 в режиме Development:
OpenAPI — `/openapi/v1.json`, Scalar UI — `/scalar`. В Docker-образе эти адреса отключены.

Строка подключения обязательна: `ConnectionStrings:Finstat` (env `ConnectionStrings__Finstat`), иначе API не стартует.

## Проверки

Из `backend/`:

```
dotnet build
```

## Миграции

Применяются автоматически при старте API (`Database.MigrateAsync`). Новая миграция (из `backend/`):

```
dotnet tool restore
dotnet ef migrations add <Name> -p src/Finstat.Infrastructure -s src/Finstat.Infrastructure -o Migrations
```

Удалить последнюю ещё не применённую миграцию: та же команда с `migrations remove --force` (инструмент не подключается к БД, поэтому без `--force` не может проверить, применена ли она).
