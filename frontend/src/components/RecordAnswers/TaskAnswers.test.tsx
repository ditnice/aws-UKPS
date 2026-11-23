import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import type { RecordAnswersQuestionDto } from '@/client/generated'
import { SummaryListAction } from '@/components/SummaryList/SummaryList'

import { TaskAnswers } from './TaskAnswers'

afterEach(cleanup)

const question = (overrides: Partial<RecordAnswersQuestionDto>): RecordAnswersQuestionDto => ({
  id: 'question',
  pageId: 'page',
  label: 'Question',
  type: 'Text',
  multiple: false,
  values: [],
  selectedOptions: null,
  itemLabel: null,
  items: null,
  addAnotherLabel: null,
  ...overrides,
})

const before = question({ id: 'before', label: 'Before', values: ['First'] })
const trials = question({
  id: 'trials',
  label: 'Enter clinical trial details',
  type: 'Group',
  itemLabel: 'Clinical trial',
  items: [
    { questions: [question({ id: 'study_name', label: 'Study name', values: ['SURVEYOR-1'] })] },
    { questions: [question({ id: 'study_name', label: 'Study name' })] },
  ],
})
const after = question({ id: 'after', label: 'After' })

/** The value of the row with the given label; a summary list puts each value after its label. */
const rowValue = (label: string) =>
  screen.getByText(label, { selector: 'dt' }).nextElementSibling?.textContent

describe('TaskAnswers', () => {
  it('shows rows and group item cards in form order', () => {
    render(<TaskAnswers questions={[before, trials, after]} groupItemHeadingLevel={3} />)

    const order = [
      screen.getByText('Before', { selector: 'dt' }),
      screen.getByRole('heading', { level: 3, name: 'Clinical trial 1' }),
      screen.getByRole('heading', { level: 3, name: 'Clinical trial 2' }),
      screen.getByText('After', { selector: 'dt' }),
    ]
    order
      .slice(1)
      .forEach((element, index) =>
        expect(order[index].compareDocumentPosition(element)).toBe(
          Node.DOCUMENT_POSITION_FOLLOWING,
        ),
      )
    expect(rowValue('Before')).toBe('First')
    expect(rowValue('After')).toBe('Not yet provided')
    expect(screen.getByText('SURVEYOR-1')).toBeDefined()
  })

  it('gives rows and group items the actions and tags passed', () => {
    render(
      <TaskAnswers
        questions={[before, trials]}
        groupItemHeadingLevel={3}
        rowTag={() => <span>Problem</span>}
        rowAction={(q) => (
          <SummaryListAction href={`/${q.id}`} visuallyHiddenText={q.label}>
            Change
          </SummaryListAction>
        )}
        groupItemAction={(q, index, title) => (
          <SummaryListAction href={`/${q.id}#${index}`} visuallyHiddenText={title}>
            Edit
          </SummaryListAction>
        )}
      />,
    )

    expect(screen.getByRole('link', { name: /Change\s*Before/ }).getAttribute('href')).toBe(
      '/before',
    )
    expect(screen.getByRole('link', { name: /Edit\s*Clinical trial 2/ }).getAttribute('href')).toBe(
      '/trials#1',
    )
    // Only the plain row is tagged; group item rows are not.
    expect(screen.getAllByText('Problem')).toHaveLength(1)
  })

  it('shows a group tag once, above its items', () => {
    render(
      <TaskAnswers
        questions={[trials]}
        groupItemHeadingLevel={3}
        groupTag={(q) => <span>Problem with {q.label}</span>}
      />,
    )

    const tag = screen.getByText('Problem with Enter clinical trial details')
    const firstTrial = screen.getByRole('heading', { level: 3, name: 'Clinical trial 1' })
    expect(tag.compareDocumentPosition(firstTrial)).toBe(Node.DOCUMENT_POSITION_FOLLOWING)
  })

  it('shows no actions when none are passed', () => {
    render(<TaskAnswers questions={[before, trials]} groupItemHeadingLevel={3} />)

    expect(screen.queryByRole('link')).toBeNull()
  })
})
