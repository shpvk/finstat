import { CircleAlert, Inbox } from 'lucide-react'
import type { ReactNode } from 'react'
import { Button } from './Button.tsx'
import styles from './states.module.css'

export function LoadingState() {
  return (
    <div className={styles.loading} role="status">
      <span className="visually-hidden">Завантаження…</span>
      <div className={styles.lines} aria-hidden="true">
        <span className={styles.skeleton} />
        <span className={styles.skeleton} />
        <span className={styles.skeleton} />
      </div>
    </div>
  )
}

interface EmptyStateProps {
  title: string
  description?: string
  action?: ReactNode
  level?: 2 | 3
}

export function EmptyState({ title, description, action, level = 2 }: EmptyStateProps) {
  const Heading = level === 3 ? 'h3' : 'h2'

  return (
    <div className={styles.panel}>
      <Inbox className={styles.icon} size={24} strokeWidth={1.75} aria-hidden="true" />
      <Heading className={styles.title}>{title}</Heading>
      {description && <p className={styles.description}>{description}</p>}
      {action && <div className={styles.action}>{action}</div>}
    </div>
  )
}

interface ErrorStateProps {
  message: string
  onRetry?: () => void
}

export function ErrorState({ message, onRetry }: ErrorStateProps) {
  return (
    <div className={styles.panel} role="alert">
      <CircleAlert className={styles.errorIcon} size={24} strokeWidth={1.75} aria-hidden="true" />
      <h2 className={styles.title}>Помилка завантаження</h2>
      <p className={styles.description}>{message}</p>
      {onRetry && (
        <div className={styles.action}>
          <Button onClick={onRetry}>Спробувати ще раз</Button>
        </div>
      )}
    </div>
  )
}
