import { ChevronDown } from 'lucide-react'
import type { SelectHTMLAttributes } from 'react'
import { Field, type FieldProps } from './Field.tsx'
import styles from './Field.module.css'

interface SelectProps
  extends Omit<SelectHTMLAttributes<HTMLSelectElement>, 'className'>, FieldProps {}

export function Select({ label, hideLabel, hint, error, id, children, ...select }: SelectProps) {
  return (
    <Field label={label} hideLabel={hideLabel} hint={hint} error={error} id={id}>
      {(control) => (
        <span className={styles.controlWrap}>
          <select {...select} {...control} className={`${styles.control} ${styles.select}`}>
            {children}
          </select>
          <ChevronDown className={styles.adornment} size={16} aria-hidden="true" />
        </span>
      )}
    </Field>
  )
}
