import { useState } from 'react'
import type { CategoryStatistics } from '../../api/statistics.ts'
import { compareAmounts, formatMoney, formatShare } from '../../shared/money.ts'
import styles from './CategoryBreakdown.module.css'

const maxSegments = 6
const ringLength = 1000
const gapLength = 4

interface Segment {
  key: string
  label: string
  tenths: number
  colorIndex: number
  categoryIds: string[]
}

function toTenths(share: string) {
  const [integer, fraction = '0'] = share.split('.')
  return Number(integer) * 10 + Number(fraction.slice(0, 1))
}

function fromTenths(tenths: number) {
  return `${Math.floor(tenths / 10)}.${tenths % 10}`
}

function buildSegments(sorted: CategoryStatistics[]): Segment[] {
  const named = sorted.length > maxSegments ? sorted.slice(0, maxSegments - 1) : sorted
  const rest = sorted.slice(named.length)
  const segments = named.map((category, index) => ({
    key: category.categoryId,
    label: category.name,
    tenths: toTenths(category.share),
    colorIndex: index,
    categoryIds: [category.categoryId],
  }))
  if (rest.length > 0) {
    segments.push({
      key: 'other',
      label: 'Інші',
      tenths: rest.reduce((sum, category) => sum + toTenths(category.share), 0),
      colorIndex: maxSegments - 1,
      categoryIds: rest.map((category) => category.categoryId),
    })
  }
  return segments
}

interface CategoryBreakdownProps {
  categories: CategoryStatistics[]
  total: string
}

export function CategoryBreakdown({ categories, total }: CategoryBreakdownProps) {
  const [active, setActive] = useState<string | null>(null)
  const sorted = [...categories].sort((a, b) => compareAmounts(b.total, a.total))
  const segments = buildSegments(sorted)
  const segmentOf = new Map(
    segments.flatMap((segment) => segment.categoryIds.map((id) => [id, segment] as const)),
  )
  const activeSegment = segments.find((segment) => segment.key === active)
  const scale = ringLength / Math.max(segments.reduce((sum, s) => sum + s.tenths, 0), 1)

  const lengths = segments.map((segment) => segment.tenths * scale)
  const arcs = segments.map((segment, index) => {
    const start = lengths.slice(0, index).reduce((sum, length) => sum + length, 0)
    const visible = segments.length > 1 ? Math.max(lengths[index] - gapLength, 1) : lengths[index]
    return { segment, dash: `${visible} ${ringLength - visible}`, offset: -start }
  })

  return (
    <section className={styles.card} aria-labelledby="by-category">
      <h2 id="by-category" className={styles.title}>
        Витрати за категоріями
      </h2>
      <div className={styles.layout}>
        <div className={styles.chart}>
          <svg
            viewBox="0 0 200 200"
            className={styles.ring}
            role="img"
            aria-label={`Частки категорій: ${segments
              .map((segment) => `${segment.label} ${formatShare(fromTenths(segment.tenths))}`)
              .join(', ')}`}
          >
            {arcs.map(({ segment, dash, offset: dashOffset }) => (
              <circle
                key={segment.key}
                className={styles.arc}
                data-dimmed={active !== null && active !== segment.key ? '' : undefined}
                cx="100"
                cy="100"
                r="80"
                pathLength={ringLength}
                strokeDasharray={dash}
                strokeDashoffset={dashOffset}
                style={{ stroke: `var(--chart-${segment.colorIndex + 1})` }}
                onPointerEnter={() => setActive(segment.key)}
                onPointerLeave={() => setActive(null)}
              />
            ))}
          </svg>
          <div className={styles.center} aria-hidden="true">
            {activeSegment ? (
              <>
                <span className={styles.centerValue}>
                  {formatShare(fromTenths(activeSegment.tenths))}
                </span>
                <span className={styles.centerLabel}>{activeSegment.label}</span>
              </>
            ) : (
              <>
                <span className={styles.centerValue}>{formatMoney(total)}</span>
                <span className={styles.centerLabel}>усього</span>
              </>
            )}
          </div>
        </div>
        <table className={styles.table}>
          <thead>
            <tr>
              <th scope="col">Категорія</th>
              <th scope="col" className={styles.number}>
                Сума
              </th>
              <th scope="col" className={styles.number}>
                Частка
              </th>
            </tr>
          </thead>
          <tbody>
            {sorted.map((category) => {
              const segment = segmentOf.get(category.categoryId)
              return (
                <tr
                  key={category.categoryId}
                  data-active={segment && active === segment.key ? '' : undefined}
                  onPointerEnter={() => segment && setActive(segment.key)}
                  onPointerLeave={() => setActive(null)}
                >
                  <th scope="row" className={styles.name}>
                    <span
                      className={styles.swatch}
                      style={{ background: `var(--chart-${(segment?.colorIndex ?? 0) + 1})` }}
                      aria-hidden="true"
                    />
                    {category.name}
                    {category.isArchived && (
                      <>
                        {' '}
                        <span className={styles.badge}>в архіві</span>
                      </>
                    )}
                  </th>
                  <td className={styles.number}>{formatMoney(category.total)}</td>
                  <td className={styles.number}>{formatShare(category.share)}</td>
                </tr>
              )
            })}
          </tbody>
        </table>
      </div>
    </section>
  )
}
