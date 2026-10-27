import clsx from 'clsx'
import { Children, isValidElement } from 'react'

import { Tag } from '@/components/Tag/Tag'
import type { TagColour } from '@/components/Tag/Tag'

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
  /** Excludes this section's tasks from the progress summary. Does not change how the section renders. */
  optional?: boolean
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
  firstIncompleteHref?: string
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
} as const satisfies Record<Exclude<TaskStatus, 'cannot-start' | 'not-applicable'>, TagColour>

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
  const sections = collectSections(children)
    .filter((section) => !section.props.optional)
    .map((section) =>
      collectTasks(section.props.children).filter((task) => task.props.status !== 'not-applicable'),
    )
  const tasks = sections.flat()
  const completed = tasks.filter((task) => task.props.status === 'completed').length
  // Only the first incomplete section is searched. If none of its tasks can be started yet, the
  // skip link is hidden rather than jumping past that section to a later one.
  const firstIncomplete = sections
    .find((sectionTasks) => sectionTasks.some((task) => task.props.status !== 'completed'))
    ?.find((task) => task.props.status === 'not-started' || task.props.status === 'in-progress')

  return (
    <>
      {tasks.length > 0 && (
        <TaskListSummary
          completed={completed}
          total={tasks.length}
          firstIncompleteHref={firstIncomplete?.props.href}
        />
      )}

      <div className={className} data-component="task-list">
        {children}
      </div>
    </>
  )
}

export function TaskListSummary({ completed, total, firstIncompleteHref }: TaskListSummaryProps) {
  return (
    <div className={styles['task-list__summary']}>
      <p className="lead">
        You have completed {completed} of {total} mandatory {total === 1 ? 'task' : 'tasks'}
      </p>
      {firstIncompleteHref && <a href={firstIncompleteHref}>Skip to first incomplete task</a>}
    </div>
  )
}

export function TaskListSection({
  children,
  className,
  description,
  title,
  titleHeadingLevel = 3,
}: TaskListSectionProps) {
  const TitleHeading = `h${titleHeadingLevel}` as const

  return (
    <section className={className}>
      <TitleHeading className={styles['task-list__section-title']}>{title}</TitleHeading>

      {description && <div className={styles['task-list__section-summary']}>{description}</div>}

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
      <div>
        <div>{href ? <a href={href}>{title}</a> : title}</div>
        {description && <div className={styles['task-list__task-description']}>{description}</div>}
      </div>
      <TaskStatusLabel className={styles['task-list__status']} status={status} />
    </li>
  )
}

// Sections and tasks are found by component type, looking through fragments and plain elements.
// Wrapping them in another component hides them from the summary.
function collectSections(children: ReactNode): ReactElement<TaskListSectionProps>[] {
  return Children.toArray(children).flatMap((child) => {
    if (!isValidElement<{ children?: ReactNode }>(child)) return []

    if (child.type === TaskListSection) {
      return [child as ReactElement<TaskListSectionProps>]
    }

    return collectSections(child.props.children)
  })
}

function collectTasks(children: ReactNode): ReactElement<TaskListTaskProps>[] {
  return Children.toArray(children).flatMap((child) => {
    if (!isValidElement<{ children?: ReactNode }>(child)) return []

    if (child.type === TaskListTask) {
      return [child as ReactElement<TaskListTaskProps>]
    }

    return collectTasks(child.props.children)
  })
}
