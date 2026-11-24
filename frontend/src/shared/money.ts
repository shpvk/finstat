const moneyFormat = new Intl.NumberFormat('uk-UA', {
  style: 'currency',
  currency: 'UAH',
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
})

export function formatMoney(amount: string) {
  return moneyFormat.format(amount as Intl.StringNumericLiteral)
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
