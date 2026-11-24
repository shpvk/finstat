import { describe, expect, it } from 'vitest'
import { amountToInput, compareAmounts, formatMoney, formatShare, parseAmount } from './money.ts'

describe('compareAmounts', () => {
  it('orders decimal strings without converting them to numbers', () => {
    expect(compareAmounts('999.99', '1000.00')).toBeLessThan(0)
    expect(compareAmounts('380.05', '380.50')).toBeLessThan(0)
    expect(compareAmounts('9999999999.99', '9999999999.98')).toBeGreaterThan(0)
    expect(compareAmounts('120.50', '120.50')).toBe(0)
    expect(compareAmounts('1.5', '1.500')).toBe(0)
  })
})

it('formats a share in uk-UA without a break before the percent sign', () => {
  expect(formatShare('77.5')).toBe('77,5%')
  expect(formatShare('100.0')).toBe('100,0%')
  expect(formatShare('0.0')).toBe('0,0%')
})

describe('formatMoney', () => {
  it('formats exact decimal strings in uk-UA without losing precision', () => {
    expect(formatMoney('1250.5')).toMatch(/^1\s250,50\s(₴|грн)$/)
    expect(formatMoney('9999999999.99')).toMatch(/^9\s999\s999\s999,99\s(₴|грн)$/)
    expect(formatMoney('0.1')).toMatch(/^0,10\s(₴|грн)$/)
  })
})

describe('parseAmount', () => {
  it('accepts a comma or a dot and ignores spaces', () => {
    expect(parseAmount(' 1 250,5 ')).toEqual({ amount: '1250.5' })
    expect(parseAmount('380')).toEqual({ amount: '380' })
    expect(parseAmount('007.10')).toEqual({ amount: '7.10' })
  })

  it('rejects what the backend would reject instead of rounding', () => {
    expect(parseAmount('')).toEqual({ error: 'Вкажіть суму.' })
    expect(parseAmount('12,345')).toEqual({ error: 'Сума може мати не більше двох знаків після коми.' })
    expect(parseAmount('0,00')).toEqual({ error: 'Сума має бути більшою за нуль.' })
    expect(parseAmount('-5')).toEqual({ error: 'Вкажіть суму числом, наприклад 125,50.' })
    expect(parseAmount('1e3')).toEqual({ error: 'Вкажіть суму числом, наприклад 125,50.' })
    expect(parseAmount('12345678901')).toEqual({ error: 'Сума завелика.' })
  })
})

it('shows an API amount in the input format', () => {
  expect(amountToInput('1250.50')).toBe('1250,50')
})
