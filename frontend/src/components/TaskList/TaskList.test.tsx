import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { TaskList, TaskListSection, TaskListTask } from './TaskList'

describe('TaskList', () => {
  it('renders sections and tasks inside the task list', () => {
    const { asFragment, container } = render(
      <TaskList className="additional-class">
        <TaskListSection title="Section 1" description="Complete these first" status="in-progress">
          <TaskListTask id="task-1" title="Task 1" href="/task-1" status="completed" />
          <TaskListTask id="task-2" title="Task 2" href="/task-2" status="in-progress" />
        </TaskListSection>
        <TaskListSection title="Section 2">
          <TaskListTask id="task-3" title="Task 3" status="not-started" />
        </TaskListSection>
      </TaskList>,
    )

    const wrapper = container.querySelector('[data-component="task-list"]')
    expect(wrapper?.classList.contains('additional-class')).toBe(true)
    expect(screen.getByRole('heading', { level: 3, name: 'Section 1' })).toBeInTheDocument()
    expect(screen.getAllByRole('listitem')).toHaveLength(3)
    expect(screen.getByText('Not yet started')).toBeInTheDocument()
    expect(screen.getByText('You have completed 0 of 2 mandatory sections')).toBeInTheDocument()
    expect(asFragment()).toMatchSnapshot()
  })

  it('shows progress and a skip link to the first incomplete task', () => {
    render(
      <TaskList>
        <TaskListSection title="Section 1" status="completed">
          <TaskListTask id="task-1" title="Task 1" status="completed" />
        </TaskListSection>
        <TaskListSection title="Section 2" status="in-progress">
          <TaskListTask id="task-2" title="Task 2" href="/task-2" status="in-progress" />
        </TaskListSection>
      </TaskList>,
    )

    expect(screen.getByText('You have completed 1 of 2 mandatory sections')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Skip to first incomplete section' })).toHaveAttribute(
      'href',
      '/task-2',
    )
  })

  it('does not render the skip link when all tasks are completed', () => {
    render(
      <TaskList>
        <TaskListSection title="Section 1" status="completed">
          <TaskListTask id="task-1" title="Task 1" status="completed" />
        </TaskListSection>
      </TaskList>,
    )

    expect(screen.getByText('You have completed 1 of 1 mandatory section')).toBeInTheDocument()
    expect(
      screen.queryByRole('link', { name: 'Skip to first incomplete section' }),
    ).not.toBeInTheDocument()
  })

  it('does not render the skip link when the first incomplete task has no link', () => {
    render(
      <TaskList>
        <TaskListSection title="Section 1">
          <TaskListTask id="task-1" title="Task 1" status="not-started" />
          <TaskListTask id="task-2" title="Task 2" href="/task-2" status="not-started" />
        </TaskListSection>
      </TaskList>,
    )

    expect(screen.getByText('You have completed 0 of 1 mandatory section')).toBeInTheDocument()
    expect(
      screen.queryByRole('link', { name: 'Skip to first incomplete section' }),
    ).not.toBeInTheDocument()
  })

  it('does not render the skip link when the first incomplete section has no task to start', () => {
    render(
      <TaskList>
        <TaskListSection title="Section 1">
          <TaskListTask id="task-1" title="Task 1" href="/task-1" status="completed" />
          <TaskListTask id="task-2" title="Task 2" status="cannot-start" />
        </TaskListSection>
        <TaskListSection title="Section 2">
          <TaskListTask id="task-3" title="Task 3" href="/task-3" status="not-started" />
        </TaskListSection>
      </TaskList>,
    )

    expect(screen.getByText('You have completed 0 of 2 mandatory sections')).toBeInTheDocument()
    expect(
      screen.queryByRole('link', { name: 'Skip to first incomplete section' }),
    ).not.toBeInTheDocument()
  })

  it('counts only mandatory, applicable sections in its progress summary', () => {
    render(
      <TaskList>
        <TaskListSection title="Optional section" optional>
          <TaskListTask
            id="optional-task"
            title="Optional task"
            href="/optional-task"
            status="not-started"
          />
        </TaskListSection>
        <TaskListSection title="Not applicable section">
          <TaskListTask
            id="not-applicable-task"
            title="Not applicable task"
            status="not-applicable"
          />
        </TaskListSection>
        <TaskListSection title="Completed section">
          <TaskListTask id="completed-task" title="Completed task" status="completed" />
          <TaskListTask
            id="completed-section-not-applicable-task"
            title="Not applicable task in completed section"
            status="not-applicable"
          />
        </TaskListSection>
        <TaskListSection title="Incomplete section">
          <TaskListTask id="cannot-start-task" title="Cannot start task" status="cannot-start" />
          <TaskListTask
            id="incomplete-section-completed-task"
            title="Completed task in incomplete section"
            status="completed"
          />
          <TaskListTask
            id="first-incomplete-task"
            title="Incomplete task"
            href="/first-incomplete-task"
            status="in-progress"
          />
          <TaskListTask
            id="not-started-task"
            title="Not started task"
            href="/not-started-task"
            status="not-started"
          />
        </TaskListSection>
      </TaskList>,
    )

    expect(screen.getByText('You have completed 1 of 2 mandatory sections')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Skip to first incomplete section' })).toHaveAttribute(
      'href',
      '/first-incomplete-task',
    )
    expect(screen.getByText('Cannot start yet')).toBeInTheDocument()
    expect(screen.getAllByText('Not applicable')).toHaveLength(2)
  })

  it('does not render the progress summary when there are no mandatory, applicable tasks', () => {
    render(
      <TaskList>
        <TaskListSection title="Optional section" optional>
          <TaskListTask
            id="optional-task"
            title="Optional task"
            href="/optional-task"
            status="not-started"
          />
        </TaskListSection>
        <TaskListSection title="Mandatory section">
          <TaskListTask
            id="not-applicable-task"
            title="Not applicable task"
            status="not-applicable"
          />
        </TaskListSection>
      </TaskList>,
    )

    expect(screen.queryByText(/You have completed/)).not.toBeInTheDocument()
    expect(
      screen.queryByRole('link', { name: 'Skip to first incomplete section' }),
    ).not.toBeInTheDocument()
  })
})

describe('TaskListSection', () => {
  it('renders the title at the given heading level', () => {
    render(
      <TaskListSection title="Provide information" titleHeadingLevel={3}>
        <TaskListTask id="task-1" title="Add case details" status="not-started" />
      </TaskListSection>,
    )

    expect(
      screen.getByRole('heading', { level: 3, name: 'Provide information' }),
    ).toBeInTheDocument()
  })
})

describe('TaskListTask', () => {
  it('renders a task link and status', () => {
    const { asFragment } = render(
      <ul>
        <TaskListTask id="task-3" title="Review evidence" href="/review" status="in-progress" />
      </ul>,
    )

    expect(screen.getByRole('link', { name: 'Review evidence' })).toHaveAttribute('href', '/review')
    expect(screen.getByText('In progress')).toBeInTheDocument()
    expect(asFragment()).toMatchSnapshot()
  })

  it.each([
    ['cannot-start', 'Cannot start yet'],
    ['not-applicable', 'Not applicable'],
  ] as const)('renders the %s status as plain text', (status, label) => {
    const { asFragment } = render(
      <ul>
        <TaskListTask title="Special designation" status={status} />
      </ul>,
    )

    expect(screen.getByText(label)).not.toHaveAttribute('data-component', 'tag')
    expect(screen.queryByRole('link')).not.toBeInTheDocument()
    expect(asFragment()).toMatchSnapshot()
  })
})
