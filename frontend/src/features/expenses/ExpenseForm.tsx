import { useState, type FormEvent } from 'react'
import type { Category } from '../../api/categories.ts'
import type { Expense, ExpenseInput } from '../../api/expenses.ts'
import { errorMessage } from '../../api/client.ts'
import { todayIso } from '../../shared/dates.ts'
import { amountToInput, parseAmount } from '../../shared/money.ts'
import { Button } from '../../ui/Button.tsx'
import { Select } from '../../ui/Select.tsx'
import { TextField } from '../../ui/TextField.tsx'
import styles from './ExpenseForm.module.css'

const maxCommentLength = 200

interface ExpenseFormProps {
  categories: Category[]
  expense?: Expense
  submitLabel: string
  onSubmit: (input: ExpenseInput) => Promise<void>
  onCancel?: () => void
}

interface Errors {
  amount?: string
  categoryId?: string
  date?: string
  form?: string
}

export function ExpenseForm({ categories, expense, submitLabel, onSubmit, onCancel }: ExpenseFormProps) {
  const [amount, setAmount] = useState(expense ? amountToInput(expense.amount) : '')
  const [categoryId, setCategoryId] = useState(expense?.categoryId ?? '')
  const [date, setDate] = useState(expense?.date ?? todayIso())
  const [comment, setComment] = useState(expense?.comment ?? '')
  const [errors, setErrors] = useState<Errors>({})
  const [saving, setSaving] = useState(false)

  const options = categories.filter(
    (category) => !category.isArchived || category.id === expense?.categoryId,
  )

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const parsed = parseAmount(amount)
    const found: Errors = {
      amount: 'error' in parsed ? parsed.error : undefined,
      categoryId: categoryId ? undefined : 'Оберіть категорію.',
      date: date ? undefined : 'Вкажіть дату.',
    }
    if (found.amount || found.categoryId || found.date || 'error' in parsed) {
      setErrors(found)
      return
    }
    setSaving(true)
    try {
      await onSubmit({
        amount: parsed.amount,
        categoryId,
        date,
        comment: comment.trim() === '' ? null : comment.trim(),
      })
      if (!expense) {
        setAmount('')
        setComment('')
      }
      setErrors({})
    } catch (failure) {
      setErrors({ form: errorMessage(failure) })
    } finally {
      setSaving(false)
    }
  }

  return (
    <form
      className={styles.form}
      onSubmit={submit}
      onKeyDown={(event) => {
        if (event.key === 'Escape' && !saving) onCancel?.()
      }}
      noValidate
    >
      <TextField
        label="Сума, грн"
        placeholder="0,00"
        inputMode="decimal"
        autoComplete="off"
        value={amount}
        error={errors.amount}
        autoFocus={Boolean(expense)}
        onChange={(event) => {
          setAmount(event.target.value)
          setErrors((current) => ({ ...current, amount: undefined, form: undefined }))
        }}
      />
      <Select
        label="Категорія"
        value={categoryId}
        error={errors.categoryId}
        onChange={(event) => {
          setCategoryId(event.target.value)
          setErrors((current) => ({ ...current, categoryId: undefined, form: undefined }))
        }}
      >
        <option value="" disabled>
          Оберіть категорію
        </option>
        {options.map((category) => (
          <option key={category.id} value={category.id}>
            {category.isArchived ? `${category.name} (в архіві)` : category.name}
          </option>
        ))}
      </Select>
      <TextField
        label="Дата"
        type="date"
        value={date}
        error={errors.date}
        onChange={(event) => {
          setDate(event.target.value)
          setErrors((current) => ({ ...current, date: undefined, form: undefined }))
        }}
      />
      <div className={styles.comment}>
        <TextField
          label="Коментар"
          placeholder="Необов’язково"
          value={comment}
          maxLength={maxCommentLength}
          onChange={(event) => {
            setComment(event.target.value)
            setErrors((current) => ({ ...current, form: undefined }))
          }}
        />
      </div>
      <div className={styles.actions}>
        <Button type="submit" variant="primary" loading={saving}>
          {submitLabel}
        </Button>
        {onCancel && (
          <Button onClick={onCancel} disabled={saving}>
            Скасувати
          </Button>
        )}
      </div>
      {errors.form && (
        <p className={styles.formError} role="alert">
          {errors.form}
        </p>
      )}
    </form>
  )
}
