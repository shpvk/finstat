import { useMemo, useState, type ReactNode } from 'react'
import { currentMonth } from './dates.ts'
import { MonthContext } from './monthContext.ts'

export function MonthProvider({ initial, children }: { initial?: string; children: ReactNode }) {
  const [month, setMonth] = useState(() => initial ?? currentMonth())
  const value = useMemo(() => ({ month, setMonth }), [month])
  return <MonthContext value={value}>{children}</MonthContext>
}
