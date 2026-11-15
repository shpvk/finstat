import { render, screen } from '@testing-library/react'
import { expect, it } from 'vitest'
import App from './App.tsx'

it('renders the application heading', () => {
  render(<App />)

  expect(screen.getByRole('heading', { level: 1, name: 'Finstat' })).toBeInTheDocument()
})
