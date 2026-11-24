import { ChevronLeft, ChevronRight } from 'lucide-react'
import { useRef } from 'react'
import { currentMonth, formatMonth, shiftMonth } from '../shared/dates.ts'
import { useMonth } from '../shared/monthContext.ts'
import styles from './MonthSwitcher.module.css'

export function MonthSwitcher() {
  const { month, setMonth } = useMonth()
  const previousButton = useRef<HTMLButtonElement>(null)
  const isCurrent = month === currentMonth()

  return (
    <div className={styles.switcher} role="group" aria-label="Місяць">
      <button
        ref={previousButton}
        type="button"
        className={styles.arrow}
        aria-label="Попередній місяць"
        onClick={() => setMonth(shiftMonth(month, -1))}
      >
        <ChevronLeft size={18} aria-hidden="true" />
      </button>
      <span className={styles.label} aria-live="polite">
        {formatMonth(month)}
      </span>
      <button
        type="button"
        className={styles.arrow}
        aria-label="Наступний місяць"
        onClick={() => setMonth(shiftMonth(month, 1))}
      >
        <ChevronRight size={18} aria-hidden="true" />
      </button>
      {!isCurrent && (
        <button
          type="button"
          className={styles.today}
          onClick={() => {
            setMonth(currentMonth())
            previousButton.current?.focus()
          }}
        >
          Поточний місяць
        </button>
      )}
    </div>
  )
}
