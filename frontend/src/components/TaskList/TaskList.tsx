'use client'

import clsx from 'clsx'
import { Children, isValidElement } from 'react'

import { Tag } from '@/components/Tag/Tag'

import styles from './TaskList.module.scss'

import type { ReactElement, ReactNode } from 'react'

export type TaskStatus =
  'not-started' | 'in-progress' | 'completed' | 'cannot-start' | 'not-applicable'

export type TaskListProps = {
  children: ReactNode
  className?: string
}

export type TaskListSectionProps = {
  children: ReactNode
  className?: string
  description?: ReactNode
  optional?: boolean
  status?: TaskStatus
  title: ReactNode
  titleHeadingLevel?: 2 | 3 | 4 | 5 | 6
}

export type TaskListTaskProps = {
  className?: string
  description?: ReactNode
  href?: string
  id?: string
  status: TaskStatus
  title: ReactNode
}

export type TaskListSummaryProps = {
  completed: number
  total: number
  firstIncompleteId?: string
}

const taskStatusLabels: Record<TaskStatus, string> = {
  'not-started': 'Not yet started',
  'in-progress': 'In progress',
  completed: 'Completed',
  'cannot-start': 'Cannot start yet',
  'not-applicable': 'Not applicable',
}

const taskStatusColours = {
  'not-started': 'blue',
  'in-progress': 'yellow',
  completed: 'green',
} as const

function TaskStatusLabel({ className, status }: { className?: string; status: TaskStatus }) {
  if (status === 'cannot-start' || status === 'not-applicable') {
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
  const firstIncomplete = tasks.find(
    (task) => task.props.status === 'not-started' || task.props.status === 'in-progress',
  )

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
      <p className={clsx('lead', styles['task-list__summary-text'])}>
        You have completed {completed} of {total} mandatory {total === 1 ? 'section' : 'sections'}
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
  description,
  status,
  title,
  titleHeadingLevel = 3,
}: TaskListSectionProps) {
  const TitleHeading = `h${titleHeadingLevel}` as const

  return (
    <section className={clsx(styles['task-list__section'], className)}>
      <TitleHeading className={styles['task-list__section-title']}>{title}</TitleHeading>

      {(description || status) && (
        <div className={styles['task-list__section-summary']}>
          <div className={styles['task-list__section-description']}>{description}</div>
          {status && <TaskStatusLabel className={styles['task-list__status']} status={status} />}
        </div>
      )}

      <ul className={styles['task-list__tasks']}>{children}</ul>
    </section>
  )
}

export function TaskListTask({
  className,
  description,
  href,
  id,
  status,
  title,
}: TaskListTaskProps) {
  return (
    <li id={id} className={clsx(styles['task-list__task'], className)}>
      <div className={styles['task-list__task-content']}>
        <div className={styles['task-list__task-title']}>
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

type AnyProps = { children?: ReactNode; status?: TaskStatus; id?: string; optional?: boolean }

function collectTasks(children: ReactNode): ReactElement<AnyProps>[] {
  return Children.toArray(children).flatMap((child) => {
    if (!isValidElement<AnyProps>(child)) return []

    if (child.props.optional) return []

    if (child.props.status && child.props.children === undefined) {
      return child.props.status === 'not-applicable' ? [] : [child]
    }

    return collectTasks(child.props.children)
  })
}
