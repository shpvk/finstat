import { LayoutDashboard, ReceiptText, Tags } from 'lucide-react'
import type { CSSProperties } from 'react'
import { matchPath, NavLink, Outlet, useLocation } from 'react-router'
import styles from './AppShell.module.css'

const sections = [
  { path: '/', label: 'Огляд', icon: LayoutDashboard },
  { path: '/expenses', label: 'Витрати', icon: ReceiptText },
  { path: '/categories', label: 'Категорії', icon: Tags },
]

export function AppShell() {
  const { pathname } = useLocation()
  const activeIndex = sections.findIndex(({ path }) =>
    matchPath({ path, end: path === '/' }, pathname),
  )

  return (
    <div className={styles.shell}>
      <aside className={styles.sidebar}>
        <p className={styles.wordmark}>
          <img src="/favicon.svg" alt="" width="24" height="24" />
          Finstat
        </p>
        <nav className={styles.nav} aria-label="Розділи">
          {activeIndex >= 0 && (
            <span
              className={styles.pill}
              style={{ '--active-index': activeIndex } as CSSProperties}
              aria-hidden="true"
            />
          )}
          <ul className={styles.sections}>
            {sections.map(({ path, label, icon: Icon }) => (
              <li key={path}>
                <NavLink to={path} end={path === '/'} className={styles.link}>
                  <Icon className={styles.icon} strokeWidth={1.75} aria-hidden="true" />
                  {label}
                </NavLink>
              </li>
            ))}
          </ul>
        </nav>
      </aside>
      <main className={styles.content}>
        <div className={styles.column}>
          <Outlet />
        </div>
      </main>
    </div>
  )
}
