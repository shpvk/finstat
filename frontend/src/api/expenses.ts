import { apiRequest } from './client.ts'

export interface Expense {
  id: string
  amount: string
  categoryId: string
  date: string
  comment: string | null
}

export interface ExpenseInput {
  amount: string
  categoryId: string
  date: string
  comment: string | null
}

export function listExpenses(month: string, signal?: AbortSignal) {
  return apiRequest<Expense[]>(`/expenses?month=${month}`, { signal })
}

export function createExpense(input: ExpenseInput) {
  return apiRequest<Expense>('/expenses', { method: 'POST', body: input })
}

export function updateExpense(id: string, input: ExpenseInput) {
  return apiRequest<Expense>(`/expenses/${id}`, { method: 'PUT', body: input })
}

export function deleteExpense(id: string) {
  return apiRequest<void>(`/expenses/${id}`, { method: 'DELETE' })
}
