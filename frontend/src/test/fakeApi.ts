import { vi } from 'vitest'

interface FakeResponse {
  status?: number
  body?: unknown
}

type Handler = (request: { body: unknown; url: URL }) => FakeResponse

export function fakeApi(routes: Record<string, Handler>) {
  const fetchMock = vi.fn<typeof fetch>(async (input, init) => {
    const url = new URL(String(input), 'http://localhost')
    const handler = routes[`${init?.method ?? 'GET'} ${url.pathname}`]
    const { status = 200, body } = handler
      ? handler({ body: init?.body ? JSON.parse(String(init.body)) : undefined, url })
      : { status: 404, body: { title: 'Not Found', status: 404 } }
    if (body === undefined) return new Response(null, { status })
    return new Response(JSON.stringify(body), {
      status,
      headers: { 'Content-Type': status >= 400 ? 'application/problem+json' : 'application/json' },
    })
  })
  vi.stubGlobal('fetch', fetchMock)
  return fetchMock
}

export function problem(detail: string, status = 400): FakeResponse {
  return { status, body: { title: 'Порушено бізнес-правило', status, detail } }
}
