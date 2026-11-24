import { execSync } from 'node:child_process'
import { fileURLToPath } from 'node:url'
import { expect, test, type APIRequestContext, type Page } from '@playwright/test'

interface Statistics {
  total: string
  categories: { categoryId: string; name: string; total: string }[]
}

interface Expense {
  id: string
  date: string
  comment: string | null
}

const marker = 'E2E-finstat'
const repositoryRoot = fileURLToPath(new URL('../..', import.meta.url))
const number = new Intl.NumberFormat('uk-UA', { minimumFractionDigits: 2, maximumFractionDigits: 2 })

function amountText(amount: string) {
  return number.format(amount as Intl.StringNumericLiteral)
}

function toCents(amount: string) {
  const [integer, fraction = ''] = amount.split('.')
  return BigInt(integer) * 100n + BigInt(fraction.padEnd(2, '0'))
}

function categoryTotal(statistics: Statistics, categoryId: string) {
  return statistics.categories.find((c) => c.categoryId === categoryId)?.total
}

async function statistics(request: APIRequestContext, month: string) {
  const response = await request.get(`/api/statistics?month=${month}`)
  expect(response.ok()).toBe(true)
  return (await response.json()) as Statistics
}

async function removeTestExpenses(request: APIRequestContext, month: string) {
  const response = await request.get(`/api/expenses?month=${month}`)
  if (!response.ok()) return
  for (const expense of (await response.json()) as Expense[]) {
    if (expense.comment?.startsWith(marker)) await request.delete(`/api/expenses/${expense.id}`)
  }
}

async function expectTotal(page: Page, total: string) {
  await page.goto('/')
  const card = page.getByRole('region', { name: /^Витрачено за/ })
  await expect(card).toContainText(amountText(total))
}

let month = ''

test.afterEach(async ({ request }) => {
  if (month) await removeTestExpenses(request, month)
})

test('a new expense updates the overview and survives recreated containers', async ({
  page,
  request,
}) => {
  const categories = await (await request.get('/api/categories')).json()
  const category = categories.find((c: { isArchived: boolean }) => !c.isArchived)
  test.skip(!category, 'Потрібна хоча б одна активна категорія')
  const comment = `${marker} ${Date.now()}`

  await page.goto('/expenses')
  const form = page.getByRole('region', { name: 'Нова витрата' })
  const date = await form.getByLabel('Дата').inputValue()
  expect(date).toMatch(/^\d{4}-\d{2}-\d{2}$/)
  month = date.slice(0, 7)
  await removeTestExpenses(request, month)
  const before = await statistics(request, month)

  await form.getByLabel('Сума, грн').fill('123,45')
  await form.getByLabel('Категорія').selectOption(category.id)
  await form.getByLabel('Коментар').fill(comment)
  await form.getByRole('button', { name: 'Додати' }).click()
  await expect(page.getByRole('status')).toContainText('Витрату додано')
  await expect(page.getByRole('table')).toContainText(comment)

  const after = await statistics(request, month)
  expect(toCents(after.total) - toCents(before.total)).toBe(12345n)
  const totalAfter = categoryTotal(after, category.id)
  expect(totalAfter).toBeDefined()
  expect(toCents(totalAfter ?? '0') - toCents(categoryTotal(before, category.id) ?? '0')).toBe(12345n)

  await expectTotal(page, after.total)
  const row = page
    .getByRole('region', { name: 'Витрати за категоріями' })
    .getByRole('row')
    .filter({ has: page.getByRole('rowheader', { name: category.name, exact: true }) })
  await expect(row).toContainText(amountText(totalAfter ?? ''))
  await expect(page.getByRole('region', { name: 'Останні операції' })).toContainText(comment)

  execSync('docker compose up -d --force-recreate db api', { cwd: repositoryRoot })
  await expect
    .poll(async () => (await request.get('/api/categories').catch(() => null))?.ok() ?? false, {
      timeout: 90_000,
    })
    .toBe(true)

  await expectTotal(page, after.total)
  await page.goto('/expenses')
  const expenseRow = page.getByRole('row').filter({ hasText: comment })
  await expect(expenseRow).toBeVisible()

  await expenseRow.getByRole('button', { name: /Видалити витрату/ }).click()
  await page
    .getByRole('group', { name: 'Видалення витрати' })
    .getByRole('button', { name: 'Видалити' })
    .click()
  await expect(page.getByRole('status')).toContainText('Витрату видалено')
  expect((await statistics(request, month)).total).toBe(before.total)
})
