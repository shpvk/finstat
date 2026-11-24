const moneyFormat = new Intl.NumberFormat('uk-UA', {
  style: 'currency',
  currency: 'UAH',
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
})

export function formatMoney(amount: string) {
  return moneyFormat.format(amount as Intl.StringNumericLiteral)
}

const shareFormat = new Intl.NumberFormat('uk-UA', {
  minimumFractionDigits: 1,
  maximumFractionDigits: 1,
})

export function formatShare(share: string) {
  return `${shareFormat.format(share as Intl.StringNumericLiteral)} %`
}

export function compareAmounts(a: string, b: string) {
  const [aInteger, aFraction = ''] = a.split('.')
  const [bInteger, bFraction = ''] = b.split('.')
  if (aInteger.length !== bInteger.length) return aInteger.length - bInteger.length
  const left = `${aInteger}.${aFraction.padEnd(2, '0')}`
  const right = `${bInteger}.${bFraction.padEnd(2, '0')}`
  return left < right ? -1 : left > right ? 1 : 0
}

export function amountToInput(amount: string) {
  return amount.replace('.', ',')
}

const maxIntegerDigits = 10

export function parseAmount(input: string): { amount: string } | { error: string } {
  const value = input.replace(/\s/g, '').replace(',', '.')
  if (value === '') return { error: 'Вкажіть суму.' }
  const match = /^(\d+)(?:\.(\d*))?$/.exec(value)
  if (!match) return { error: 'Вкажіть суму числом, наприклад 125,50.' }
  const [, integer, fraction = ''] = match
  if (fraction.length > 2) return { error: 'Сума може мати не більше двох знаків після коми.' }
  const digits = integer.replace(/^0+(?=\d)/, '')
  if (/^0*$/.test(digits + fraction)) return { error: 'Сума має бути більшою за нуль.' }
  if (digits.length > maxIntegerDigits) return { error: 'Сума завелика.' }
  return { amount: fraction === '' ? digits : `${digits}.${fraction}` }
}
