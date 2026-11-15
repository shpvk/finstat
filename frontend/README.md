# Finstat: фронтенд

React, TypeScript (strict) и Vite. Нужен Node.js 22 (от 22.13) или 24.

## Команды

Выполняются в папке `frontend/`.

| Команда | Что делает |
| --- | --- |
| `npm ci` | Ставит зависимости строго по `package-lock.json` |
| `npm run dev` | Dev-сервер на http://localhost:5173 |
| `npm run typecheck` | Проверка типов |
| `npm run lint` | Линтер (oxlint) |
| `npm test` | Тесты (Vitest + React Testing Library) |
| `npm run build` | Проверка типов и сборка в `dist/` |

## Связь с API

Клиент — `src/api/client.ts`. Все запросы идут через `apiRequest`; любая неудача приходит как `ApiError` со статусом и разобранным Problem Details. Пользователю показывается `detail` из Problem Details, поэтому бэкенд пишет его по-украински. Отменённый через `signal` запрос не превращается в `ApiError`: исходная ошибка отмены пробрасывается как есть.

- `VITE_API_BASE_URL` — базовый адрес API для браузера, по умолчанию `/api` (тот же origin).
- `API_PROXY_TARGET` — куда dev-сервер проксирует `/api`, по умолчанию `http://localhost:8080`.

Значения задаются в `frontend/.env`, образец — `.env.example`.
