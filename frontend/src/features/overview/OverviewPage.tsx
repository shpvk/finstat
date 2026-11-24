import { PageHeader } from '../../ui/PageHeader.tsx'
import { EmptyState } from '../../ui/states.tsx'

export function OverviewPage() {
  return (
    <>
      <PageHeader title="Огляд" description="Підсумки поточного місяця" />
      <EmptyState
        title="Розділ ще не готовий"
        description="Тут з’являться сума за місяць, витрати за категоріями та останні операції."
      />
    </>
  )
}
