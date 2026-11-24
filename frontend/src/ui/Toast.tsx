import { CircleCheck } from 'lucide-react'
import { useCallback, useEffect, useState, type ReactNode } from 'react'
import styles from './Toast.module.css'
import { ToastContext } from './toastContext.ts'

interface Toast {
  id: number
  message: string
}

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toast, setToast] = useState<Toast | null>(null)

  useEffect(() => {
    if (!toast) return
    const timer = setTimeout(() => setToast(null), 3500)
    return () => clearTimeout(timer)
  }, [toast])

  const show = useCallback((message: string) => setToast({ id: Date.now(), message }), [])

  return (
    <ToastContext value={show}>
      {children}
      <div className={styles.region} role="status" aria-live="polite">
        {toast && (
          <p key={toast.id} className={styles.toast}>
            <CircleCheck className={styles.icon} size={16} strokeWidth={2} aria-hidden="true" />
            {toast.message}
          </p>
        )}
      </div>
    </ToastContext>
  )
}
