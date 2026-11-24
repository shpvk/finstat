import { Button } from '../ui/Button.tsx'
import { PageHeader } from '../ui/PageHeader.tsx'
import { Select } from '../ui/Select.tsx'
import { EmptyState, ErrorState, LoadingState } from '../ui/states.tsx'
import { TextField } from '../ui/TextField.tsx'
import styles from './UiShowcasePage.module.css'

export default function UiShowcasePage() {
  return (
    <>
      <PageHeader
        title="Компоненти"
        description="Довідкова сторінка для розробки: базові компоненти в усіх станах"
        action={<Button variant="primary">Основна дія</Button>}
      />

      <section className={styles.section}>
        <h2 className={styles.heading}>Кнопки</h2>
        <div className={styles.row}>
          <Button variant="primary">Зберегти</Button>
          <Button>Скасувати</Button>
          <Button variant="danger">Видалити</Button>
        </div>
        <div className={styles.row}>
          <Button variant="primary" loading>
            Збереження…
          </Button>
          <Button loading>Завантаження…</Button>
          <Button variant="primary" disabled>
            Недоступна
          </Button>
          <Button disabled>Недоступна</Button>
        </div>
      </section>

      <section className={styles.section}>
        <h2 className={styles.heading}>Поля</h2>
        <div className={styles.grid}>
          <TextField label="Назва категорії" placeholder="Наприклад, Транспорт" />
          <TextField
            label="Сума"
            inputMode="decimal"
            defaultValue="412,50"
            hint="У гривнях, не більше двох знаків після коми"
          />
          <TextField
            label="Сума"
            inputMode="decimal"
            defaultValue="12,345"
            error="Вкажіть суму не більше ніж із двома знаками після коми."
          />
          <TextField label="Дата" type="date" defaultValue="2026-11-15" />
          <Select label="Категорія" defaultValue="food">
            <option value="food">Їжа</option>
            <option value="utilities">Комунальні послуги</option>
          </Select>
          <Select label="Категорія" defaultValue="" error="Оберіть категорію.">
            <option value="" disabled>
              Оберіть категорію
            </option>
            <option value="food">Їжа</option>
            <option value="utilities">Комунальні послуги</option>
          </Select>
          <TextField label="Коментар" defaultValue="Недоступне поле" disabled />
          <Select label="Категорія" defaultValue="food" disabled>
            <option value="food">Їжа</option>
          </Select>
        </div>
      </section>

      <section className={styles.section}>
        <h2 className={styles.heading}>Стани</h2>
        <div className={styles.stack}>
          <LoadingState />
          <EmptyState
            title="Витрат за цей місяць ще немає"
            description="Додайте першу витрату, і вона з’явиться в цьому списку."
            action={<Button variant="primary">Додати витрату</Button>}
          />
          <ErrorState message="Не вдалося з’єднатися із сервером." onRetry={() => {}} />
        </div>
      </section>
    </>
  )
}
