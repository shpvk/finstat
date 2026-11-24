import { Pencil, Trash2 } from 'lucide-react'
import { useState } from 'react'
import { Link } from 'react-router'
import { listCategories, type Category } from '../../api/categories.ts'
import {
  createExpense,
  deleteExpense,
  listExpenses,
  updateExpense,
  type Expense,
  type ExpenseInput,
} from '../../api/expenses.ts'
import { errorMessage, useResource } from '../../api/useResource.ts'
import { formatDay, formatMonth, formatWeekday, monthOf } from '../../shared/dates.ts'
import { formatMoney } from '../../shared/money.ts'
import { useMonth } from '../../shared/monthContext.ts'
import { Button } from '../../ui/Button.tsx'
import { MonthSwitcher } from '../../ui/MonthSwitcher.tsx'
import { PageHeader } from '../../ui/PageHeader.tsx'
import { useToast } from '../../ui/toastContext.ts'
import { EmptyState, ErrorState, LoadingState } from '../../ui/states.tsx'
import { ExpenseForm } from './ExpenseForm.tsx'
import styles from './ExpensesPage.module.css'

const plural = new Intl.PluralRules('uk-UA')
const expenseWords: Record<string, string> = {
  one: 'витрата',
  few: 'витрати',
  many: 'витрат',
  other: 'витрати',
}

function countLabel(count: number) {
  return `${count} ${expenseWords[plural.select(count)]}`
}

type RowAction = 'edit' | 'delete'

export function ExpensesPage() {
  const toast = useToast()
  const { month, setMonth } = useMonth()
  const categories = useResource('categories', (signal) => listCategories(true, signal))
  const expenses = useResource(`expenses:${month}`, (signal) => listExpenses(month, signal))
  const [selected, setSelected] = useState<{ id: string; action: RowAction } | null>(null)

  const categoryList = categories.data ?? []
  const hasActive = categoryList.some((category) => !category.isArchived)
  const categoryById = new Map(categoryList.map((category) => [category.id, category]))

  async function create(input: ExpenseInput) {
    await createExpense(input)
    const expenseMonth = monthOf(input.date)
    if (expenseMonth === month) {
      toast('Витрату додано')
      expenses.reload()
    } else {
      toast(`Витрату додано в ${formatMonth(expenseMonth).toLowerCase()}`)
      setMonth(expenseMonth)
    }
  }

  return (
    <>
      <PageHeader title="Витрати" description="Записуйте витрати та переглядайте їх за місяцями" />
      <section className={styles.panel} aria-labelledby="new-expense">
        <h2 id="new-expense" className={styles.panelTitle}>
          Нова витрата
        </h2>
        {categories.error ? (
          <ErrorState message={categories.error} onRetry={categories.reload} />
        ) : !categories.data ? (
          <LoadingState />
        ) : hasActive ? (
          <ExpenseForm categories={categoryList} submitLabel="Додати" onSubmit={create} />
        ) : (
          <p className={styles.note}>
            Щоб записати витрату, спершу <Link to="/categories">додайте категорію</Link>.
          </p>
        )}
      </section>

      <section aria-labelledby="month-expenses">
        <div className={styles.toolbar}>
          <h2 id="month-expenses" className="visually-hidden">
            Витрати за {formatMonth(month).toLowerCase()}
          </h2>
          <MonthSwitcher />
          {expenses.data && !expenses.loading && (
            <span className={styles.count}>{countLabel(expenses.data.length)}</span>
          )}
        </div>
        {expenses.error ? (
          <ErrorState message={expenses.error} onRetry={expenses.reload} />
        ) : !expenses.data || (expenses.loading && expenses.data.length === 0) ? (
          <LoadingState />
        ) : expenses.data.length === 0 ? (
          <EmptyState
            title="Витрат за цей місяць немає"
            description="Додайте витрату у формі вище, і вона з’явиться в цьому списку."
          />
        ) : (
          <table className={styles.table} aria-busy={expenses.loading || undefined}>
            <thead>
              <tr>
                <th scope="col">Дата</th>
                <th scope="col">Категорія</th>
                <th scope="col">Коментар</th>
                <th scope="col" className={styles.amountHead}>
                  Сума
                </th>
                <th scope="col">
                  <span className="visually-hidden">Дії</span>
                </th>
              </tr>
            </thead>
            <tbody>
              {expenses.data.map((expense) => (
                <ExpenseRow
                  key={expense.id}
                  expense={expense}
                  categories={categoryList}
                  category={categoryById.get(expense.categoryId)}
                  action={selected?.id === expense.id ? selected.action : null}
                  onActionChange={(action) =>
                    setSelected(action ? { id: expense.id, action } : null)
                  }
                  onChanged={expenses.reload}
                />
              ))}
            </tbody>
          </table>
        )}
      </section>
    </>
  )
}

interface ExpenseRowProps {
  expense: Expense
  categories: Category[]
  category: Category | undefined
  action: RowAction | null
  onActionChange: (action: RowAction | null) => void
  onChanged: () => void
}

function ExpenseRow({
  expense,
  categories,
  category,
  action,
  onActionChange,
  onChanged,
}: ExpenseRowProps) {
  const toast = useToast()
  const close = () => onActionChange(null)
  const amount = formatMoney(expense.amount)

  if (action === 'edit') {
    return (
      <tr className={styles.expanded}>
        <td colSpan={5}>
          <ExpenseForm
            categories={categories}
            expense={expense}
            submitLabel="Зберегти"
            onCancel={close}
            onSubmit={async (input) => {
              await updateExpense(expense.id, input)
              toast('Зміни збережено')
              close()
              onChanged()
            }}
          />
        </td>
      </tr>
    )
  }

  if (action === 'delete') {
    return (
      <tr className={styles.expanded}>
        <td colSpan={5}>
          <DeleteConfirm expense={expense} amount={amount} onDone={close} onChanged={onChanged} />
        </td>
      </tr>
    )
  }

  return (
    <tr>
      <td className={styles.date}>
        {formatDay(expense.date)}
        <span className={styles.weekday}>{formatWeekday(expense.date)}</span>
      </td>
      <td>
        {category?.name ?? 'Невідома категорія'}
        {category?.isArchived && <span className={styles.badge}>в архіві</span>}
      </td>
      <td className={styles.comment}>{expense.comment ?? '—'}</td>
      <td className={styles.amount}>{amount}</td>
      <td className={styles.actions}>
        <Button
          variant="ghost"
          aria-label={`Редагувати витрату ${amount} від ${formatDay(expense.date)}`}
          onClick={() => onActionChange('edit')}
        >
          <Pencil size={16} aria-hidden="true" />
        </Button>
        <Button
          variant="ghost"
          aria-label={`Видалити витрату ${amount} від ${formatDay(expense.date)}`}
          onClick={() => onActionChange('delete')}
        >
          <Trash2 size={16} aria-hidden="true" />
        </Button>
      </td>
    </tr>
  )
}

interface DeleteConfirmProps {
  expense: Expense
  amount: string
  onDone: () => void
  onChanged: () => void
}

function DeleteConfirm({ expense, amount, onDone, onChanged }: DeleteConfirmProps) {
  const toast = useToast()
  const [error, setError] = useState<string>()
  const [saving, setSaving] = useState(false)

  async function confirm() {
    setSaving(true)
    try {
      await deleteExpense(expense.id)
      toast('Витрату видалено')
      onDone()
      onChanged()
    } catch (failure) {
      setError(errorMessage(failure))
      setSaving(false)
    }
  }

  return (
    <div
      className={styles.confirm}
      role="group"
      aria-label="Видалення витрати"
      onKeyDown={(event) => {
        if (event.key === 'Escape') onDone()
      }}
    >
      <div className={styles.grow}>
        <p className={styles.confirmTitle}>
          Видалити витрату {amount} від {formatDay(expense.date)}?
        </p>
        <p className={error ? styles.error : styles.note} role={error ? 'alert' : undefined}>
          {error ?? 'Її буде прибрано з історії та статистики.'}
        </p>
      </div>
      <Button variant="danger" loading={saving} onClick={confirm} autoFocus>
        Видалити
      </Button>
      <Button onClick={onDone}>Скасувати</Button>
    </div>
  )
}
