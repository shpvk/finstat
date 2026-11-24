import { createContext, use } from 'react'
import { currentMonth } from './dates.ts'

interface MonthState {
  month: string
  setMonth: (month: string) => void
}

export const MonthContext = createContext<MonthState>({
  month: currentMonth(),
  setMonth: () => {},
})

export function useMonth() {
  return use(MonthContext)
}
