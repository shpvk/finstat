import { fireEvent, render, screen, within } from '@testing-library/react'
import { MemoryRouter } from 'react-router'
import { afterEach, describe, expect, it, vi } from 'vitest'
import type { Category } from '../../api/categories.ts'
import type { Expense } from '../../api/expenses.ts'
import { MonthProvider } from '../../shared/MonthProvider.tsx'
import { fakeApi, problem } from '../../test/fakeApi.ts'
import { ToastProvider } from '../../ui/Toast.tsx'
import { ExpensesPage } from './ExpensesPage.tsx'

const categories: Category[] = [
  { id: 'food', name: 'Їжа', isArchived: false },
  { id: 'taxi', name: 'Таксі', isArchived: true },
]

function renderPage() {
  render(
    <MemoryRouter>
      <ToastProvider>
        <MonthProvider initial="2026-11">
          <ExpensesPage />
        </MonthProvider>
      </ToastProvider>
    </MemoryRouter>,
  )
}

function expensesApi(initial: Expense[]) {
  const expenses = [...initial]
  const fetchMock = fakeApi({
    'GET /api/categories': () => ({ body: categories }),
    'GET /api/expenses': ({ url }) => ({
      body: expenses.filter((e) => e.date.startsWith(url.searchParams.get('month') ?? '')),
    }),
    'POST /api/expenses': ({ body }) => {
      const expense = { id: `e${expenses.length + 1}`, ...(body as Omit<Expense, 'id'>) }
      expenses.push(expense)
      return { status: 201, body: expense }
    },
  })
  return { expenses, fetchMock }
}

function requests(fetchMock: ReturnType<typeof fakeApi>, method: string) {
  return fetchMock.mock.calls.filter(([, init]) => (init?.method ?? 'GET') === method)
}

const lunch: Expense = {
  id: 'e1',
  amount: '1250.50',
  categoryId: 'food',
  date: '2026-11-20',
  comment: 'Обід',
}

afterEach(() => {
  vi.useRealTimers()
  vi.unstubAllGlobals()
})

describe('ExpensesPage', () => {
  it('lists the month expenses with formatted amounts and categories', async () => {
    expensesApi([lunch, { id: 'e2', amount: '90', categoryId: 'taxi', date: '2026-11-02', comment: null }])
    renderPage()

    const table = await screen.findByRole('table')
    const rows = within(table).getAllByRole('row')
    expect(rows).toHaveLength(3)
    expect(rows[1]).toHaveTextContent('20 листопада')
    expect(rows[1]).toHaveTextContent('Обід')
    expect(rows[1].textContent).toMatch(/1\s250,50/)
    expect(rows[1]).toHaveTextContent(/20 листопада \S+/)
    expect(rows[2]).toHaveTextContent('Таксі в архіві')
    expect(screen.getByText('2 витрати')).toBeInTheDocument()
  })

  it('offers only active categories for a new expense and saves exact values', async () => {
    const { fetchMock } = expensesApi([])
    renderPage()

    const category = await screen.findByLabelText('Категорія')
    expect(within(category).getAllByRole('option').map((o) => o.textContent)).toEqual([
      'Оберіть категорію',
      'Їжа',
    ])

    fireEvent.change(screen.getByLabelText('Сума, грн'), { target: { value: '1 250,5' } })
    fireEvent.change(category, { target: { value: 'food' } })
    fireEvent.change(screen.getByLabelText('Дата'), { target: { value: '2026-11-21' } })
    fireEvent.click(screen.getByRole('button', { name: 'Додати' }))

    expect(await screen.findByText('Витрату додано')).toBeInTheDocument()
    expect(requests(fetchMock, 'POST')[0][1]?.body).toBe(
      '{"amount":"1250.5","categoryId":"food","date":"2026-11-21","comment":null}',
    )
    expect(await screen.findByRole('table')).toHaveTextContent('21 листопада')
    expect(screen.getByLabelText('Сума, грн')).toHaveValue('')
  })

  it('defaults the date to the local today', async () => {
    vi.useFakeTimers({ toFake: ['Date'], now: new Date(2026, 10, 24, 0, 30) })
    expensesApi([])
    renderPage()

    expect(await screen.findByLabelText('Дата')).toHaveValue('2026-11-24')
  })

  it('checks the form before sending', async () => {
    const { fetchMock } = expensesApi([])
    renderPage()
    await screen.findByLabelText('Категорія')

    fireEvent.change(screen.getByLabelText('Сума, грн'), { target: { value: '12,345' } })
    fireEvent.click(screen.getByRole('button', { name: 'Додати' }))

    expect(screen.getByText('Сума може мати не більше двох знаків після коми.')).toBeInTheDocument()
    expect(screen.getByText('Оберіть категорію.')).toBeInTheDocument()
    expect(requests(fetchMock, 'POST')).toHaveLength(0)
  })

  it('shows an expense added to another month by switching to it', async () => {
    const { fetchMock } = expensesApi([])
    renderPage()
    await screen.findByLabelText('Категорія')

    fireEvent.change(screen.getByLabelText('Сума, грн'), { target: { value: '300' } })
    fireEvent.change(screen.getByLabelText('Категорія'), { target: { value: 'food' } })
    fireEvent.change(screen.getByLabelText('Дата'), { target: { value: '2026-10-05' } })
    fireEvent.click(screen.getByRole('button', { name: 'Додати' }))

    expect(await screen.findByText('Витрату додано за жовтень 2026')).toBeInTheDocument()
    expect(screen.getByText('Жовтень 2026')).toBeInTheDocument()
    expect(await screen.findByText('5 жовтня')).toBeInTheDocument()
    expect(requests(fetchMock, 'GET').some(([url]) => String(url).endsWith('month=2026-10'))).toBe(true)
  })

  it('edits an expense in place', async () => {
    const expenses = [lunch]
    const fetchMock = fakeApi({
      'GET /api/categories': () => ({ body: categories }),
      'GET /api/expenses': () => ({ body: expenses }),
      'PUT /api/expenses/e1': ({ body }) => {
        expenses[0] = { id: 'e1', ...(body as Omit<Expense, 'id'>) }
        return { body: expenses[0] }
      },
    })
    renderPage()

    fireEvent.click(await screen.findByRole('button', { name: /Редагувати витрату/ }))
    const amount = screen.getAllByLabelText('Сума, грн')[1]
    expect(amount).toHaveValue('1250,50')
    fireEvent.change(amount, { target: { value: '1300' } })
    fireEvent.click(screen.getByRole('button', { name: 'Зберегти' }))

    expect(await screen.findByText('Зміни збережено')).toBeInTheDocument()
    expect(requests(fetchMock, 'PUT')[0][1]?.body).toBe(
      '{"amount":"1300","categoryId":"food","date":"2026-11-20","comment":"Обід"}',
    )
    expect((await screen.findByRole('table')).textContent).toMatch(/1\s300,00/)
    expect(screen.getByRole('button', { name: /Редагувати витрату/ })).toHaveFocus()
  })

  it('keeps an archived category available only for the expense already in it', async () => {
    expensesApi([{ id: 'e2', amount: '90', categoryId: 'taxi', date: '2026-11-02', comment: null }])
    renderPage()

    fireEvent.click(await screen.findByRole('button', { name: /Редагувати витрату/ }))
    const select = screen.getAllByLabelText('Категорія')[1]
    expect(select).toHaveValue('taxi')
    expect(within(select).getAllByRole('option').map((o) => o.textContent)).toEqual([
      'Оберіть категорію',
      'Їжа',
      'Таксі (в архіві)',
    ])
  })

  it('removes an expense moved to another month and keeps focus in the list', async () => {
    const expenses = [lunch, { ...lunch, id: 'e2', amount: '40', date: '2026-11-03', comment: null }]
    fakeApi({
      'GET /api/categories': () => ({ body: categories }),
      'GET /api/expenses': () => ({ body: expenses.filter((e) => e.date.startsWith('2026-11')) }),
      'PUT /api/expenses/e1': ({ body }) => {
        expenses[0] = { id: 'e1', ...(body as Omit<Expense, 'id'>) }
        return { body: expenses[0] }
      },
    })
    renderPage()

    fireEvent.click(await screen.findByRole('button', { name: /Редагувати витрату 1/ }))
    fireEvent.change(screen.getAllByLabelText('Дата')[1], { target: { value: '2026-12-01' } })
    fireEvent.click(screen.getByRole('button', { name: 'Зберегти' }))

    expect(await screen.findByText('Витрату перенесено на грудень 2026')).toBeInTheDocument()
    expect(screen.queryByText('Обід')).not.toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Редагувати витрату 40/ })).toHaveFocus()
  })

  it('cancels editing with Escape and moves focus between rows', async () => {
    const fetchMock = expensesApi([
      lunch,
      { ...lunch, id: 'e2', amount: '40', date: '2026-11-03', comment: null },
    ]).fetchMock
    renderPage()

    fireEvent.click(await screen.findByRole('button', { name: /Редагувати витрату 1/ }))
    fireEvent.click(screen.getByRole('button', { name: /Видалити витрату 40/ }))
    const confirm = screen.getByRole('group', { name: 'Видалення витрати' })
    expect(within(confirm).getByRole('button', { name: 'Скасувати' })).toHaveFocus()
    expect(screen.queryByRole('button', { name: 'Зберегти' })).not.toBeInTheDocument()

    fireEvent.keyDown(confirm, { key: 'Escape' })
    expect(screen.getByRole('button', { name: /Видалити витрату 40/ })).toHaveFocus()
    expect(requests(fetchMock, 'DELETE')).toHaveLength(0)
  })

  it('does not offer archived categories when editing an active one', async () => {
    expensesApi([lunch])
    renderPage()

    fireEvent.click(await screen.findByRole('button', { name: /Редагувати витрату/ }))
    const select = screen.getAllByLabelText('Категорія')[1]
    expect(within(select).getAllByRole('option').map((o) => o.textContent)).toEqual([
      'Оберіть категорію',
      'Їжа',
    ])
  })

  it('keeps the row when deleting fails', async () => {
    fakeApi({
      'GET /api/categories': () => ({ body: categories }),
      'GET /api/expenses': () => ({ body: [lunch] }),
      'DELETE /api/expenses/e1': () => problem('Витрату не знайдено', 404),
    })
    renderPage()

    fireEvent.click(await screen.findByRole('button', { name: /Видалити витрату/ }))
    const confirm = screen.getByRole('group', { name: 'Видалення витрати' })
    fireEvent.click(within(confirm).getByRole('button', { name: 'Видалити' }))

    expect(await within(confirm).findByRole('alert')).toHaveTextContent('Витрату не знайдено')
    expect(screen.getByRole('table')).toBeInTheDocument()
  })

  it('deletes an expense after confirmation', async () => {
    const expenses = [lunch]
    fakeApi({
      'GET /api/categories': () => ({ body: categories }),
      'GET /api/expenses': () => ({ body: expenses }),
      'DELETE /api/expenses/e1': () => {
        expenses.pop()
        return { status: 204 }
      },
    })
    renderPage()

    fireEvent.click(await screen.findByRole('button', { name: /Видалити витрату/ }))
    const confirm = screen.getByRole('group', { name: 'Видалення витрати' })
    expect(within(confirm).getByRole('button', { name: 'Скасувати' })).toHaveFocus()
    fireEvent.click(within(confirm).getByRole('button', { name: 'Видалити' }))

    expect(await screen.findByText('Витрату видалено')).toBeInTheDocument()
    expect(await screen.findByText('Витрат за цей місяць немає')).toBeInTheDocument()
  })

  it('shows the server error when saving fails', async () => {
    fakeApi({
      'GET /api/categories': () => ({ body: categories }),
      'GET /api/expenses': () => ({ body: [] }),
      'POST /api/expenses': () => problem('Категорія в архіві, оберіть активну категорію'),
    })
    renderPage()
    await screen.findByLabelText('Категорія')

    fireEvent.change(screen.getByLabelText('Сума, грн'), { target: { value: '10' } })
    fireEvent.change(screen.getByLabelText('Категорія'), { target: { value: 'food' } })
    fireEvent.click(screen.getByRole('button', { name: 'Додати' }))

    expect(await screen.findByRole('alert')).toHaveTextContent('Категорія в архіві, оберіть активну категорію')
    expect(screen.getByLabelText('Сума, грн')).toHaveValue('10')
  })

  it('switches months and asks to create a category first when there are none', async () => {
    const fetchMock = fakeApi({
      'GET /api/categories': () => ({ body: [] }),
      'GET /api/expenses': () => ({ body: [] }),
    })
    renderPage()

    expect(await screen.findByRole('link', { name: 'додайте категорію' })).toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: 'Попередній місяць' }))

    expect(screen.getByText('Жовтень 2026')).toBeInTheDocument()
    expect(await screen.findByText('Витрат за цей місяць немає')).toBeInTheDocument()
    expect(requests(fetchMock, 'GET').some(([url]) => String(url).endsWith('month=2026-10'))).toBe(true)
  })
})
