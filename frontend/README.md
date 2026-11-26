# Finstat: frontend

React, TypeScript (strict) and Vite. Requires Node.js 22 (22.13 or later) or 24.

## Commands

Run them in the `frontend/` folder.

| Command | What it does |
| --- | --- |
| `npm ci` | Installs dependencies exactly as in `package-lock.json` |
| `npm run dev` | Dev server on http://localhost:5173 |
| `npm run typecheck` | Type check |
| `npm run lint` | Linter (oxlint) |
| `npm test` | Tests (Vitest + React Testing Library) |
| `npm run build` | Type check and build into `dist/` |
| `npm run test:e2e` | End-to-end browser test (Playwright) against a running `docker compose up` |

### End-to-end test

`e2e/expense-flow.spec.ts` adds an expense through the UI, checks that the monthly total and the category table on «Огляд» (Overview) updated, recreates the `db` and `api` containers (`docker compose up -d --force-recreate db api`; the data volume is left alone) and confirms the expense is still there. At the end the expense is deleted, so the database is left unchanged; the date and month are read from the form, so the test doesn't depend on the machine's time zone. It needs a running Compose stack and at least one active category.

- App address: `E2E_BASE_URL`, default http://localhost:3000.
- Browser: an installed Google Chrome (`E2E_BROWSER_CHANNEL`, default `chrome`). Without Chrome: `npx playwright install chromium` and `E2E_BROWSER_CHANNEL=chromium`.

## Layout

- `src/App.tsx`: routes.
- `src/app`: the app shell with the menu and the "not found" page.
- `src/features`: screens by section: `overview`, `expenses`, `categories`.
- `src/ui`: base components: button, field, select, loading, empty and error states, page header.
- `src/styles`: visual system tokens and base styles.
- `src/api`: the API client.

Visual system decisions are recorded in `docs/technical-decisions.md`. Every base component in every state can be viewed on the reference page http://localhost:5173/ui; it is available only under `npm run dev`.

## Talking to the API

The client is `src/api/client.ts`. All requests go through `apiRequest`; any failure arrives as an `ApiError` with the status and the parsed Problem Details. The user sees `detail` from the Problem Details, so the backend writes it in Ukrainian. A request cancelled through `signal` doesn't become an `ApiError`: the original abort error is rethrown as is.

- Status `0` in `ApiError` means the full response couldn't be received: the server is unreachable or the connection dropped.
- `title` from the Problem Details isn't shown to the user: for errors raised by ASP.NET Core itself it's a generic English title. It stays available in `ApiError.problem`.
- An empty body of a successful response (e.g. `204`) is returned as `undefined`.

## Settings

- `VITE_API_BASE_URL`: the API base address for the browser, without a trailing slash. Defaults to `/api`, i.e. the same origin: the Vite proxy in development, the web server in Docker.
- `API_PROXY_TARGET`: the address of the running backend that the dev server proxies `/api` to. Defaults to `http://localhost:8080`.

`.env.example` holds the defaults. To change them, copy the file to `frontend/.env`.
