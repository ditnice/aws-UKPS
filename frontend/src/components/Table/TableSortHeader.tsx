'use client'

import clsx from 'clsx'

import styles from './TableSortHeader.module.scss'

import type { ComponentPropsWithoutRef } from 'react'

export type TableSortDirection = 'none' | 'ascending' | 'descending'

export type TableSortHeaderProps = Omit<ComponentPropsWithoutRef<'th'>, 'aria-sort'> & {
  direction: TableSortDirection
  onSort: (direction: Exclude<TableSortDirection, 'none'>) => void
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
    <th {...props} aria-sort={direction} className={clsx(styles.header, className)} scope={scope}>
      <button className={styles.button} onClick={() => onSort(nextDirection)} type="button">
        {children}
        <SortIndicator direction={direction} />
      </button>
    </th>
  )
}

function SortIndicator({ direction }: { direction: TableSortDirection }) {
  if (direction === 'ascending') {
    return (
      <svg
        aria-hidden="true"
        className={styles.indicator}
        fill="none"
        focusable="false"
        height="22"
        role="img"
        viewBox="0 0 22 22"
        width="22"
        xmlns="http://www.w3.org/2000/svg"
      >
        <path d="M6.5625 15.5L11 6.63125L15.4375 15.5H6.5625Z" fill="currentColor" />
      </svg>
    )
  }

  if (direction === 'descending') {
    return (
      <svg
        aria-hidden="true"
        className={styles.indicator}
        fill="none"
        focusable="false"
        height="22"
        role="img"
        viewBox="0 0 22 22"
        width="22"
        xmlns="http://www.w3.org/2000/svg"
      >
        <path d="M15.4375 7L11 15.8687L6.5625 7L15.4375 7Z" fill="currentColor" />
      </svg>
    )
  }

  return (
    <svg
      aria-hidden="true"
      className={styles.indicator}
      fill="none"
      focusable="false"
      height="22"
      role="img"
      viewBox="0 0 22 22"
      width="22"
      xmlns="http://www.w3.org/2000/svg"
    >
      <path d="M8.1875 9.5L10.9609 3.95703L13.7344 9.5H8.1875Z" fill="currentColor" />
      <path d="M13.7344 12.0781L10.9609 17.6211L8.1875 12.0781H13.7344Z" fill="currentColor" />
    </svg>
  )
}
