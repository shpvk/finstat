import { lazy, Suspense } from 'react'
import { BrowserRouter, Route, Routes } from 'react-router'
import { AppShell } from './app/AppShell.tsx'
import { NotFoundPage } from './app/NotFoundPage.tsx'
import { CategoriesPage } from './features/categories/CategoriesPage.tsx'
import { ExpensesPage } from './features/expenses/ExpensesPage.tsx'
import { OverviewPage } from './features/overview/OverviewPage.tsx'
import { LoadingState } from './ui/states.tsx'
import { ToastProvider } from './ui/Toast.tsx'

const UiShowcasePage = import.meta.env.DEV ? lazy(() => import('./dev/UiShowcasePage.tsx')) : null

function App() {
  return (
    <ToastProvider>
      <BrowserRouter>
        <Routes>
          <Route element={<AppShell />}>
            <Route index element={<OverviewPage />} />
            <Route path="expenses" element={<ExpensesPage />} />
            <Route path="categories" element={<CategoriesPage />} />
            {UiShowcasePage && (
              <Route
                path="ui"
                element={
                  <Suspense fallback={<LoadingState />}>
                    <UiShowcasePage />
                  </Suspense>
                }
              />
            )}
            <Route path="*" element={<NotFoundPage />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </ToastProvider>
  )
}

export default App
