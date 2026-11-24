export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  instance?: string
  errors?: Record<string, string[]>
}

export class ApiError extends Error {
  readonly status: number
  readonly problem: ProblemDetails | undefined

  constructor(message: string, status: number, problem?: ProblemDetails) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.problem = problem
  }
}

interface RequestOptions {
  method?: 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE'
  body?: unknown
  signal?: AbortSignal
}

export async function apiRequest<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const { method = 'GET', body, signal } = options
  const baseUrl = import.meta.env.VITE_API_BASE_URL ?? '/api'

  let response: Response
  let text: string
  try {
    response = await fetch(`${baseUrl}${path}`, {
      method,
      signal,
      headers: {
        Accept: 'application/json',
        ...(body !== undefined && { 'Content-Type': 'application/json' }),
      },
      body: body === undefined ? undefined : JSON.stringify(body),
    })
    text = await response.text()
  } catch (error) {
    if (signal?.aborted) throw error
    throw new ApiError('Не вдалося з’єднатися із сервером.', 0)
  }

  if (!response.ok) {
    const isProblem = response.headers.get('Content-Type')?.includes('application/problem+json')
    const problem = isProblem ? parseJson<ProblemDetails>(text) : undefined
    const unavailable = [502, 503, 504].includes(response.status)
    const message =
      problem?.detail ??
      (unavailable
        ? 'Сервер зараз недоступний. Перевірте, що застосунок запущено, і спробуйте ще раз.'
        : `Сервер повернув помилку (${response.status}).`)
    throw new ApiError(message, response.status, problem)
  }

  if (text === '') return undefined as T
  const data = parseJson<T>(text)
  if (data === undefined) {
    throw new ApiError('Сервер повернув некоректну відповідь.', response.status)
  }
  return data
}

export function errorMessage(error: unknown) {
  return error instanceof ApiError ? error.message : 'Щось пішло не так. Спробуйте ще раз.'
}

function parseJson<T>(text: string): T | undefined {
  try {
    return JSON.parse(text) as T
  } catch {
    return undefined
  }
}
