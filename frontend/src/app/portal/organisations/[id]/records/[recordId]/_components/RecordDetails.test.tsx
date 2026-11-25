import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import type { RecordAnswersQuestionDto, RecordAnswersSectionDto } from '@/client/generated'

import { RecordDetails } from './RecordDetails'

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
  checkAnswersAddLabel: null,
  ...overrides,
})

const sections: RecordAnswersSectionDto[] = [
  {
    id: 'product-record-details',
    title: 'Product record details',
    optional: false,
    tasks: [
      {
        id: 'names-and-identifiers',
        title: 'Names and identifiers',
        notApplicable: false,
        questions: [question({ id: 'company_code', label: 'Company code', values: ['ABC-123'] })],
      },
    ],
  },
  {
    id: 'clinical-trials',
    title: 'Clinical trials',
    optional: false,
    tasks: [
      {
        id: 'clinical-trials',
        title: 'Clinical trial details',
        notApplicable: false,
        questions: [
          question({
            id: 'record_clinical_trial',
            label: 'Enter clinical trial details',
            type: 'Group',
            itemLabel: 'Clinical trial',
            checkAnswersAddLabel: 'Add clinical trial',
            items: [
              {
                questions: [
                  question({ id: 'study_name', label: 'Study name', values: ['SURVEYOR-1'] }),
                ],
              },
            ],
          }),
        ],
      },
    ],
  },
]

// Accordions start collapsed, so their content is hidden from accessibility queries.
const heading = (level: number, name: string) =>
  screen.getByRole('heading', { level, name, hidden: true })

describe('RecordDetails', () => {
  it('shows an accordion per section and a card per task', () => {
    render(<RecordDetails sections={sections} />)

    expect(screen.getByText('Product record details')).toBeDefined()
    expect(heading(4, 'Names and identifiers')).toBeDefined()
    expect(screen.getByText('Company code')).toBeDefined()
    expect(screen.getByText('ABC-123')).toBeDefined()
  })

  it('shows each group item as its own card', () => {
    render(<RecordDetails sections={sections} />)

    expect(heading(5, 'Clinical trial 1')).toBeDefined()
    expect(screen.getByText('SURVEYOR-1')).toBeDefined()
  })

  it('offers no way to change answers', () => {
    render(<RecordDetails sections={sections} />)

    expect(screen.queryByRole('link', { hidden: true })).toBeNull()
    expect(screen.queryByText('Add clinical trial')).toBeNull()
  })
})
