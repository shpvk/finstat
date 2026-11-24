import { fireEvent, render, screen } from '@testing-library/react'
import { expect, it, vi } from 'vitest'
import { Button } from './Button.tsx'
import { Select } from './Select.tsx'
import { ErrorState, LoadingState } from './states.tsx'
import { TextField } from './TextField.tsx'

it('ignores clicks and stays focusable while a button is loading', () => {
  const onClick = vi.fn()
  render(
    <Button loading onClick={onClick}>
      Зберегти
    </Button>,
  )
  const button = screen.getByRole('button', { name: 'Зберегти' })

  fireEvent.click(button)

  expect(onClick).not.toHaveBeenCalled()
  expect(button).toHaveAttribute('aria-busy', 'true')
  expect(button).toBeEnabled()
})

it('does not submit a form while its submit button is loading', () => {
  const onSubmit = vi.fn((event: React.FormEvent) => event.preventDefault())
  render(
    <form onSubmit={onSubmit}>
      <Button type="submit" loading>
        Зберегти
      </Button>
    </form>,
  )

  fireEvent.click(screen.getByRole('button', { name: 'Зберегти' }))

  expect(onSubmit).not.toHaveBeenCalled()
})

it('calls the handler of an idle button', () => {
  const onClick = vi.fn()
  render(<Button onClick={onClick}>Додати</Button>)

  fireEvent.click(screen.getByRole('button', { name: 'Додати' }))

  expect(onClick).toHaveBeenCalledOnce()
})

it('ties a text field to its label and hint', () => {
  render(<TextField label="Сума" hint="У гривнях" />)

  const input = screen.getByLabelText('Сума')

  expect(input).toHaveAccessibleDescription('У гривнях')
  expect(input).not.toHaveAttribute('aria-invalid')
})

it('marks a text field invalid and announces the error instead of the hint', () => {
  render(<TextField label="Сума" hint="У гривнях" error="Вкажіть суму." />)

  const input = screen.getByLabelText('Сума')

  expect(input).toBeInvalid()
  expect(input).toHaveAccessibleDescription('Вкажіть суму.')
  expect(screen.queryByText('У гривнях')).not.toBeInTheDocument()
})

it('ties a select to its label and error', () => {
  render(
    <Select label="Категорія" error="Оберіть категорію.">
      <option value="food">Їжа</option>
    </Select>,
  )

  const select = screen.getByLabelText('Категорія')

  expect(select).toBeInvalid()
  expect(select).toHaveAccessibleDescription('Оберіть категорію.')
})

it('announces a loading state to assistive technology', () => {
  render(<LoadingState />)

  expect(screen.getByRole('status')).toHaveTextContent('Завантаження…')
})

it('shows the error message and retries on request', () => {
  const onRetry = vi.fn()
  render(<ErrorState message="Не вдалося з’єднатися із сервером." onRetry={onRetry} />)

  expect(screen.getByRole('alert')).toHaveTextContent('Не вдалося з’єднатися із сервером.')

  fireEvent.click(screen.getByRole('button', { name: 'Спробувати ще раз' }))

  expect(onRetry).toHaveBeenCalledOnce()
})
