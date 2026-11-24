import { afterEach, describe, expect, it, vi } from 'vitest'
import { ApiError, apiRequest } from './client.ts'

function stubFetch(response: Response) {
  const fetchMock = vi.fn<typeof fetch>(() => Promise.resolve(response))
  vi.stubGlobal('fetch', fetchMock)
  return fetchMock
}

function stubFetchFailure(error: unknown) {
  vi.stubGlobal(
    'fetch',
    vi.fn<typeof fetch>(() => Promise.reject(error)),
  )
}

function json(body: unknown, status = 200, contentType = 'application/json') {
  return new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': contentType },
  })
}

async function catchError(promise: Promise<unknown>): Promise<unknown> {
  try {
    await promise
  } catch (error) {
    return error
  }
  throw new Error('Expected the request to fail')
}

afterEach(() => {
  vi.unstubAllGlobals()
  vi.unstubAllEnvs()
})

describe('apiRequest', () => {
  it('requests the configured base address and returns the parsed body', async () => {
    vi.stubEnv('VITE_API_BASE_URL', 'http://api.test/base')
    const fetchMock = stubFetch(json([{ id: 1, name: 'Їжа' }]))

    const result = await apiRequest('/categories')

    expect(result).toEqual([{ id: 1, name: 'Їжа' }])
    expect(fetchMock).toHaveBeenCalledWith(
      'http://api.test/base/categories',
      expect.objectContaining({ method: 'GET' }),
    )
  })

  it('defaults to the same-origin /api base', async () => {
    vi.stubEnv('VITE_API_BASE_URL', undefined)
    const fetchMock = stubFetch(json([]))

    await apiRequest('/categories')

    expect(fetchMock.mock.calls[0][0]).toBe('/api/categories')
  })

  it('sends the body as JSON', async () => {
    const fetchMock = stubFetch(json({ id: 2 }, 201))

    await apiRequest('/categories', { method: 'POST', body: { name: 'Транспорт' } })

    const init = fetchMock.mock.calls[0][1]
    expect(init?.method).toBe('POST')
    expect(init?.body).toBe('{"name":"Транспорт"}')
    expect(init?.headers).toMatchObject({ 'Content-Type': 'application/json' })
  })

  it('returns nothing for a successful response without a body', async () => {
    stubFetch(new Response(null, { status: 204 }))
    await expect(apiRequest('/expenses/1', { method: 'DELETE' })).resolves.toBeUndefined()

    stubFetch(new Response('', { status: 200 }))
    await expect(apiRequest('/categories/1', { method: 'PUT', body: {} })).resolves.toBeUndefined()
  })

  it('reports a successful response that is not JSON as an ApiError', async () => {
    stubFetch(new Response('<!doctype html>', { status: 200, headers: { 'Content-Type': 'text/html' } }))

    const error = await catchError(apiRequest('/categories'))

    expect(error).toBeInstanceOf(ApiError)
    expect(error).toMatchObject({ status: 200, message: 'Сервер повернув некоректну відповідь.' })
  })

  it('turns Problem Details into an ApiError', async () => {
    const problem = {
      title: 'Validation failed',
      status: 400,
      detail: 'Сума має бути додатною.',
      errors: { amount: ['Сума має бути додатною.'] },
    }
    stubFetch(json(problem, 400, 'application/problem+json; charset=utf-8'))

    const error = await catchError(apiRequest('/expenses', { method: 'POST', body: {} }))

    expect(error).toBeInstanceOf(ApiError)
    expect(error).toMatchObject({
      status: 400,
      message: 'Сума має бути додатною.',
      problem,
    })
  })

  it('does not show the generic title when Problem Details has no detail', async () => {
    stubFetch(json({ title: 'Not Found', status: 404 }, 404, 'application/problem+json'))

    const error = await catchError(apiRequest('/expenses/999'))

    expect(error).toMatchObject({
      status: 404,
      message: 'Сервер повернув помилку (404).',
      problem: { title: 'Not Found' },
    })
  })

  it('reports an error without Problem Details by its status', async () => {
    stubFetch(new Response('<html>Server Error</html>', { status: 500 }))

    const error = await catchError(apiRequest('/categories'))

    expect(error).toBeInstanceOf(ApiError)
    expect(error).toMatchObject({
      status: 500,
      message: 'Сервер повернув помилку (500).',
      problem: undefined,
    })
  })

  it.each([502, 503, 504])('explains that the server is unavailable on %i', async (status) => {
    stubFetch(new Response('<html>Bad Gateway</html>', { status }))

    const error = await catchError(apiRequest('/categories'))

    expect(error).toMatchObject({
      status,
      message: 'Сервер зараз недоступний. Перевірте, що застосунок запущено, і спробуйте ще раз.',
    })
  })

  it('keeps the server detail even when the server is unavailable', async () => {
    stubFetch(json({ title: 'Service Unavailable', status: 503, detail: 'Йде оновлення бази.' }, 503, 'application/problem+json'))

    const error = await catchError(apiRequest('/categories'))

    expect(error).toMatchObject({ status: 503, message: 'Йде оновлення бази.' })
  })

  it('reports an unreachable server as an ApiError with status 0', async () => {
    stubFetchFailure(new TypeError('Failed to fetch'))

    const error = await catchError(apiRequest('/categories'))

    expect(error).toBeInstanceOf(ApiError)
    expect(error).toMatchObject({ status: 0, message: 'Не вдалося з’єднатися із сервером.' })
  })

  it('lets a cancelled request through unchanged', async () => {
    const controller = new AbortController()
    controller.abort()
    const abort: unknown = controller.signal.reason
    stubFetchFailure(abort)

    const error = await catchError(apiRequest('/categories', { signal: controller.signal }))

    expect(error).toBe(abort)
    expect(error).not.toBeInstanceOf(ApiError)
  })

  it('lets a request cancelled while reading the body through unchanged', async () => {
    const controller = new AbortController()
    controller.abort()
    const abort: unknown = controller.signal.reason
    const brokenBody = new ReadableStream({ start: (stream) => stream.error(abort) })
    stubFetch(new Response(brokenBody, { status: 500 }))

    const error = await catchError(apiRequest('/categories', { signal: controller.signal }))

    expect(error).toBe(abort)
  })
})
