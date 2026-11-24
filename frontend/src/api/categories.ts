import { apiRequest } from './client.ts'

export interface Category {
  id: string
  name: string
  isArchived: boolean
}

export function listCategories(includeArchived: boolean, signal?: AbortSignal) {
  return apiRequest<Category[]>(`/categories?includeArchived=${includeArchived}`, { signal })
}

export function createCategory(name: string) {
  return apiRequest<Category>('/categories', { method: 'POST', body: { name } })
}

export function renameCategory(id: string, name: string) {
  return apiRequest<Category>(`/categories/${id}`, { method: 'PUT', body: { name } })
}

export function archiveCategory(id: string) {
  return apiRequest<void>(`/categories/${id}/archive`, { method: 'POST' })
}
