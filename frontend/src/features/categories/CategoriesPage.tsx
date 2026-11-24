import { Archive, Pencil } from 'lucide-react'
import { useEffect, useId, useRef, useState, type FormEvent } from 'react'
import {
  archiveCategory,
  createCategory,
  listCategories,
  renameCategory,
  type Category,
} from '../../api/categories.ts'
import { errorMessage } from '../../api/client.ts'
import { useResource } from '../../api/useResource.ts'
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
  const { data, error, reload, mutate } = useResource('categories', (signal) =>
    listCategories(true, signal),
  )
  const [selected, setSelected] = useState<{ id: string; action: RowAction } | null>(null)
  const activeHeading = useRef<HTMLHeadingElement>(null)

  const active = data?.filter((category) => !category.isArchived) ?? []
  const archived = data?.filter((category) => category.isArchived) ?? []

  function replace(category: Category) {
    mutate((list) => list.map((item) => (item.id === category.id ? category : item)))
    reload()
  }

  return (
    <>
      <PageHeader
        title="Категорії"
        description="Категорії допомагають бачити, на що саме йдуть гроші"
      />
      <AddCategoryForm
        onAdded={(category) => {
          mutate((list) => [...list, category])
          reload()
        }}
      />
      {error ? (
        <ErrorState message={error} onRetry={reload} />
      ) : !data ? (
        <LoadingState />
      ) : (
        <>
          <section className={styles.section} aria-labelledby="active-categories">
            <h2
              id="active-categories"
              ref={activeHeading}
              tabIndex={-1}
              className={styles.heading}
            >
              Активні <span className={styles.count}>{active.length}</span>
            </h2>
            {active.length === 0 ? (
              <EmptyState
                level={3}
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
                    onOpen={(action) => setSelected({ id: category.id, action })}
                    onClose={() =>
                      setSelected((current) => (current?.id === category.id ? null : current))
                    }
                    onRenamed={replace}
                    onArchived={(archivedCategory) => {
                      replace(archivedCategory)
                      activeHeading.current?.focus()
                    }}
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

function AddCategoryForm({ onAdded }: { onAdded: (category: Category) => void }) {
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
      onAdded(category)
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
          readOnly={saving}
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
  onOpen: (action: RowAction) => void
  onClose: () => void
  onRenamed: (category: Category) => void
  onArchived: (category: Category) => void
}

function ActiveCategoryRow({
  category,
  action,
  onOpen,
  onClose,
  onRenamed,
  onArchived,
}: ActiveCategoryRowProps) {
  const renameButton = useRef<HTMLButtonElement>(null)
  const archiveButton = useRef<HTMLButtonElement>(null)
  const opened = useRef<RowAction | null>(null)

  useEffect(() => {
    if (action) {
      opened.current = action
      return
    }
    if (opened.current) {
      const opener = opened.current === 'rename' ? renameButton : archiveButton
      opener.current?.focus()
      opened.current = null
    }
  }, [action])

  return (
    <li className={styles.row}>
      {action === 'rename' ? (
        <RenameForm
          category={category}
          onCancel={onClose}
          onSaved={(renamed) => {
            onClose()
            onRenamed(renamed)
          }}
        />
      ) : action === 'archive' ? (
        <ArchiveConfirm
          category={category}
          onCancel={onClose}
          onSaved={() => {
            opened.current = null
            onClose()
            onArchived({ ...category, isArchived: true })
          }}
        />
      ) : (
        <>
          <span className={styles.name}>{category.name}</span>
          <span className={styles.actions}>
            <Button
              ref={renameButton}
              variant="ghost"
              aria-label={`Перейменувати «${category.name}»`}
              onClick={() => onOpen('rename')}
            >
              <Pencil size={16} aria-hidden="true" />
              Перейменувати
            </Button>
            <Button
              ref={archiveButton}
              variant="ghost"
              aria-label={`Архівувати «${category.name}»`}
              onClick={() => onOpen('archive')}
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

interface RenameFormProps {
  category: Category
  onCancel: () => void
  onSaved: (category: Category) => void
}

function RenameForm({ category, onCancel, onSaved }: RenameFormProps) {
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
      onCancel()
      return
    }
    setSaving(true)
    try {
      const renamed = await renameCategory(category.id, name.trim())
      toast(`Категорію перейменовано на «${renamed.name}»`)
      onSaved(renamed)
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
        if (event.key === 'Escape' && !saving) onCancel()
      }}
      noValidate
    >
      <div className={styles.grow}>
        <TextField
          label={`Нова назва для «${category.name}»`}
          hideLabel
          value={name}
          maxLength={maxNameLength}
          readOnly={saving}
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
        <Button onClick={onCancel} disabled={saving}>
          Скасувати
        </Button>
      </span>
    </form>
  )
}

interface ArchiveConfirmProps {
  category: Category
  onCancel: () => void
  onSaved: () => void
}

function ArchiveConfirm({ category, onCancel, onSaved }: ArchiveConfirmProps) {
  const toast = useToast()
  const [error, setError] = useState<string>()
  const [saving, setSaving] = useState(false)
  const noteId = useId()

  async function confirm() {
    setSaving(true)
    try {
      await archiveCategory(category.id)
      toast(`Категорію «${category.name}» перенесено в архів`)
      onSaved()
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
      aria-describedby={noteId}
      onKeyDown={(event) => {
        if (event.key === 'Escape' && !saving) onCancel()
      }}
    >
      <div className={styles.grow}>
        <p className={styles.confirmTitle}>Архівувати «{category.name}»?</p>
        <p id={noteId} className={styles.note}>
          Нові витрати в ній додати не вийде. Відновлення з архіву не передбачено.
        </p>
        {error && (
          <p className={styles.confirmError} role="alert">
            {error}
          </p>
        )}
      </div>
      <span className={styles.actions}>
        <Button variant="danger" loading={saving} onClick={confirm}>
          Архівувати
        </Button>
        <Button onClick={onCancel} disabled={saving} autoFocus>
          Скасувати
        </Button>
      </span>
    </div>
  )
}
