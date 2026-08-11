import clsx from 'clsx'
import { Children, cloneElement, isValidElement } from 'react'

import styles from './SummaryList.module.scss'

import type { ReactElement, ReactNode } from 'react'

export type SummaryListActionProps = {
  children: ReactNode
  href: string
  visuallyHiddenText: string
}

type ValidSummaryListAction = ReactElement<SummaryListActionProps> | boolean | null | undefined

export type SummaryListRowProps = {
  children?: ValidSummaryListAction | readonly ValidSummaryListAction[]
  label: ReactNode
  value: ReactNode
}

type InternalSummaryListRowProps = SummaryListRowProps & {
  listHasActions?: boolean
  variant?: SummaryListVariant
}

type ValidSummaryListRow = ReactElement<InternalSummaryListRowProps> | boolean | null | undefined

type SummaryListProps = {
  children: ValidSummaryListRow | readonly ValidSummaryListRow[]
  className?: string
  variant?: SummaryListVariant
}

type SummaryListVariant = 'default' | 'two-column'

export function SummaryListAction({ children, href, visuallyHiddenText }: SummaryListActionProps) {
  const accessibleName = `${children} ${visuallyHiddenText}`

  return (
    <a aria-label={accessibleName} href={href}>
      {children}
      <span className="visually-hidden">{visuallyHiddenText}</span>
    </a>
  )
}

export function SummaryListRow({
  children,
  label,
  listHasActions = false,
  variant = 'default',
  value,
}: InternalSummaryListRowProps) {
  const actions = Children.toArray(children).filter(
    (child): child is ReactElement<SummaryListActionProps> => isValidElement(child),
  )
  const rowClassName = clsx(
    styles['summary-list__row'],
    listHasActions &&
      actions.length === 0 &&
      variant !== 'two-column' &&
      styles['summary-list__row--no-actions'],
  )

  return (
    <div className={rowClassName}>
      <dt className={styles['summary-list__key']}>{label}</dt>
      <dd className={styles['summary-list__value']}>{value}</dd>
      {actions.length > 0 && (
        <dd className={styles['summary-list__actions']}>
          {actions.length === 1 ? (
            actions[0]
          ) : (
            <ul className={styles['summary-list__actions-list']}>
              {actions.map((action) => (
                <li className={styles['summary-list__actions-list-item']} key={action.key}>
                  {action}
                </li>
              ))}
            </ul>
          )}
        </dd>
      )}
    </div>
  )
}

export function SummaryList({ children, className, variant = 'default' }: SummaryListProps) {
  const rows = Children.toArray(children).filter(
    (child): child is ReactElement<InternalSummaryListRowProps> => isValidElement(child),
  )
  const hasActions = rows.some((row) => Children.toArray(row.props.children).length > 0)
  const rootClassName = clsx(
    styles['summary-list'],
    variant === 'two-column' && styles['summary-list--two-column'],
    className,
  )

  return (
    <dl className={rootClassName} data-component="summary-list">
      {rows.map((row) => cloneElement(row, { listHasActions: hasActions, variant }))}
    </dl>
  )
}
