import { fireEvent, render, screen, within } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import type { CategoryStatistics } from '../../api/statistics.ts'
import { CategoryBreakdown } from './CategoryBreakdown.tsx'

function category(name: string, total: string, share: string, isArchived = false): CategoryStatistics {
  return { categoryId: name, name, isArchived, total, share }
}

describe('CategoryBreakdown', () => {
  it('lists categories from the most expensive with amounts and shares', () => {
    render(
      <CategoryBreakdown
        total="2888.65"
        categories={[
          category('Транспорт', '365.50', '12.7'),
          category('Їжа', '1623.15', '56.2'),
          category('Розваги', '520.00', '18.0', true),
          category('Комунальні послуги', '380.00', '13.2'),
        ]}
      />,
    )

    const rows = within(screen.getByRole('table')).getAllByRole('row').slice(1)
    expect(rows.map((row) => within(row).getByRole('rowheader').textContent)).toEqual([
      'Їжа',
      'Розваги в архіві',
      'Комунальні послуги',
      'Транспорт',
    ])
    expect(rows[0].textContent).toMatch(/1\s623,15.*56,2\s%/)
    expect(screen.getByRole('img')).toHaveAccessibleName(
      /^Частки категорій: Їжа 56,2\s%, Розваги 18,0\s%, Комунальні послуги 13,2\s%, Транспорт 12,7\s%$/,
    )
  })

  it('folds small categories into one segment but keeps every row in the table', () => {
    const categories = [
      category('A', '400.00', '40.0'),
      category('B', '200.00', '20.0'),
      category('C', '150.00', '15.0'),
      category('D', '100.00', '10.0'),
      category('E', '80.00', '8.0'),
      category('F', '40.00', '4.0'),
      category('G', '30.00', '3.0'),
    ]
    render(<CategoryBreakdown total="1000.00" categories={categories} />)

    expect(screen.getByRole('img')).toHaveAccessibleName(/E 8,0\s%, Інші 7,0\s%$/)
    expect(within(screen.getByRole('table')).getAllByRole('row')).toHaveLength(8)
  })

  it('shows the share of the hovered category in the middle of the ring', () => {
    const { container } = render(
      <CategoryBreakdown
        total="900.00"
        categories={[category('Їжа', '600.00', '66.7'), category('Таксі', '300.00', '33.3')]}
      />,
    )

    expect(container.textContent).toMatch(/900,00.*усього/)
    fireEvent.pointerEnter(screen.getByRole('row', { name: /Таксі/ }))

    expect(container.textContent).toMatch(/33,3\s%Таксі/)
    expect(container.querySelectorAll('circle[data-dimmed]')).toHaveLength(1)
  })
})
