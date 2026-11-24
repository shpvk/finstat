function pad(value: number) {
  return String(value).padStart(2, '0')
}

function toLocalDate(isoDate: string) {
  const [year, month, day] = isoDate.split('-').map(Number)
  return new Date(year, month - 1, day ?? 1)
}

export function todayIso(now = new Date()) {
  return `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}`
}

export function currentMonth(now = new Date()) {
  return todayIso(now).slice(0, 7)
}

export function monthOf(isoDate: string) {
  return isoDate.slice(0, 7)
}

export function shiftMonth(month: string, delta: number) {
  const date = toLocalDate(month)
  date.setMonth(date.getMonth() + delta)
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}`
}

const monthName = new Intl.DateTimeFormat('uk-UA', { month: 'long' })
const dayFormat = new Intl.DateTimeFormat('uk-UA', { day: 'numeric', month: 'long' })
const weekdayFormat = new Intl.DateTimeFormat('uk-UA', { weekday: 'short' })

export function formatMonth(month: string) {
  const date = toLocalDate(month)
  const name = monthName.format(date)
  return `${name.charAt(0).toUpperCase()}${name.slice(1)} ${date.getFullYear()}`
}

export function formatDay(isoDate: string) {
  return dayFormat.format(toLocalDate(isoDate))
}

export function formatWeekday(isoDate: string) {
  return weekdayFormat.format(toLocalDate(isoDate))
}
