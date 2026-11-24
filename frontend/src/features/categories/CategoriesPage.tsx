import { PageHeader } from '../../ui/PageHeader.tsx'
import { EmptyState } from '../../ui/states.tsx'

export function CategoriesPage() {
  return (
    <>
      <PageHeader title="Категорії" description="Категорії для ваших витрат" />
      <EmptyState
        title="Розділ ще не готовий"
        description="Тут можна буде додавати, перейменовувати та архівувати категорії."
      />
    </>
  )
}
