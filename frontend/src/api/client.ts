/** Problem Details (RFC 9457) as returned by the ASP.NET Core API. */
export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  instance?: string
  /** Validation errors: field name to messages. */
  errors?: Record<string, string[]>
}

/** The only failure the API client reports; `status` is 0 when the response could not be received in full. */
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
  /** Sent as JSON. */
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
    // A cancelled request is not a failure: let the caller ignore it.
    if (signal?.aborted) throw error
    throw new ApiError('Не вдалося з’єднатися із сервером.', 0)
  }

  if (!response.ok) {
    const isProblem = response.headers.get('Content-Type')?.includes('application/problem+json')
    const problem = isProblem ? parseJson<ProblemDetails>(text) : undefined
    // `title` is a generic English status label in ASP.NET Core, so only `detail` is shown to the user.
    const message = problem?.detail ?? `Сервер повернув помилку (${response.status}).`
    throw new ApiError(message, response.status, problem)
  }

  if (text === '') return undefined as T
  const data = parseJson<T>(text)
  if (data === undefined) {
    throw new ApiError('Сервер повернув некоректну відповідь.', response.status)
  }
  return data
}

/** Returns `undefined` for invalid JSON; valid JSON never parses to `undefined`. */
function parseJson<T>(text: string): T | undefined {
  try {
    return JSON.parse(text) as T
  } catch {
    return undefined
  }
}
