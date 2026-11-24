import { CircleAlert } from 'lucide-react'
import { useId, type ReactNode } from 'react'
import styles from './Field.module.css'

export interface FieldProps {
  label: string
  hideLabel?: boolean
  hint?: string
  error?: string
}

interface ControlProps {
  id: string
  'aria-invalid': true | undefined
  'aria-describedby': string | undefined
}

interface FieldWrapperProps extends FieldProps {
  id?: string
  children: (control: ControlProps) => ReactNode
}

export function Field({ label, hideLabel, hint, error, id, children }: FieldWrapperProps) {
  const generatedId = useId()
  const controlId = id ?? generatedId
  const messageId = `${controlId}-message`

  return (
    <div className={styles.field}>
      <label className={hideLabel ? 'visually-hidden' : styles.label} htmlFor={controlId}>
        {label}
      </label>
      {children({
        id: controlId,
        'aria-invalid': error ? true : undefined,
        'aria-describedby': error || hint ? messageId : undefined,
      })}
      {error ? (
        <p id={messageId} className={styles.error} role="alert">
          <CircleAlert size={14} strokeWidth={2} aria-hidden="true" />
          {error}
        </p>
      ) : (
        hint && (
          <p id={messageId} className={styles.hint}>
            {hint}
          </p>
        )
      )}
    </div>
  )
}
