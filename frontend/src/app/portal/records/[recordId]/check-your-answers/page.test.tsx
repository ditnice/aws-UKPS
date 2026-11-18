import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { getRecordCheckAnswers } from '@/client/generated'
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
        tasks: [
          {
            id: 'names-and-identifiers',
            title: 'Names and identifiers',
            questions: [
              {
                id: 'company-code',
                pageId: 'names',
                label: 'Company code',
                type: 'Text',
                multiple: false,
                values: ['GC22'],
                selectedOptions: null,
              },
              {
                id: 'generic-name',
                pageId: 'names',
                label: 'Generic name',
                type: 'Text',
                multiple: false,
                values: [],
                selectedOptions: null,
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
    })
  })

  it('still offers the actions when there are errors, so Submit can report them', async () => {
    vi.mocked(getRecordCheckAnswers).mockResolvedValue({
      ...checkAnswers,
      data: { ...checkAnswers.data, errors: [genericNameError] },
    })

    await renderPage()

    expect(screen.getByText('Submit and return actions')).toBeInTheDocument()
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
