import { PageHeader } from '../../ui/PageHeader.tsx'
import { EmptyState } from '../../ui/states.tsx'

export function ExpensesPage() {
  return (
    <>
      <PageHeader title="Витрати" description="Усі витрати за місяць" />
      <EmptyState
        title="Розділ ще не готовий"
        description="Тут можна буде додавати витрати та переглядати їх за місяць."
      />
    </>
  )
}
