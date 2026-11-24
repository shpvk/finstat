import { fireEvent, render, screen, within } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import type { Category } from '../../api/categories.ts'
import { fakeApi, problem } from '../../test/fakeApi.ts'
import { ToastProvider } from '../../ui/Toast.tsx'
import { CategoriesPage } from './CategoriesPage.tsx'

function renderPage() {
  render(
    <ToastProvider>
      <CategoriesPage />
    </ToastProvider>,
  )
}

function categoriesApi(initial: Category[]) {
  const categories = [...initial]
  const fetchMock = fakeApi({
    'GET /api/categories': () => ({ body: categories }),
    'POST /api/categories': ({ body }) => {
      const { name } = body as { name: string }
      if (categories.some((c) => c.name.toLowerCase() === name.toLowerCase())) {
        return problem('Категорія з такою назвою вже існує')
      }
      const category = { id: `id-${categories.length + 1}`, name, isArchived: false }
      categories.push(category)
      return { status: 201, body: category }
    },
  })
  return { categories, fetchMock }
}

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('CategoriesPage', () => {
  it('lists active categories and keeps archived ones in a separate section', async () => {
    categoriesApi([
      { id: '1', name: 'Їжа', isArchived: false },
      { id: '2', name: 'Таксі', isArchived: true },
    ])

    renderPage()

    const active = await screen.findByRole('region', { name: /Активні/ })
    expect(within(active).getByText('Їжа')).toBeInTheDocument()
    expect(within(active).queryByText('Таксі')).not.toBeInTheDocument()

    const archive = screen.getByRole('region', { name: /Архів/ })
    expect(within(archive).getByText('Таксі')).toBeInTheDocument()
    expect(within(archive).getByText('В архіві')).toBeInTheDocument()
    expect(within(archive).queryByRole('button')).not.toBeInTheDocument()
  })

  it('adds a category and confirms it', async () => {
    const { fetchMock } = categoriesApi([{ id: '1', name: 'Їжа', isArchived: false }])
    renderPage()
    await screen.findByText('Їжа')

    fireEvent.change(screen.getByLabelText('Нова категорія'), { target: { value: '  Транспорт ' } })
    fireEvent.click(screen.getByRole('button', { name: 'Додати' }))

    expect(await screen.findByText('Категорію «Транспорт» додано')).toBeInTheDocument()
    expect(await screen.findByText('Транспорт')).toBeInTheDocument()
    expect(screen.getByLabelText('Нова категорія')).toHaveValue('')
    const post = fetchMock.mock.calls.find(([, init]) => init?.method === 'POST')
    expect(post?.[1]?.body).toBe('{"name":"Транспорт"}')
  })

  it('checks the name before sending and shows the server error', async () => {
    const { fetchMock } = categoriesApi([{ id: '1', name: 'Їжа', isArchived: false }])
    renderPage()
    await screen.findByText('Їжа')

    fireEvent.click(screen.getByRole('button', { name: 'Додати' }))
    expect(screen.getByText('Вкажіть назву категорії.')).toBeInTheDocument()
    expect(fetchMock.mock.calls.some(([, init]) => init?.method === 'POST')).toBe(false)

    fireEvent.change(screen.getByLabelText('Нова категорія'), { target: { value: 'їжа' } })
    fireEvent.click(screen.getByRole('button', { name: 'Додати' }))

    expect(await screen.findByText('Категорія з такою назвою вже існує')).toBeInTheDocument()
    expect(screen.getByLabelText('Нова категорія')).toHaveAttribute('aria-invalid', 'true')
  })

  it('renames a category in place', async () => {
    const categories: Category[] = [{ id: '1', name: 'Їжа', isArchived: false }]
    const fetchMock = fakeApi({
      'GET /api/categories': () => ({ body: categories }),
      'PUT /api/categories/1': ({ body }) => {
        categories[0] = { ...categories[0], name: (body as { name: string }).name }
        return { body: categories[0] }
      },
    })
    renderPage()

    fireEvent.click(await screen.findByRole('button', { name: 'Перейменувати «Їжа»' }))
    fireEvent.change(screen.getByLabelText('Нова назва для «Їжа»'), {
      target: { value: 'Продукти' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Зберегти' }))

    expect(await screen.findByText('Категорію перейменовано на «Продукти»')).toBeInTheDocument()
    expect(await screen.findByText('Продукти')).toBeInTheDocument()
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/categories/1',
      expect.objectContaining({ method: 'PUT', body: '{"name":"Продукти"}' }),
    )
  })

  it('archives a category only after confirmation', async () => {
    const categories: Category[] = [{ id: '1', name: 'Їжа', isArchived: false }]
    const fetchMock = fakeApi({
      'GET /api/categories': () => ({ body: categories }),
      'POST /api/categories/1/archive': () => {
        categories[0] = { ...categories[0], isArchived: true }
        return { status: 204 }
      },
    })
    renderPage()

    fireEvent.click(await screen.findByRole('button', { name: 'Архівувати «Їжа»' }))
    const confirm = screen.getByRole('group', { name: 'Архівування «Їжа»' })
    expect(fetchMock.mock.calls.some(([url]) => String(url).endsWith('/archive'))).toBe(false)

    fireEvent.click(within(confirm).getByRole('button', { name: 'Скасувати' }))
    expect(screen.queryByRole('group', { name: 'Архівування «Їжа»' })).not.toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: 'Архівувати «Їжа»' }))
    fireEvent.click(
      within(screen.getByRole('group', { name: 'Архівування «Їжа»' })).getByRole('button', {
        name: 'Архівувати',
      }),
    )

    expect(await screen.findByText('Категорію «Їжа» перенесено в архів')).toBeInTheDocument()
    const archive = await screen.findByRole('region', { name: /Архів/ })
    expect(within(archive).getByText('Їжа')).toBeInTheDocument()
  })

  it('offers a retry when the list fails to load', async () => {
    let fail = true
    fakeApi({
      'GET /api/categories': () =>
        fail ? problem('Сервер недоступний', 500) : { body: [{ id: '1', name: 'Їжа', isArchived: false }] },
    })
    renderPage()

    expect(await screen.findByText('Сервер недоступний')).toBeInTheDocument()
    fail = false
    fireEvent.click(screen.getByRole('button', { name: 'Спробувати ще раз' }))

    expect(await screen.findByText('Їжа')).toBeInTheDocument()
  })

  it('explains that there are no active categories', async () => {
    categoriesApi([{ id: '2', name: 'Таксі', isArchived: true }])
    renderPage()

    expect(await screen.findByText('Активних категорій немає')).toBeInTheDocument()
    expect(screen.getByRole('region', { name: /Архів/ })).toBeInTheDocument()
  })

  it('keeps the rename form open with the server error', async () => {
    fakeApi({
      'GET /api/categories': () => ({ body: [{ id: '1', name: 'Їжа', isArchived: false }] }),
      'PUT /api/categories/1': () => problem('Категорія з такою назвою вже існує'),
    })
    renderPage()

    fireEvent.click(await screen.findByRole('button', { name: 'Перейменувати «Їжа»' }))
    fireEvent.change(screen.getByLabelText('Нова назва для «Їжа»'), {
      target: { value: 'Комунальні послуги' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Зберегти' }))

    expect(await screen.findByRole('alert')).toHaveTextContent('Категорія з такою назвою вже існує')
    expect(screen.getByLabelText('Нова назва для «Їжа»')).toHaveValue('Комунальні послуги')
  })

  it('cancels renaming with Escape and returns focus to the row', async () => {
    const fetchMock = fakeApi({
      'GET /api/categories': () => ({ body: [{ id: '1', name: 'Їжа', isArchived: false }] }),
    })
    renderPage()

    fireEvent.click(await screen.findByRole('button', { name: 'Перейменувати «Їжа»' }))
    const input = screen.getByLabelText('Нова назва для «Їжа»')
    fireEvent.change(input, { target: { value: 'Продукти' } })
    fireEvent.keyDown(input, { key: 'Escape' })

    expect(screen.queryByLabelText('Нова назва для «Їжа»')).not.toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Перейменувати «Їжа»' })).toHaveFocus()
    expect(fetchMock.mock.calls.some(([, init]) => init?.method === 'PUT')).toBe(false)
  })

  it('shows why archiving failed and keeps the category active', async () => {
    fakeApi({
      'GET /api/categories': () => ({ body: [{ id: '1', name: 'Їжа', isArchived: false }] }),
      'POST /api/categories/1/archive': () => problem('Категорію не знайдено', 404),
    })
    renderPage()

    fireEvent.click(await screen.findByRole('button', { name: 'Архівувати «Їжа»' }))
    const confirm = screen.getByRole('group', { name: 'Архівування «Їжа»' })
    expect(within(confirm).getByRole('button', { name: 'Скасувати' })).toHaveFocus()
    fireEvent.click(within(confirm).getByRole('button', { name: 'Архівувати' }))

    expect(await within(confirm).findByRole('alert')).toHaveTextContent('Категорію не знайдено')
    expect(screen.queryByRole('region', { name: /Архів/ })).not.toBeInTheDocument()
  })
})
