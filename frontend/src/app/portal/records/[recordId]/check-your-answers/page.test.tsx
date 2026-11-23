import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { getRecordCheckAnswers } from '@/client/generated'
import type {
  RecordAnswersGroupItemDto,
  RecordAnswersQuestionDto,
  RecordAnswersSectionDto,
} from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'

import CheckAnswersPage from './page'

const mocks = vi.hoisted(() => ({
  notFound: vi.fn(() => {
    throw new Error('NEXT_NOT_FOUND')
  }),
  actions: vi.fn(),
}))

vi.mock('@/client/generated', () => ({
  getRecordCheckAnswers: vi.fn(),
}))

vi.mock('@/client/server-api', () => ({
  createServerApiClient: vi.fn(),
}))

vi.mock('next/navigation', () => ({
  notFound: mocks.notFound,
}))

// The real component is a client component that needs the router; its own test covers it.
vi.mock('./_components/CheckAnswersActions', () => ({
  CheckAnswersActions: (props: unknown) => {
    mocks.actions(props)
    return <div>Submit and return actions</div>
  },
}))

type GetRecordCheckAnswersResult = Awaited<ReturnType<typeof getRecordCheckAnswers>>

const checkAnswers = {
  data: {
    formVersion: 'medicine-1',
    recordId: 42,
    organisationId: 7,
    revisionHash: 'hash-1',
    sections: [
      {
        id: 'names',
        title: 'Names',
        optional: false,
        tasks: [
          {
            id: 'names-and-identifiers',
            title: 'Names and identifiers',
            notApplicable: false,
            questions: [
              {
                id: 'company-code',
                pageId: 'names',
                label: 'Company code',
                type: 'Text',
                multiple: false,
                values: ['GC22'],
                selectedOptions: null,
                itemLabel: null,
                addAnotherLabel: null,
                items: null,
              },
              {
                id: 'generic-name',
                pageId: 'names',
                label: 'Generic name',
                type: 'Text',
                multiple: false,
                values: [],
                selectedOptions: null,
                itemLabel: null,
                addAnotherLabel: null,
                items: null,
              },
            ],
          },
        ],
      },
    ],
    errors: [],
  },
  error: undefined,
  response: new Response(null, { status: 200 }),
} satisfies GetRecordCheckAnswersResult

const genericNameError = {
  questionId: 'generic-name',
  pageId: 'names',
  taskId: 'names-and-identifiers',
  message: 'Enter a generic name',
}

const trialItem = (studyName: string): RecordAnswersGroupItemDto => ({
  questions: [
    {
      id: 'study_name',
      pageId: 'clinical-trial-details',
      label: 'Study name',
      type: 'Text',
      multiple: false,
      values: [studyName],
      selectedOptions: null,
      itemLabel: null,
      addAnotherLabel: null,
      items: null,
    },
  ],
})

const trialsCheckAnswers = {
  ...checkAnswers,
  data: {
    ...checkAnswers.data,
    sections: [
      {
        id: 'clinical-trials',
        title: 'Clinical trials',
        optional: false,
        tasks: [
          {
            id: 'clinical-trials',
            title: 'Clinical trials',
            notApplicable: false,
            questions: [
              {
                id: 'has_clinical_trials',
                pageId: 'add-clinical-trial',
                label: 'Do you wish to add a clinical trial?',
                type: 'Radio',
                multiple: false,
                values: ['Yes'],
                selectedOptions: null,
                itemLabel: null,
                addAnotherLabel: null,
                items: null,
              },
              {
                id: 'record_clinical_trial',
                pageId: 'clinical-trial-details',
                label: 'Enter clinical trial details',
                type: 'Group',
                multiple: false,
                values: [],
                selectedOptions: null,
                itemLabel: 'Clinical trial',
                addAnotherLabel: 'Add another clinical trial',
                items: [trialItem('SURVEYOR-1'), trialItem('SURVEYOR-2')],
              },
            ],
          },
        ],
      },
    ],
  },
} satisfies GetRecordCheckAnswersResult

type Section = RecordAnswersSectionDto
type Question = RecordAnswersQuestionDto

const textQuestion = (id: string, pageId: string, label: string, values: string[] = []) =>
  ({
    id,
    pageId,
    label,
    type: 'Text',
    multiple: false,
    values,
    selectedOptions: null,
    itemLabel: null,
    addAnotherLabel: null,
    items: null,
  }) satisfies Question

const withSections = (sections: Section[]) =>
  ({
    ...checkAnswers,
    data: { ...checkAnswers.data, sections },
  }) satisfies GetRecordCheckAnswersResult

const section = (
  task: { id: string; title: string; notApplicable?: boolean; questions: Question[] },
  optional = false,
): Section => ({
  id: optional ? 'service-readiness' : 'names',
  title: 'Section',
  optional,
  tasks: [{ notApplicable: false, ...task }],
})

const recordTitleId = 'record_product_detail.record_title'

const renderPage = async (recordId = '42') => {
  const page = await CheckAnswersPage({ params: Promise.resolve({ recordId }) })
  render(page)
}

beforeEach(() => {
  vi.clearAllMocks()
  vi.mocked(createServerApiClient).mockResolvedValue(
    {} as Awaited<ReturnType<typeof createServerApiClient>>,
  )
  vi.mocked(getRecordCheckAnswers).mockResolvedValue(checkAnswers)
})

afterEach(cleanup)

describe('CheckAnswersPage', () => {
  it('renders each task with its answers and a Change link for each question', async () => {
    await renderPage()

    expect(getRecordCheckAnswers).toHaveBeenCalledWith(
      expect.objectContaining({ path: { recordId: 42 } }),
    )
    expect(screen.getByRole('link', { name: 'Back' })).toHaveAttribute(
      'href',
      '/portal/records/42/task-list',
    )
    expect(screen.getByText('Names and identifiers')).toBeInTheDocument()
    expect(screen.getByText('GC22')).toBeInTheDocument()
    expect(screen.getByText('Not yet provided')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /Change\s*Company code/ })).toHaveAttribute(
      'href',
      '/portal/records/42/names?returnTo=check-your-answers',
    )
  })

  it('shows each item of a group as its own card with an Edit link to that item', async () => {
    vi.mocked(getRecordCheckAnswers).mockResolvedValue(trialsCheckAnswers)

    await renderPage()

    expect(screen.getByRole('heading', { level: 3, name: 'Clinical trial 1' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { level: 3, name: 'Clinical trial 2' })).toBeInTheDocument()
    expect(screen.getByText('SURVEYOR-1')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /Edit\s*Clinical trial 2/ })).toHaveAttribute(
      'href',
      '/portal/records/42/clinical-trial-details?returnTo=check-your-answers#record_clinical_trial-item-1',
    )
  })

  it('offers to add another item, opening it after the saved ones', async () => {
    vi.mocked(getRecordCheckAnswers).mockResolvedValue(trialsCheckAnswers)

    await renderPage()

    expect(screen.getByRole('link', { name: 'Add another clinical trial' })).toHaveAttribute(
      'href',
      '/portal/records/42/clinical-trial-details?returnTo=check-your-answers&add=group-item#record_clinical_trial-item-2',
    )
  })

  it('shows a question that comes before a group above its items', async () => {
    vi.mocked(getRecordCheckAnswers).mockResolvedValue(trialsCheckAnswers)

    await renderPage()

    const question = screen.getByText('Do you wish to add a clinical trial?', { selector: 'dt' })
    const firstTrial = screen.getByRole('heading', { level: 3, name: 'Clinical trial 1' })
    expect(question.compareDocumentPosition(firstTrial)).toBe(Node.DOCUMENT_POSITION_FOLLOWING)
  })

  it('does not show a group as a Change row', async () => {
    vi.mocked(getRecordCheckAnswers).mockResolvedValue(trialsCheckAnswers)

    await renderPage()

    expect(
      screen.queryByRole('link', { name: /Change\s*Enter clinical trial details/ }),
    ).not.toBeInTheDocument()
  })

  it('tags a group with an error once, naming the group', async () => {
    vi.mocked(getRecordCheckAnswers).mockResolvedValue({
      ...trialsCheckAnswers,
      data: {
        ...trialsCheckAnswers.data,
        errors: [
          {
            questionId: 'record_clinical_trial',
            pageId: 'clinical-trial-details',
            taskId: 'clinical-trials',
            message: 'Add a clinical trial',
          },
        ],
      },
    })

    await renderPage()

    const tags = screen.getAllByText(
      (_, element) =>
        element?.textContent === 'There is a problem with Enter clinical trial details' &&
        element.tagName === 'SPAN',
    )
    expect(tags).toHaveLength(1)
    const firstTrial = screen.getByRole('heading', { level: 3, name: 'Clinical trial 1' })
    expect(tags[0].compareDocumentPosition(firstTrial)).toBe(Node.DOCUMENT_POSITION_FOLLOWING)
  })

  it('marks an optional task and says when it becomes mandatory', async () => {
    vi.mocked(getRecordCheckAnswers).mockResolvedValue(
      withSections([
        section(
          {
            id: 'laboratory-testing',
            title: 'Laboratory testing',
            questions: [textQuestion('diagnostic', 'diagnostic-test', 'Needs a test?', ['No'])],
          },
          true,
        ),
      ]),
    )

    await renderPage()

    expect(screen.getByRole('heading', { name: 'Laboratory testing (optional)' })).toBeVisible()
    expect(
      screen.getByText(
        'This task becomes mandatory within 12 months of the market authorisation date.',
      ),
    ).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /Change\s*Needs a test\?/ })).toBeInTheDocument()
    expect(screen.queryByRole('link', { name: /Edit section/ })).not.toBeInTheDocument()
  })

  it('shows an untouched optional task as one "Not yet provided" row with Edit section', async () => {
    vi.mocked(getRecordCheckAnswers).mockResolvedValue(
      withSections([
        section(
          {
            id: 'patient-and-clinical',
            title: 'Patient and clinical requirements',
            questions: [
              textQuestion('screening', 'screening-page', 'Is screening needed?'),
              textQuestion('details', 'details-page', 'Screening details'),
            ],
          },
          true,
        ),
      ]),
    )

    await renderPage()

    expect(screen.getAllByText('Not yet provided')).toHaveLength(1)
    expect(screen.queryByText('Is screening needed?')).not.toBeInTheDocument()
    expect(screen.queryByRole('link', { name: /Change/ })).not.toBeInTheDocument()
    expect(
      screen.getByRole('link', { name: /Edit section\s*Patient and clinical requirements/ }),
    ).toHaveAttribute('href', '/portal/records/42/screening-page?returnTo=check-your-answers')
  })

  it('does not collapse an unanswered task that is not optional', async () => {
    vi.mocked(getRecordCheckAnswers).mockResolvedValue(
      withSections([
        section({
          id: 'names-and-identifiers',
          title: 'Names and identifiers',
          questions: [textQuestion('generic-name', 'names', 'Generic name')],
        }),
      ]),
    )

    await renderPage()

    expect(screen.queryByRole('link', { name: /Edit section/ })).not.toBeInTheDocument()
    expect(screen.getByRole('link', { name: /Change\s*Generic name/ })).toBeInTheDocument()
  })

  it('explains a task that does not apply, with no rows', async () => {
    vi.mocked(getRecordCheckAnswers).mockResolvedValue(
      withSections([
        section({ id: 'pim-and-eams', title: 'PIM and EAMS', notApplicable: true, questions: [] }),
      ]),
    )

    await renderPage()

    expect(
      screen.getByText(/Your product is a biosimilar, so you do not need to complete this task/),
    ).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'indication details' })).toHaveAttribute(
      'href',
      expect.stringContaining('technology-status'),
    )
    expect(screen.queryByRole('link', { name: /Change|Edit section/ })).not.toBeInTheDocument()
  })

  it('tags the record title as hidden, and no other row', async () => {
    vi.mocked(getRecordCheckAnswers).mockResolvedValue(
      withSections([
        section({
          id: 'names-and-identifiers',
          title: 'Names and identifiers',
          questions: [
            textQuestion('company-code', 'names', 'Company code', ['GC22']),
            textQuestion(recordTitleId, 'names', 'Record title', ['A title']),
          ],
        }),
      ]),
    )

    await renderPage()

    expect(screen.getAllByText('Hidden')).toHaveLength(1)
  })

  it('shows both tags on a hidden row that also has an error', async () => {
    const titleOnly = withSections([
      section({
        id: 'names-and-identifiers',
        title: 'Names and identifiers',
        questions: [textQuestion(recordTitleId, 'names', 'Record title', ['A title'])],
      }),
    ])
    vi.mocked(getRecordCheckAnswers).mockResolvedValue({
      ...titleOnly,
      data: {
        ...titleOnly.data,
        errors: [
          {
            questionId: recordTitleId,
            pageId: 'names',
            taskId: 'names-and-identifiers',
            message: 'Fix the title',
          },
        ],
      },
    })

    await renderPage()

    expect(screen.getByText('Hidden')).toBeInTheDocument()
    // One in the summary heading and one on the row's tag.
    expect(screen.getAllByText('There is a problem')).toHaveLength(2)
  })

  it('leaves a group with no items untagged, its error only in the summary', async () => {
    const [section] = trialsCheckAnswers.data.sections
    const [task] = section.tasks
    vi.mocked(getRecordCheckAnswers).mockResolvedValue({
      ...trialsCheckAnswers,
      data: {
        ...trialsCheckAnswers.data,
        sections: [
          {
            ...section,
            tasks: [
              {
                ...task,
                questions: task.questions.map((question) =>
                  question.id === 'record_clinical_trial' ? { ...question, items: [] } : question,
                ),
              },
            ],
          },
        ],
        errors: [
          {
            questionId: 'record_clinical_trial',
            pageId: 'clinical-trial-details',
            taskId: 'clinical-trials',
            message: 'Add at least one clinical trial',
          },
        ],
      },
    })

    await renderPage()

    expect(screen.getByRole('link', { name: 'Add at least one clinical trial' })).toBeVisible()
    expect(screen.queryByText(/There is a problem with/)).not.toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Add another clinical trial' })).toBeInTheDocument()
  })

  it('shows no error summary or problem tags when there are no errors', async () => {
    await renderPage()

    expect(screen.queryByText('There is a problem')).not.toBeInTheDocument()
  })

  it('shows the errors in a summary that links to each question, and tags its row', async () => {
    vi.mocked(getRecordCheckAnswers).mockResolvedValue({
      ...checkAnswers,
      data: { ...checkAnswers.data, errors: [genericNameError] },
    })

    await renderPage()

    expect(screen.getByRole('link', { name: 'Enter a generic name' })).toHaveAttribute(
      'href',
      '/portal/records/42/names?returnTo=check-your-answers#generic-name',
    )
    // One in the summary heading and one on the row's tag.
    expect(screen.getAllByText('There is a problem')).toHaveLength(2)
  })

  it('passes the submission details to the actions', async () => {
    await renderPage()

    expect(screen.getByText('Submit and return actions')).toBeInTheDocument()
    expect(mocks.actions).toHaveBeenCalledWith({
      recordId: 42,
      formVersion: 'medicine-1',
      revisionHash: 'hash-1',
      organisationId: 7,
      canSubmit: true,
    })
  })

  it('still offers the actions when there are errors, but not Submit', async () => {
    vi.mocked(getRecordCheckAnswers).mockResolvedValue({
      ...checkAnswers,
      data: { ...checkAnswers.data, errors: [genericNameError] },
    })

    await renderPage()

    expect(screen.getByText('Submit and return actions')).toBeInTheDocument()
    expect(mocks.actions).toHaveBeenCalledWith(expect.objectContaining({ canSubmit: false }))
  })

  it('returns not found for an invalid record ID', async () => {
    await expect(renderPage('abc')).rejects.toThrow('NEXT_NOT_FOUND')
    expect(getRecordCheckAnswers).not.toHaveBeenCalled()
  })

  it('returns not found when the API says so', async () => {
    vi.mocked(getRecordCheckAnswers).mockResolvedValue({
      data: undefined,
      response: { status: 404 },
    } as never)

    await expect(renderPage()).rejects.toThrow('NEXT_NOT_FOUND')
  })

  it.each([403, 409])(
    'says the record cannot be submitted, without the answers, when the API returns %i',
    async (status) => {
      vi.mocked(getRecordCheckAnswers).mockResolvedValue({
        data: undefined,
        error: {},
        response: { status },
      } as never)

      await renderPage()

      expect(
        screen.getByRole('heading', { level: 1, name: "This record can't be submitted" }),
      ).toBeInTheDocument()
      expect(screen.getByRole('link', { name: 'Back' })).toHaveAttribute(
        'href',
        '/portal/records/42/task-list',
      )
      expect(screen.queryByText('Submit and return actions')).not.toBeInTheDocument()
    },
  )

  it('shows an error when the answers cannot be loaded', async () => {
    vi.mocked(getRecordCheckAnswers).mockResolvedValue({
      data: undefined,
      response: { status: 500 },
    } as never)

    await renderPage()

    expect(
      screen.getByRole('heading', { level: 1, name: 'Unable to load check your answers' }),
    ).toBeInTheDocument()
    expect(
      screen.getByText('There was a problem retrieving your answers. Please try again later.'),
    ).toBeInTheDocument()
  })
})
