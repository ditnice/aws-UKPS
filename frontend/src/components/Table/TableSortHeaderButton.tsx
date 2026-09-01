'use client'

import { ComponentPropsWithoutRef } from 'react'

import { ActiveSortDirection, TableSortDirection } from './TableSortHeader'
import styles from './TableSortHeader.module.scss'

export type TableSortHeaderButtonProps = Omit<ComponentPropsWithoutRef<'th'>, 'aria-sort'> & {
  direction: TableSortDirection
  nextDirection: ActiveSortDirection
  onSort: (direction: ActiveSortDirection) => void
}

const TableSortHeaderButton = ({ children, nextDirection, onSort }: TableSortHeaderButtonProps) => {
  return (
    <button className={styles.button} onClick={() => onSort(nextDirection)} type="button">
      {children}
    </button>
  )
}

export default TableSortHeaderButton
