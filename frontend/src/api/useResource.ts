import { useCallback, useEffect, useRef, useState } from 'react'
import { ApiError } from './client.ts'

export function errorMessage(error: unknown) {
  return error instanceof ApiError ? error.message : 'Щось пішло не так. Спробуйте ще раз.'
}

interface Result<T> {
  key: string
  data?: T
  error?: string
}

export function useResource<T>(key: string, load: (signal: AbortSignal) => Promise<T>) {
  const [version, setVersion] = useState(0)
  const [result, setResult] = useState<Result<T>>()
  const loadRef = useRef(load)
  const requestKey = `${key}#${version}`

  useEffect(() => {
    loadRef.current = load
  })

  useEffect(() => {
    const controller = new AbortController()
    loadRef.current(controller.signal).then(
      (data) => {
        if (!controller.signal.aborted) setResult({ key: requestKey, data })
      },
      (error: unknown) => {
        if (!controller.signal.aborted) setResult({ key: requestKey, error: errorMessage(error) })
      },
    )
    return () => controller.abort()
  }, [requestKey])

  const reload = useCallback(() => setVersion((current) => current + 1), [])
  const loading = result?.key !== requestKey

  return {
    data: result?.data,
    error: loading ? undefined : result?.error,
    loading,
    reload,
  }
}
