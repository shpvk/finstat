import { ArrowRight } from 'lucide-react'
import { Link } from 'react-router'
import { getStatistics, type MonthStatistics } from '../../api/statistics.ts'
import { useResource } from '../../api/useResource.ts'
import { formatDay, formatMonth } from '../../shared/dates.ts'
import { compareAmounts, formatMoney, formatShare } from '../../shared/money.ts'
import { useMonth } from '../../shared/monthContext.ts'
import { ButtonLink } from '../../ui/Button.tsx'
import { MonthSwitcher } from '../../ui/MonthSwitcher.tsx'
import { PageHeader } from '../../ui/PageHeader.tsx'
import { EmptyState, ErrorState, LoadingState } from '../../ui/states.tsx'
import styles from './OverviewPage.module.css'

const plural = new Intl.PluralRules('uk-UA')
const categoryWords: Record<string, string> = {
  one: 'категорія',
  few: 'категорії',
  many: 'категорій',
  other: 'категорії',
}

export function OverviewPage() {
  const { month } = useMonth()
  const statistics = useResource(`statistics:${month}`, (signal) => getStatistics(month, signal))
  const monthName = formatMonth(month).toLowerCase()

  return (
    <>
      <PageHeader title="Огляд" description="Скільки витрачено за місяць і на що" action={<MonthSwitcher />} />
      {statistics.error ? (
        <ErrorState message={statistics.error} onRetry={statistics.reload} />
      ) : !statistics.data ? (
        <LoadingState />
      ) : statistics.data.categories.length === 0 ? (
        <EmptyState
          title={`За ${monthName} витрат немає`}
          description="Запишіть першу витрату, і тут з’являться підсумки місяця."
          action={
            <ButtonLink to="/expenses" variant="primary">
              Додати витрату
            </ButtonLink>
          }
        />
      ) : (
        <Summary statistics={statistics.data} monthName={monthName} />
      )}
    </>
  )
}

function Summary({ statistics, monthName }: { statistics: MonthStatistics; monthName: string }) {
  const byTotal = [...statistics.categories].sort((a, b) => compareAmounts(b.total, a.total))
  const top = byTotal[0]
  const names = new Map(statistics.categories.map((c) => [c.categoryId, c.name]))
  const count = statistics.categories.length

  return (
    <div className={styles.summary}>
      <section className={styles.card} aria-labelledby="month-total">
        <h2 id="month-total" className={styles.cardLabel}>
          Витрачено за {monthName}
        </h2>
        <p className={styles.total}>{formatMoney(statistics.total)}</p>
        <p className={styles.meta}>
          {count} {categoryWords[plural.select(count)]}
        </p>
        <p className={styles.top}>
          Найбільше — <strong>{top.name}</strong>, {formatShare(top.share)} витрат
        </p>
      </section>
      <section className={styles.card} aria-labelledby="recent">
        <div className={styles.cardHeader}>
          <h2 id="recent" className={styles.cardTitle}>
            Останні операції
          </h2>
          <Link to="/expenses" className={styles.more}>
            Усі витрати
            <ArrowRight size={16} aria-hidden="true" />
          </Link>
        </div>
        <ul className={styles.recent}>
          {statistics.recent.map((expense) => (
            <li key={expense.id} className={styles.operation}>
              <span className={styles.operationText}>
                <span className={styles.operationName}>
                  {names.get(expense.categoryId) ?? 'Невідома категорія'}
                </span>
                <span className={styles.operationMeta}>
                  {formatDay(expense.date)}
                  {expense.comment && ` · ${expense.comment}`}
                </span>
              </span>
              <span className={styles.operationAmount}>{formatMoney(expense.amount)}</span>
            </li>
          ))}
        </ul>
      </section>
    </div>
  )
}
