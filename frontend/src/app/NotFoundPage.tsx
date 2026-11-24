import { Link } from 'react-router'
import { PageHeader } from '../ui/PageHeader.tsx'
import { EmptyState } from '../ui/states.tsx'

export function NotFoundPage() {
  return (
    <>
      <PageHeader title="Сторінку не знайдено" />
      <EmptyState
        title="Такої сторінки немає"
        description="Перевірте адресу або поверніться до огляду."
        action={<Link to="/">Перейти до огляду</Link>}
      />
    </>
  )
}
