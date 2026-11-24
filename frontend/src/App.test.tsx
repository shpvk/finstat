import { fireEvent, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, expect, it, vi } from 'vitest'
import App from './App.tsx'
import { fakeApi } from './test/fakeApi.ts'

beforeEach(() => {
  window.history.pushState({}, '', '/')
  fakeApi({ 'GET /api/categories': () => ({ body: [] }) })
})

afterEach(() => {
  vi.unstubAllGlobals()
})

it('opens the overview and switches sections from the navigation', () => {
  render(<App />)

  expect(screen.getByRole('heading', { level: 1, name: 'Огляд' })).toBeInTheDocument()
  expect(screen.getByRole('link', { name: 'Огляд' })).toHaveAttribute('aria-current', 'page')

  fireEvent.click(screen.getByRole('link', { name: 'Витрати' }))

  expect(screen.getByRole('heading', { level: 1, name: 'Витрати' })).toBeInTheDocument()
  expect(screen.getByRole('link', { name: 'Витрати' })).toHaveAttribute('aria-current', 'page')
  expect(screen.getByRole('link', { name: 'Огляд' })).not.toHaveAttribute('aria-current')

  fireEvent.click(screen.getByRole('link', { name: 'Категорії' }))

  expect(screen.getByRole('heading', { level: 1, name: 'Категорії' })).toBeInTheDocument()
})

function currentSectionMarker(container: HTMLElement) {
  return container.querySelector<HTMLElement>('nav > span[aria-hidden="true"]')
}

it('opens a section directly by its address', () => {
  window.history.pushState({}, '', '/categories')

  const { container } = render(<App />)

  expect(screen.getByRole('heading', { level: 1, name: 'Категорії' })).toBeInTheDocument()
  expect(screen.getByRole('link', { name: 'Категорії' })).toHaveAttribute('aria-current', 'page')
  expect(currentSectionMarker(container)?.style.getPropertyValue('--active-index')).toBe('2')
})

it('moves the current section marker with the navigation', () => {
  const { container } = render(<App />)

  expect(currentSectionMarker(container)?.style.getPropertyValue('--active-index')).toBe('0')

  fireEvent.click(screen.getByRole('link', { name: 'Витрати' }))

  expect(currentSectionMarker(container)?.style.getPropertyValue('--active-index')).toBe('1')
})

it('explains an unknown address and offers the way back', () => {
  window.history.pushState({}, '', '/no-such-page')

  const { container } = render(<App />)

  expect(screen.getByRole('heading', { level: 1, name: 'Сторінку не знайдено' })).toBeInTheDocument()
  expect(currentSectionMarker(container)).toBeNull()

  fireEvent.click(screen.getByRole('link', { name: 'Перейти до огляду' }))

  expect(screen.getByRole('heading', { level: 1, name: 'Огляд' })).toBeInTheDocument()
})
