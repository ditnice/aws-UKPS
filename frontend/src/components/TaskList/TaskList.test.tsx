import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { TaskList, TaskListSection, TaskListTask } from './TaskList'

describe('TaskList', () => {
  it('renders sections and tasks inside the task list', () => {
    const { asFragment, container } = render(
      <TaskList className="additional-class">
        <TaskListSection title="Section 1" description="Complete these first" status="in-progress">
          <TaskListTask id="task-1" title="Task 1" href="/task-1" status="completed" />
          <TaskListTask id="task-2" title="Task 2" status="in-progress" />
        </TaskListSection>
        <TaskListSection title="Section 2">
          <TaskListTask id="task-3" title="Task 3" status="not-started" />
        </TaskListSection>
      </TaskList>,
    )

    const wrapper = container.querySelector('[data-component="task-list"]')
    expect(wrapper?.classList.contains('additional-class')).toBe(true)
    expect(screen.getByRole('heading', { level: 2, name: 'Section 1' })).toBeInTheDocument()
    expect(screen.getAllByRole('listitem')).toHaveLength(3)
    expect(screen.getByText('Not started')).toBeInTheDocument()
    expect(asFragment()).toMatchSnapshot()
  })

  it('shows progress and a skip link to the first incomplete section', () => {
    render(
      <TaskList>
        <TaskListSection title="Section 1" status="completed">
          <TaskListTask id="task-1" title="Task 1" status="completed" />
        </TaskListSection>
        <TaskListSection title="Section 2" status="in-progress">
          <TaskListTask id="task-2" title="Task 2" status="in-progress" />
        </TaskListSection>
      </TaskList>,
    )

    expect(screen.getByText('You have completed 1 of 2 sections')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Skip to first incomplete section' })).toHaveAttribute(
      'href',
      '#task-2',
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

    expect(screen.getByText('You have completed 1 of 1 section')).toBeInTheDocument()
    expect(
      screen.queryByRole('link', { name: 'Skip to first incomplete section' }),
    ).not.toBeInTheDocument()
  })

  it('summarises progress by the number of completed tasks', () => {
    render(
      <TaskList>
        <TaskListSection title="Section 1" status="in-progress">
          <TaskListTask id="task-1" title="Task 1" status="in-progress" />
          <TaskListTask id="task-2" title="Task 2" status="in-progress" />
          <TaskListTask id="task-3" title="Task 3" status="completed" />
        </TaskListSection>
      </TaskList>,
    )

    expect(screen.getByText('You have completed 1 of 3 sections')).toBeInTheDocument()
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
  it('renders a task link, status and field progress summary', () => {
    const { asFragment } = render(
      <ul>
        <TaskListTask
          id="task-3"
          title="Review evidence"
          href="/review"
          status="in-progress"
          completedFields={1}
          totalFields={3}
        />
      </ul>,
    )

    expect(screen.getByRole('link', { name: 'Review evidence' })).toHaveAttribute('href', '/review')
    expect(screen.getByText('You have completed 1 of 3 fields')).toBeInTheDocument()
    expect(screen.getByText('In progress')).toBeInTheDocument()
    expect(asFragment()).toMatchSnapshot()
  })
})
