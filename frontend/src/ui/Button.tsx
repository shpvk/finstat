import { LoaderCircle } from 'lucide-react'
import type { ComponentProps } from 'react'
import { Link } from 'react-router'
import styles from './Button.module.css'

type Variant = 'primary' | 'secondary' | 'danger' | 'ghost'

export function ButtonLink({
  variant = 'secondary',
  ...link
}: Omit<ComponentProps<typeof Link>, 'className'> & { variant?: Variant }) {
  return <Link {...link} className={`${styles.button} ${styles[variant]}`} />
}

interface ButtonProps extends Omit<ComponentProps<'button'>, 'className'> {
  variant?: Variant
  loading?: boolean
}

export function Button({
  variant = 'secondary',
  loading = false,
  type = 'button',
  onClick,
  children,
  ...rest
}: ButtonProps) {
  return (
    <button
      {...rest}
      type={type}
      className={`${styles.button} ${styles[variant]}`}
      aria-disabled={loading || undefined}
      aria-busy={loading || undefined}
      onClick={(event) => {
        if (loading) {
          event.preventDefault()
          return
        }
        onClick?.(event)
      }}
    >
      {loading && <LoaderCircle className={styles.spinner} size={16} aria-hidden="true" />}
      {children}
    </button>
  )
}
