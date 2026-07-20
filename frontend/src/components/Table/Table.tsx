import { Table as NdsTable } from '@nice-digital/nds-table'

import styles from './Table.module.scss'

import type { ComponentPropsWithoutRef } from 'react'

export type TableColumnWidth = 'default' | 'content' | 'equal'

export type TableProps = ComponentPropsWithoutRef<'table'> & {
  /**
   * 'default' – unmodified design system styling (table shrinks to fit its content).
   * 'content' – table fills its container; columns are sized by their content.
   * 'equal'   – table fills its container; all columns get equal width.
   */
  columnWidth?: TableColumnWidth
}

export function Table({ children, className, columnWidth = 'default', ...rest }: TableProps) {
  if (columnWidth === 'default') {
    return (
      <NdsTable className={className} {...rest}>
        {children}
      </NdsTable>
    )
  }

  const tableClassName = [
    styles['table--full-width'],
    columnWidth === 'equal' && styles['table--equal'],
    className,
  ]
    .filter(Boolean)
    .join(' ')

  return (
    <div className={styles['table-wrapper']}>
      <NdsTable className={tableClassName} {...rest}>
        {children}
      </NdsTable>
    </div>
  )
}
