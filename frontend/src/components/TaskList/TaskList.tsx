'use client'

import clsx from 'clsx'
import { useState, Children, isValidElement, ReactElement } from 'react'

import ChevronDownIcon from '@nice-digital/icons/lib/ChevronDown'

import { Tag } from '@/components/Tag/Tag'

import styles from './TaskList.module.scss'

import type { ReactNode } from 'react'

export type TaskStatus = 'not-started' | 'in-progress' | 'completed'

export type TaskListProps = {
  children: ReactNode
  className?: string
}

export type TaskListSectionProps = {
  children: ReactNode
  className?: string
  collapsible?: boolean
  defaultExpanded?: boolean
  description?: ReactNode
  status?: TaskStatus
  title: ReactNode
  titleHeadingLevel?: 2 | 3 | 4 | 5 | 6
}

export type TaskListTaskProps = {
  className?: string
  completedFields?: number
  description?: ReactNode
  href?: string
  id?: string
  title: ReactNode
  status: TaskStatus
  totalFields?: number
}

export type TaskListSummaryProps = {
  completed: number
  total: number
  firstIncompleteId?: string
}

const taskStatusLabels: Record<TaskStatus, string> = {
  'not-started': 'Not started',
  'in-progress': 'In progress',
  completed: 'Completed',
}

const taskStatusColours: Record<Exclude<TaskStatus, 'completed'>, 'blue' | 'yellow'> = {
  'not-started': 'blue',
  'in-progress': 'yellow',
}

function TaskStatusLabel({ className, status }: { className: string; status: TaskStatus }) {
  if (status === 'completed') {
    return <span className={className}>{taskStatusLabels[status]}</span>
  }

  return (
    <Tag className={className} colour={taskStatusColours[status]}>
      {taskStatusLabels[status]}
    </Tag>
  )
}

export function TaskList({ children, className }: TaskListProps) {
  const tasks = collectTasks(children)
  const completed = tasks.filter((task) => task.props.status === 'completed').length
  const firstIncomplete = tasks.find((task) => task.props.status !== 'completed')

  return (
    <>
      <TaskListSummary
        completed={completed}
        total={tasks.length}
        firstIncompleteId={firstIncomplete?.props.id}
      />

      <div className={clsx(styles['task-list'], className)} data-component="task-list">
        {children}
      </div>
    </>
  )
}

export function TaskListSummary({ completed, total, firstIncompleteId }: TaskListSummaryProps) {
  return (
    <div className={styles['task-list__summary']}>
      <p className={styles['task-list__summary-text']}>
        You have completed {completed} of {total} {total === 1 ? 'section' : 'sections'}
      </p>
      {firstIncompleteId && (
        <a className={styles['task-list__summary-link']} href={`#${firstIncompleteId}`}>
          Skip to first incomplete section
        </a>
      )}
    </div>
  )
}

export function TaskListSection({
  children,
  className,
  collapsible = false,
  defaultExpanded = false,
  description,
  status,
  title,
  titleHeadingLevel = 2,
}: TaskListSectionProps) {
  const [expanded, setExpanded] = useState(defaultExpanded)
  const TitleHeading = `h${titleHeadingLevel}` as const

  return (
    <section className={clsx(styles['task-list__section'], className)}>
      <TitleHeading
        className={clsx(
          styles['task-list__section-title'],
          collapsible && styles['task-list__section-title--collapsible'],
        )}
      >
        {title}
      </TitleHeading>

      {(description || status) && (
        <div className={styles['task-list__section-summary']}>
          <div className={styles['task-list__section-description']}>{description}</div>
          {status && <TaskStatusLabel className={styles['task-list__status']} status={status} />}
        </div>
      )}

      {collapsible && (
        <div className={styles['task-list__section-controls']}>
          <button
            type="button"
            className={styles['task-list__toggle']}
            aria-expanded={expanded}
            onClick={() => setExpanded((current) => !current)}
          >
            <ChevronDownIcon
              className={clsx(
                styles['task-list__toggle-icon'],
                expanded && styles['task-list__toggle-icon--expanded'],
              )}
            />
            <span className={styles['task-list__toggle-text']}>
              {expanded ? 'Hide all' : 'Show all'}
            </span>
          </button>
        </div>
      )}

      <ul className={styles['task-list__tasks']} hidden={collapsible && !expanded}>
        {children}
      </ul>
    </section>
  )
}

export function TaskListTask({
  className,
  completedFields,
  description,
  href,
  id,
  title,
  status,
  totalFields,
}: TaskListTaskProps) {
  return (
    <li id={id} className={clsx(styles['task-list__task'], className)}>
      <div className={styles['task-list__task-content']}>
        <div className={styles['task-list__task-title']}>
          {completedFields !== undefined && totalFields !== undefined && (
            <p className={styles['task-list__task-field-summary']}>
              You have completed {completedFields} of {totalFields}{' '}
              {totalFields === 1 ? 'field' : 'fields'}
            </p>
          )}
          {href ? (
            <a className={styles['task-list__task-link']} href={href}>
              {title}
            </a>
          ) : (
            title
          )}
        </div>
        {description && <div className={styles['task-list__task-description']}>{description}</div>}
      </div>
      <TaskStatusLabel className={styles['task-list__status']} status={status} />
    </li>
  )
}

type AnyProps = { children?: ReactNode; status?: TaskStatus; id?: string }

function collectTasks(children: ReactNode): ReactElement<AnyProps>[] {
  return Children.toArray(children).flatMap((child) => {
    if (!isValidElement<AnyProps>(child)) return []

    if (child.props.status && child.props.children === undefined) {
      return [child]
    }

    return collectTasks(child.props.children)
  })
}
