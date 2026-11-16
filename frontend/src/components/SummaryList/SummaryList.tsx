import clsx from 'clsx'
import { Children, cloneElement, isValidElement } from 'react'

import { Panel } from '@nice-digital/nds-panel'

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
  /* Shown at the right of the value cell, e.g. a "Hidden" tag. */
  tag?: ReactNode
  value: ReactNode
}

type InternalSummaryListRowProps = SummaryListRowProps & {
  listHasActions?: boolean
  variant?: SummaryListVariant
}

type ValidSummaryListRow = ReactElement<InternalSummaryListRowProps> | boolean | null | undefined

type SummaryListProps = {
  /* Links on the title line, e.g. `SummaryListAction`s such as "Edit section". */
  actions?: ReactNode
  /* Draws the title and rows inside an NDS panel. */
  card?: boolean
  children?: ValidSummaryListRow | readonly ValidSummaryListRow[]
  className?: string
  /* Content between the title and the rows, e.g. "Not yet provided" for a section with no rows. */
  intro?: ReactNode
  title?: ReactNode
  /* Defaults to 4. */
  titleHeadingLevel?: 2 | 3 | 4 | 5 | 6
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
  tag,
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
      <dd className={styles['summary-list__value']}>
        {tag ? (
          <div className={styles['summary-list__value-with-tag']}>
            <div>{value}</div>
            {tag}
          </div>
        ) : (
          value
        )}
      </dd>
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

export function SummaryList({
  actions,
  card = false,
  children,
  className,
  intro,
  title,
  titleHeadingLevel = 4,
  variant = 'default',
}: SummaryListProps) {
  const rows = Children.toArray(children).filter(
    (child): child is ReactElement<InternalSummaryListRowProps> => isValidElement(child),
  )
  const hasActions = rows.some((row) => Children.toArray(row.props.children).length > 0)
  const rootClassName = clsx(
    styles['summary-list'],
    variant === 'two-column' && styles['summary-list--two-column'],
    className,
  )

  const TitleHeading = `h${titleHeadingLevel}` as const
  const titleElement = title ? (
    <TitleHeading className={styles['summary-list__title']}>{title}</TitleHeading>
  ) : null
  const header = actions ? (
    <div className={styles['summary-list__header']}>
      {titleElement}
      <div className={styles['summary-list__header-actions']}>{actions}</div>
    </div>
  ) : (
    titleElement
  )

  const content = (
    <>
      {header}
      {intro}
      {rows.length > 0 && (
        <dl className={rootClassName} data-component="summary-list">
          {rows.map((row) => cloneElement(row, { listHasActions: hasActions, variant }))}
        </dl>
      )}
    </>
  )

  return card ? <Panel className={styles['summary-list__card']}>{content}</Panel> : content
}
