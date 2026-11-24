import { execSync } from 'node:child_process'
import { fileURLToPath } from 'node:url'
import { expect, test, type APIRequestContext, type Page } from '@playwright/test'

interface Statistics {
  total: string
  categories: { categoryId: string; name: string; total: string }[]
}

interface Expense {
  id: string
  comment: string | null
}

const repositoryRoot = fileURLToPath(new URL('../..', import.meta.url))
const number = new Intl.NumberFormat('uk-UA', { minimumFractionDigits: 2, maximumFractionDigits: 2 })

function amountText(amount: string) {
  return number.format(amount as Intl.StringNumericLiteral)
}

function today() {
  const now = new Date()
  return [
    now.getFullYear(),
    String(now.getMonth() + 1).padStart(2, '0'),
    String(now.getDate()).padStart(2, '0'),
  ].join('-')
}

function toCents(amount: string) {
  const [integer, fraction = ''] = amount.split('.')
  return BigInt(integer) * 100n + BigInt(fraction.padEnd(2, '0'))
}

async function statistics(request: APIRequestContext) {
  const response = await request.get(`/api/statistics?month=${today().slice(0, 7)}`)
  expect(response.ok()).toBe(true)
  return (await response.json()) as Statistics
}

async function removeTestExpenses(request: APIRequestContext) {
  const response = await request.get(`/api/expenses?month=${today().slice(0, 7)}`)
  if (!response.ok()) return
  for (const expense of (await response.json()) as Expense[]) {
    if (expense.comment?.startsWith('E2E ')) await request.delete(`/api/expenses/${expense.id}`)
  }
}

async function expectTotal(page: Page, total: string) {
  await page.goto('/')
  const card = page.getByRole('region', { name: /^Витрачено за/ })
  await expect(card).toContainText(amountText(total))
}

test.afterEach(async ({ request }) => {
  await removeTestExpenses(request)
})

test('a new expense updates the overview and survives a restart', async ({ page, request }) => {
  await removeTestExpenses(request)
  const categories = await (await request.get('/api/categories')).json()
  const category = categories.find((c: { isArchived: boolean }) => !c.isArchived)
  test.skip(!category, 'Потрібна хоча б одна активна категорія')
  const comment = `E2E ${Date.now()}`
  const before = await statistics(request)

  await page.goto('/expenses')
  const form = page.getByRole('region', { name: 'Нова витрата' })
  await form.getByLabel('Сума, грн').fill('123,45')
  await form.getByLabel('Категорія').selectOption(category.id)
  await expect(form.getByLabel('Дата')).toHaveValue(today())
  await form.getByLabel('Коментар').fill(comment)
  await form.getByRole('button', { name: 'Додати' }).click()
  await expect(page.getByRole('status')).toContainText('Витрату додано')
  await expect(page.getByRole('table')).toContainText(comment)

  const after = await statistics(request)
  expect(toCents(after.total) - toCents(before.total)).toBe(12345n)
  await expectTotal(page, after.total)

  const breakdown = page.getByRole('region', { name: 'Витрати за категоріями' })
  const row = breakdown.getByRole('row', { name: new RegExp(category.name) })
  const categoryTotal = after.categories.find((c) => c.categoryId === category.id)?.total ?? ''
  await expect(row).toContainText(amountText(categoryTotal))
  await expect(page.getByRole('region', { name: 'Останні операції' })).toContainText(comment)

  execSync('docker compose restart db api', { cwd: repositoryRoot, stdio: 'ignore' })
  await expect
    .poll(async () => (await request.get('/api/categories').catch(() => null))?.ok() ?? false, {
      timeout: 60_000,
    })
    .toBe(true)

  await expectTotal(page, after.total)
  await page.goto('/expenses')
  const expenseRow = page.getByRole('row', { name: new RegExp(comment) })
  await expect(expenseRow).toBeVisible()

  await expenseRow.getByRole('button', { name: /Видалити витрату/ }).click()
  await page
    .getByRole('group', { name: 'Видалення витрати' })
    .getByRole('button', { name: 'Видалити' })
    .click()
  await expect(page.getByRole('status')).toContainText('Витрату видалено')
  expect((await statistics(request)).total).toBe(before.total)
})
