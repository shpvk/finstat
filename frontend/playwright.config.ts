import { defineConfig } from '@playwright/test'

export default defineConfig({
  testDir: './e2e',
  timeout: 120_000,
  workers: 1,
  reporter: 'list',
  use: {
    baseURL: process.env.E2E_BASE_URL ?? 'http://localhost:3000',
    channel: process.env.E2E_BROWSER_CHANNEL ?? 'chrome',
    locale: 'uk-UA',
    timezoneId: 'Europe/Kyiv',
    viewport: { width: 1280, height: 800 },
    trace: 'retain-on-failure',
  },
})
