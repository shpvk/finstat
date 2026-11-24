import { Archive, Pencil } from 'lucide-react'
import { useState, type FormEvent } from 'react'
import {
  archiveCategory,
  createCategory,
  listCategories,
  renameCategory,
  type Category,
} from '../../api/categories.ts'
import { errorMessage, useResource } from '../../api/useResource.ts'
import { Button } from '../../ui/Button.tsx'
import { PageHeader } from '../../ui/PageHeader.tsx'
import { TextField } from '../../ui/TextField.tsx'
import { useToast } from '../../ui/toastContext.ts'
import { EmptyState, ErrorState, LoadingState } from '../../ui/states.tsx'
import styles from './CategoriesPage.module.css'

const maxNameLength = 50

function validateName(name: string) {
  const trimmed = name.trim()
  if (trimmed === '') return 'Вкажіть назву категорії.'
  if (trimmed.length > maxNameLength) return `Назва не може бути довшою за ${maxNameLength} символів.`
  return undefined
}

type RowAction = 'rename' | 'archive'

export function CategoriesPage() {
  const { data, error, reload } = useResource('categories', (signal) =>
    listCategories(true, signal),
  )
  const [selected, setSelected] = useState<{ id: string; action: RowAction } | null>(null)

  const active = data?.filter((category) => !category.isArchived) ?? []
  const archived = data?.filter((category) => category.isArchived) ?? []

  return (
    <>
      <PageHeader
        title="Категорії"
        description="Категорії допомагають бачити, на що саме йдуть гроші"
      />
      <AddCategoryForm onAdded={reload} />
      {error ? (
        <ErrorState message={error} onRetry={reload} />
      ) : !data ? (
        <LoadingState />
      ) : (
        <>
          <section className={styles.section} aria-labelledby="active-categories">
            <h2 id="active-categories" className={styles.heading}>
              Активні <span className={styles.count}>{active.length}</span>
            </h2>
            {active.length === 0 ? (
              <EmptyState
                title="Активних категорій немає"
                description="Додайте категорію, щоб записувати в неї витрати."
              />
            ) : (
              <ul className={styles.list}>
                {active.map((category) => (
                  <ActiveCategoryRow
                    key={category.id}
                    category={category}
                    action={selected?.id === category.id ? selected.action : null}
                    onActionChange={(action) =>
                      setSelected(action ? { id: category.id, action } : null)
                    }
                    onChanged={reload}
                  />
                ))}
              </ul>
            )}
          </section>
          {archived.length > 0 && (
            <section className={styles.section} aria-labelledby="archived-categories">
              <h2 id="archived-categories" className={styles.heading}>
                Архів <span className={styles.count}>{archived.length}</span>
              </h2>
              <p className={styles.note}>
                Витрати в цих категоріях лишаються в історії та статистиці, але нові додати не
                можна.
              </p>
              <ul className={styles.list}>
                {archived.map((category) => (
                  <li key={category.id} className={`${styles.row} ${styles.archived}`}>
                    <span className={styles.name}>{category.name}</span>
                    <span className={styles.badge}>В архіві</span>
                  </li>
                ))}
              </ul>
            </section>
          )}
        </>
      )}
    </>
  )
}

function AddCategoryForm({ onAdded }: { onAdded: () => void }) {
  const toast = useToast()
  const [name, setName] = useState('')
  const [error, setError] = useState<string>()
  const [saving, setSaving] = useState(false)

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const invalid = validateName(name)
    if (invalid) {
      setError(invalid)
      return
    }
    setSaving(true)
    try {
      const category = await createCategory(name.trim())
      setName('')
      setError(undefined)
      toast(`Категорію «${category.name}» додано`)
      onAdded()
    } catch (failure) {
      setError(errorMessage(failure))
    } finally {
      setSaving(false)
    }
  }

  return (
    <form className={styles.addForm} onSubmit={submit} noValidate>
      <div className={styles.grow}>
        <TextField
          label="Нова категорія"
          placeholder="Наприклад, Транспорт"
          value={name}
          maxLength={maxNameLength}
          error={error}
          onChange={(event) => {
            setName(event.target.value)
            setError(undefined)
          }}
        />
      </div>
      <div className={styles.addButton}>
        <Button type="submit" variant="primary" loading={saving}>
          Додати
        </Button>
      </div>
    </form>
  )
}

interface ActiveCategoryRowProps {
  category: Category
  action: RowAction | null
  onActionChange: (action: RowAction | null) => void
  onChanged: () => void
}

function ActiveCategoryRow({ category, action, onActionChange, onChanged }: ActiveCategoryRowProps) {
  const close = () => onActionChange(null)

  return (
    <li className={styles.row}>
      {action === 'rename' ? (
        <RenameForm category={category} onDone={close} onChanged={onChanged} />
      ) : action === 'archive' ? (
        <ArchiveConfirm category={category} onDone={close} onChanged={onChanged} />
      ) : (
        <>
          <span className={styles.name}>{category.name}</span>
          <span className={styles.actions}>
            <Button
              variant="ghost"
              aria-label={`Перейменувати «${category.name}»`}
              onClick={() => onActionChange('rename')}
            >
              <Pencil size={16} aria-hidden="true" />
              Перейменувати
            </Button>
            <Button
              variant="ghost"
              aria-label={`Архівувати «${category.name}»`}
              onClick={() => onActionChange('archive')}
            >
              <Archive size={16} aria-hidden="true" />
              Архівувати
            </Button>
          </span>
        </>
      )}
    </li>
  )
}

interface RowFormProps {
  category: Category
  onDone: () => void
  onChanged: () => void
}

function RenameForm({ category, onDone, onChanged }: RowFormProps) {
  const toast = useToast()
  const [name, setName] = useState(category.name)
  const [error, setError] = useState<string>()
  const [saving, setSaving] = useState(false)

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const invalid = validateName(name)
    if (invalid) {
      setError(invalid)
      return
    }
    if (name.trim() === category.name) {
      onDone()
      return
    }
    setSaving(true)
    try {
      const renamed = await renameCategory(category.id, name.trim())
      toast(`Категорію перейменовано на «${renamed.name}»`)
      onDone()
      onChanged()
    } catch (failure) {
      setError(errorMessage(failure))
      setSaving(false)
    }
  }

  return (
    <form
      className={styles.inlineForm}
      onSubmit={submit}
      onKeyDown={(event) => {
        if (event.key === 'Escape') onDone()
      }}
      noValidate
    >
      <div className={styles.grow}>
        <TextField
          label={`Нова назва для «${category.name}»`}
          hideLabel
          value={name}
          maxLength={maxNameLength}
          error={error}
          autoFocus
          onChange={(event) => {
            setName(event.target.value)
            setError(undefined)
          }}
        />
      </div>
      <span className={styles.actions}>
        <Button type="submit" variant="primary" loading={saving}>
          Зберегти
        </Button>
        <Button onClick={onDone}>Скасувати</Button>
      </span>
    </form>
  )
}

function ArchiveConfirm({ category, onDone, onChanged }: RowFormProps) {
  const toast = useToast()
  const [error, setError] = useState<string>()
  const [saving, setSaving] = useState(false)

  async function confirm() {
    setSaving(true)
    try {
      await archiveCategory(category.id)
      toast(`Категорію «${category.name}» перенесено в архів`)
      onDone()
      onChanged()
    } catch (failure) {
      setError(errorMessage(failure))
      setSaving(false)
    }
  }

  return (
    <div
      className={styles.inlineForm}
      role="group"
      aria-label={`Архівування «${category.name}»`}
      onKeyDown={(event) => {
        if (event.key === 'Escape') onDone()
      }}
    >
      <div className={styles.grow}>
        <p className={styles.confirmTitle}>Архівувати «{category.name}»?</p>
        <p className={error ? styles.confirmError : styles.note} role={error ? 'alert' : undefined}>
          {error ?? 'Нові витрати в ній додати не вийде. Відновлення з архіву не передбачено.'}
        </p>
      </div>
      <span className={styles.actions}>
        <Button variant="danger" loading={saving} onClick={confirm} autoFocus>
          Архівувати
        </Button>
        <Button onClick={onDone}>Скасувати</Button>
      </span>
    </div>
  )
}
