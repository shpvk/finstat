import { fireEvent, render, screen, within } from '@testing-library/react'
import { MemoryRouter } from 'react-router'
import { afterEach, describe, expect, it, vi } from 'vitest'
import type { MonthStatistics } from '../../api/statistics.ts'
import { MonthProvider } from '../../shared/MonthProvider.tsx'
import { fakeApi, problem } from '../../test/fakeApi.ts'
import { OverviewPage } from './OverviewPage.tsx'

const november: MonthStatistics = {
  month: '2026-11',
  total: '1690.75',
  categories: [
    { categoryId: 'bills', name: 'Комунальні послуги', isArchived: false, total: '380.00', share: '22.5' },
    { categoryId: 'food', name: 'Їжа', isArchived: false, total: '1310.75', share: '77.5' },
  ],
  recent: [
    { id: 'e2', amount: '380.00', categoryId: 'bills', date: '2026-11-24', comment: null },
    { id: 'e1', amount: '1310.75', categoryId: 'food', date: '2026-11-20', comment: 'Сільпо' },
  ],
}

const empty = (month: string): MonthStatistics => ({ month, total: '0.00', categories: [], recent: [] })

function renderPage() {
  render(
    <MemoryRouter>
      <MonthProvider initial="2026-11">
        <OverviewPage />
      </MonthProvider>
    </MemoryRouter>,
  )
}

afterEach(() => {
  vi.useRealTimers()
  vi.unstubAllGlobals()
})

describe('OverviewPage', () => {
  it('shows the month total, the biggest category and the latest operations', async () => {
    fakeApi({ 'GET /api/statistics': () => ({ body: november }) })
    renderPage()

    const total = await screen.findByRole('region', { name: 'Витрачено за листопад 2026' })
    expect(total.textContent).toMatch(/1\s690,75/)
    expect(total).toHaveTextContent('2 категорії')
    expect(total).toHaveTextContent('Найбільше — Їжа, 77,5% витрат')

    const recent = screen.getByRole('region', { name: 'Останні операції' })
    const items = within(recent).getAllByRole('listitem')
    expect(items).toHaveLength(2)
    expect(items[0]).toHaveTextContent('Комунальні послуги')
    expect(items[1]).toHaveTextContent('20 листопада · Сільпо')
    expect(within(recent).getByRole('link', { name: 'Усі витрати' })).toHaveAttribute('href', '/expenses')
  })

  it('replaces the total and the latest operations with the other month', async () => {
    const october: MonthStatistics = {
      month: '2026-10',
      total: '75.00',
      categories: [{ categoryId: 'taxi', name: 'Таксі', isArchived: true, total: '75.00', share: '100.0' }],
      recent: [{ id: 'e9', amount: '75.00', categoryId: 'taxi', date: '2026-10-30', comment: null }],
    }
    fakeApi({
      'GET /api/statistics': ({ url }) => ({
        body: url.searchParams.get('month') === '2026-10' ? october : november,
      }),
    })
    renderPage()
    await screen.findByRole('region', { name: 'Витрачено за листопад 2026' })

    fireEvent.click(screen.getByRole('button', { name: 'Попередній місяць' }))

    const total = await screen.findByRole('region', { name: 'Витрачено за жовтень 2026' })
    expect(total.textContent).toMatch(/75,00/)
    expect(total).toHaveTextContent('Найбільше — Таксі, 100,0% витрат')
    const items = within(screen.getByRole('region', { name: 'Останні операції' })).getAllByRole('listitem')
    expect(items).toHaveLength(1)
    expect(items[0]).toHaveTextContent('Таксі30 жовтня')
  })

  it('reloads everything when the month changes', async () => {
    vi.useFakeTimers({ toFake: ['Date'], now: new Date(2026, 10, 24, 12) })
    const fetchMock = fakeApi({
      'GET /api/statistics': ({ url }) => {
        const month = url.searchParams.get('month') ?? ''
        return { body: month === '2026-11' ? november : empty(month) }
      },
    })
    renderPage()
    await screen.findByRole('region', { name: 'Витрачено за листопад 2026' })

    fireEvent.click(screen.getByRole('button', { name: 'Попередній місяць' }))

    expect(await screen.findByText('За жовтень 2026 витрат немає')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Додати витрату' })).toHaveAttribute('href', '/expenses')
    expect(screen.queryByText('Останні операції')).not.toBeInTheDocument()
    expect(fetchMock.mock.calls.map(([url]) => String(url))).toContain('/api/statistics?month=2026-10')

    fireEvent.click(screen.getByRole('button', { name: 'Поточний місяць' }))
    expect(await screen.findByText(/Витрачено за/)).toBeInTheDocument()
  })

  it('offers a retry when statistics fail to load', async () => {
    let fail = true
    fakeApi({
      'GET /api/statistics': () => (fail ? problem('Сервер недоступний', 500) : { body: november }),
    })
    renderPage()

    expect(await screen.findByText('Сервер недоступний')).toBeInTheDocument()
    fail = false
    fireEvent.click(screen.getByRole('button', { name: 'Спробувати ще раз' }))

    expect(await screen.findByRole('region', { name: 'Витрачено за листопад 2026' })).toBeInTheDocument()
  })
})
