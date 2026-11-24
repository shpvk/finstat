import { compareAmounts } from '../shared/money.ts'
import { apiRequest } from './client.ts'
import type { Expense } from './expenses.ts'

export interface CategoryStatistics {
  categoryId: string
  name: string
  isArchived: boolean
  total: string
  share: string
}

export interface MonthStatistics {
  month: string
  total: string
  categories: CategoryStatistics[]
  recent: Expense[]
}

export function byTotalDesc(a: CategoryStatistics, b: CategoryStatistics) {
  return compareAmounts(b.total, a.total) || a.name.localeCompare(b.name, 'uk')
}

export function getStatistics(month: string, signal?: AbortSignal) {
  return apiRequest<MonthStatistics>(`/statistics?month=${month}`, { signal })
}
