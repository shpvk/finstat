import { describe, expect, it } from 'vitest'
import { currentMonth, formatDay, formatMonth, monthOf, shiftMonth, todayIso } from './dates.ts'

describe('dates', () => {
  it('takes today from the local calendar date', () => {
    expect(todayIso(new Date(2026, 0, 5, 23, 59))).toBe('2026-01-05')
    expect(currentMonth(new Date(2026, 11, 31, 23, 59))).toBe('2026-12')
  })

  it('moves between months across years', () => {
    expect(shiftMonth('2026-01', -1)).toBe('2025-12')
    expect(shiftMonth('2026-12', 1)).toBe('2027-01')
    expect(monthOf('2026-11-24')).toBe('2026-11')
  })

  it('formats months and days in Ukrainian', () => {
    expect(formatMonth('2026-11')).toBe('Листопад 2026')
    expect(formatDay('2026-11-24')).toBe('24 листопада')
  })
})
