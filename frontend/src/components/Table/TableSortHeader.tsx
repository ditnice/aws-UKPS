'use client'

import styles from './TableSortHeader.module.scss'

import type { ComponentPropsWithoutRef } from 'react'

export type TableSortDirection = 'none' | 'ascending' | 'descending'

export type TableSortHeaderProps = Omit<ComponentPropsWithoutRef<'th'>, 'aria-sort'> & {
  direction: TableSortDirection
  onSort: (direction: Exclude<TableSortDirection, 'none'>) => void
}

const indicatorPaths: Record<TableSortDirection, readonly string[]> = {
  ascending: ['M6.5625 15.5L11 6.63125L15.4375 15.5H6.5625Z'],
  descending: ['M15.4375 7L11 15.8687L6.5625 7L15.4375 7Z'],
  none: [
    'M8.1875 9.5L10.9609 3.95703L13.7344 9.5H8.1875Z',
    'M13.7344 12.0781L10.9609 17.6211L8.1875 12.0781H13.7344Z',
  ],
}

export function TableSortHeader({
  children,
  className,
  direction,
  onSort,
  scope = 'col',
  ...props
}: TableSortHeaderProps) {
  const nextDirection = direction === 'ascending' ? 'descending' : 'ascending'

  return (
    <th {...props} aria-sort={direction} className={className} scope={scope}>
      <button className={styles.button} onClick={() => onSort(nextDirection)} type="button">
        {children}
        <SortIndicator direction={direction} />
      </button>
    </th>
  )
}

function SortIndicator({ direction }: { direction: TableSortDirection }) {
  return (
    <svg
      aria-hidden="true"
      className={styles.indicator}
      fill="currentColor"
      focusable="false"
      viewBox="0 0 22 22"
    >
      {indicatorPaths[direction].map((path) => (
        <path d={path} key={path} />
      ))}
    </svg>
  )
}
