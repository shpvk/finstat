import { Pencil, Trash2 } from 'lucide-react'
import { useEffect, useId, useRef, useState } from 'react'
import { Link } from 'react-router'
import { listCategories, type Category } from '../../api/categories.ts'
import { errorMessage } from '../../api/client.ts'
import {
  createExpense,
  deleteExpense,
  listExpenses,
  updateExpense,
  type Expense,
  type ExpenseInput,
} from '../../api/expenses.ts'
import { useResource } from '../../api/useResource.ts'
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

function byDateDesc(list: Expense[]) {
  return [...list].sort((a, b) => (a.date < b.date ? 1 : a.date > b.date ? -1 : 0))
}

type RowAction = 'edit' | 'delete'

export function ExpensesPage() {
  const toast = useToast()
  const { month, setMonth } = useMonth()
  const categories = useResource('categories', (signal) => listCategories(true, signal))
  const expenses = useResource(`expenses:${month}`, (signal) => listExpenses(month, signal))
  const [selected, setSelected] = useState<{ id: string; action: RowAction } | null>(null)
  const newExpense = useRef<HTMLElement>(null)
  const table = useRef<HTMLTableElement>(null)
  const focusRow = useRef<number | null>(null)

  const categoryList = categories.data ?? []
  const hasActive = categoryList.some((category) => !category.isArchived)
  const categoryById = new Map(categoryList.map((category) => [category.id, category]))

  useEffect(() => {
    if (focusRow.current === null) return
    const rows = table.current?.querySelectorAll<HTMLTableRowElement>('tbody tr') ?? []
    const row = rows[Math.min(focusRow.current, rows.length - 1)]
    const target = row?.querySelector('button') ?? newExpense.current?.querySelector('input')
    target?.focus()
    focusRow.current = null
  }, [expenses.data])

  async function create(input: ExpenseInput) {
    const created = await createExpense(input)
    const expenseMonth = monthOf(created.date)
    if (expenseMonth === month) {
      toast('Витрату додано')
      expenses.mutate((list) => byDateDesc([created, ...list]))
      expenses.reload()
    } else {
      toast(`Витрату додано за ${formatMonth(expenseMonth).toLowerCase()}`)
      setMonth(expenseMonth)
    }
  }

  function updated(saved: Expense, index: number) {
    if (monthOf(saved.date) === month) {
      toast('Зміни збережено')
      expenses.mutate((list) =>
        byDateDesc(list.map((item) => (item.id === saved.id ? saved : item))),
      )
    } else {
      toast(`Витрату перенесено на ${formatMonth(monthOf(saved.date)).toLowerCase()}`)
      focusRow.current = index
      expenses.mutate((list) => list.filter((item) => item.id !== saved.id))
    }
    expenses.reload()
  }

  function deleted(id: string, index: number) {
    focusRow.current = index
    expenses.mutate((list) => list.filter((item) => item.id !== id))
    expenses.reload()
  }

  return (
    <>
      <PageHeader title="Витрати" description="Записуйте витрати та переглядайте їх за місяцями" />
      <section ref={newExpense} className={styles.panel} aria-labelledby="new-expense">
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
            {categoryList.length > 0 ? 'Усі категорії в архіві. ' : ''}
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
          {expenses.data && expenses.data.length > 0 && (
            <span className={styles.count}>{countLabel(expenses.data.length)}</span>
          )}
        </div>
        {expenses.error ? (
          <ErrorState message={expenses.error} onRetry={expenses.reload} />
        ) : !expenses.data ? (
          <LoadingState />
        ) : expenses.data.length === 0 ? (
          <EmptyState
            level={3}
            title="Витрат за цей місяць немає"
            description="Додайте витрату у формі вище, і вона з’явиться в цьому списку."
          />
        ) : (
          <table ref={table} className={styles.table} aria-busy={expenses.loading || undefined}>
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
              {expenses.data.map((expense, index) => (
                <ExpenseRow
                  key={expense.id}
                  expense={expense}
                  categories={categories.data}
                  category={categoryById.get(expense.categoryId)}
                  action={selected?.id === expense.id ? selected.action : null}
                  onOpen={(action) => setSelected({ id: expense.id, action })}
                  onClose={() =>
                    setSelected((current) => (current?.id === expense.id ? null : current))
                  }
                  onUpdated={(saved) => updated(saved, index)}
                  onDeleted={() => deleted(expense.id, index)}
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
  categories: Category[] | undefined
  category: Category | undefined
  action: RowAction | null
  onOpen: (action: RowAction) => void
  onClose: () => void
  onUpdated: (expense: Expense) => void
  onDeleted: () => void
}

function ExpenseRow({
  expense,
  categories,
  category,
  action,
  onOpen,
  onClose,
  onUpdated,
  onDeleted,
}: ExpenseRowProps) {
  const editButton = useRef<HTMLButtonElement>(null)
  const deleteButton = useRef<HTMLButtonElement>(null)
  const opened = useRef<RowAction | null>(null)
  const amount = formatMoney(expense.amount)
  const day = formatDay(expense.date)

  useEffect(() => {
    if (action) {
      opened.current = action
      return
    }
    if (opened.current) {
      const opener = opened.current === 'edit' ? editButton : deleteButton
      if (!document.activeElement || document.activeElement === document.body) {
        opener.current?.focus()
      }
      opened.current = null
    }
  }, [action])

  if (action === 'edit' && categories) {
    return (
      <tr className={styles.expanded}>
        <td colSpan={5}>
          <ExpenseForm
            categories={categories}
            expense={expense}
            submitLabel="Зберегти"
            onCancel={onClose}
            onSubmit={async (input) => {
              const saved = await updateExpense(expense.id, input)
              if (monthOf(saved.date) !== monthOf(expense.date)) opened.current = null
              onClose()
              onUpdated(saved)
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
          <DeleteConfirm
            expense={expense}
            label={`${amount} від ${day}`}
            onCancel={onClose}
            onDeleted={() => {
              opened.current = null
              onClose()
              onDeleted()
            }}
          />
        </td>
      </tr>
    )
  }

  return (
    <tr>
      <td className={styles.date}>
        {day}{' '}
        <span className={styles.weekday}>{formatWeekday(expense.date)}</span>
      </td>
      <td>
        {!categories ? '…' : (category?.name ?? 'Невідома категорія')}
        {category?.isArchived && (
          <>
            {' '}
            <span className={styles.badge}>в архіві</span>
          </>
        )}
      </td>
      <td className={styles.comment}>{expense.comment ?? '—'}</td>
      <td className={styles.amount}>{amount}</td>
      <td className={styles.actions}>
        <Button
          ref={editButton}
          variant="ghost"
          disabled={!categories}
          aria-label={`Редагувати витрату ${amount} від ${day}`}
          onClick={() => onOpen('edit')}
        >
          <Pencil size={16} aria-hidden="true" />
        </Button>
        <Button
          ref={deleteButton}
          variant="ghost"
          aria-label={`Видалити витрату ${amount} від ${day}`}
          onClick={() => onOpen('delete')}
        >
          <Trash2 size={16} aria-hidden="true" />
        </Button>
      </td>
    </tr>
  )
}

interface DeleteConfirmProps {
  expense: Expense
  label: string
  onCancel: () => void
  onDeleted: () => void
}

function DeleteConfirm({ expense, label, onCancel, onDeleted }: DeleteConfirmProps) {
  const toast = useToast()
  const [error, setError] = useState<string>()
  const [saving, setSaving] = useState(false)
  const noteId = useId()

  async function confirm() {
    setSaving(true)
    try {
      await deleteExpense(expense.id)
      toast('Витрату видалено')
      onDeleted()
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
      aria-describedby={noteId}
      onKeyDown={(event) => {
        if (event.key === 'Escape' && !saving) onCancel()
      }}
    >
      <div className={styles.grow}>
        <p className={styles.confirmTitle}>Видалити витрату {label}?</p>
        <p id={noteId} className={styles.note}>
          Її буде прибрано з історії та статистики.
        </p>
        {error && (
          <p className={styles.error} role="alert">
            {error}
          </p>
        )}
      </div>
      <Button variant="danger" loading={saving} onClick={confirm}>
        Видалити
      </Button>
      <Button onClick={onCancel} disabled={saving} autoFocus>
        Скасувати
      </Button>
    </div>
  )
}
