import { useCallback, useEffect, useRef, useState } from 'react'
import { errorMessage } from './client.ts'

interface Result<T> {
  key: string
  version: number
  data?: T
  error?: string
}

export function useResource<T>(key: string, load: (signal: AbortSignal) => Promise<T>) {
  const [version, setVersion] = useState(0)
  const [result, setResult] = useState<Result<T>>()
  const loadRef = useRef(load)

  useEffect(() => {
    loadRef.current = load
  })

  useEffect(() => {
    const controller = new AbortController()
    loadRef.current(controller.signal).then(
      (data) => {
        if (!controller.signal.aborted) setResult({ key, version, data })
      },
      (error: unknown) => {
        if (!controller.signal.aborted) setResult({ key, version, error: errorMessage(error) })
      },
    )
    return () => controller.abort()
  }, [key, version])

  const reload = useCallback(() => setVersion((current) => current + 1), [])
  const mutate = useCallback((update: (data: T) => T) => {
    setResult((current) =>
      current?.data === undefined ? current : { ...current, data: update(current.data) },
    )
  }, [])

  const sameKey = result?.key === key
  const loading = !sameKey || result.version !== version

  return {
    data: sameKey ? result.data : undefined,
    error: loading ? undefined : result?.error,
    loading,
    reload,
    mutate,
  }
}
