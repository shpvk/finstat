import { Calendar } from 'lucide-react'
import type { InputHTMLAttributes } from 'react'
import { Field, type FieldProps } from './Field.tsx'
import styles from './Field.module.css'

interface TextFieldProps
  extends Omit<InputHTMLAttributes<HTMLInputElement>, 'className'>, FieldProps {}

export function TextField({ label, hideLabel, hint, error, id, type, ...input }: TextFieldProps) {
  const isDate = type === 'date'

  return (
    <Field label={label} hideLabel={hideLabel} hint={hint} error={error} id={id}>
      {(control) => (
        <span className={styles.controlWrap}>
          <input
            {...input}
            {...control}
            type={type}
            className={isDate ? `${styles.control} ${styles.date}` : styles.control}
          />
          {isDate && (
            <Calendar
              className={`${styles.adornment} ${styles.dateIcon}`}
              size={16}
              aria-hidden="true"
            />
          )}
        </span>
      )}
    </Field>
  )
}
