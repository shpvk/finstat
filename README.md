# finstat

Локальный трекер расходов: ручной ввод, категории, статистика по месяцам. Бэкенд — .NET 10 (DDD), фронтенд — React + TypeScript + Vite.

Решения по продукту и архитектуре:
- [docs/business-decisions.md](docs/business-decisions.md) — сценарии и границы MVP.
- [docs/technical-decisions.md](docs/technical-decisions.md) — стек, архитектура, правила.

## Разработка с Claude Code

Весь процесс описан в репозитории: `CLAUDE.md` и `.claude/` (агенты, команды, настройки, Git-политика, скилл `impeccable` для UI).

Нужно установить: Claude Code, Git, GitHub CLI (`gh auth login`), Node.js (для UI-хуков), .NET 10 SDK.

1. Клонировать репозиторий и открыть в нём Claude Code.
2. Доверить проект и согласиться установить плагин `ponytail`, когда Claude Code его предложит.
3. Задача: `/do <что сделать>`. Claude сам проходит этапы разработки, ревью и тестирования, но ничего не коммитит.
4. Публикация: `/git commit`, `/git publish` (или `/git commit and publish`). Каждая задача делается в своей ветке и попадает в `Development` через pull request.

Локальными остаются только `.claude/settings.local.json` и журналы задач `.claude/work/`.
