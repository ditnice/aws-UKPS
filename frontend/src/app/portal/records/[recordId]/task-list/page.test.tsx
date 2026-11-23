import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { getRecordTaskList } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'

import RecordTaskListPage from './page'

const mocks = vi.hoisted(() => ({
  notFound: vi.fn(() => {
    throw new Error('NEXT_NOT_FOUND')
  }),
}))

vi.mock('@/client/generated', () => ({
  getRecordTaskList: vi.fn(),
}))

vi.mock('@/client/server-api', () => ({
  createServerApiClient: vi.fn(),
}))

vi.mock('next/navigation', () => ({
  notFound: mocks.notFound,
}))

type GetRecordTaskListResult = Awaited<ReturnType<typeof getRecordTaskList>>

// A trimmed version of the Medicine form: one task from each of its two sections, to show that
// every section and task is rendered.
const taskList = {
  data: {
    recordId: 42,
    organisationId: 7,
    displayStatus: 'Draft',
    companyCode: 'GC123',
    recordTitle: 'COVID-19, 18+ who do not require supplemental oxygen',
    sections: [
      {
        id: 'indication-and-development',
        title: 'Provide indication and development information',
        tasks: [
          {
            id: 'indication-details',
            title: 'Indication details',
            status: 'InProgress',
            firstPageId: 'indication',
          },
        ],
      },
      {
        id: 'service-readiness',
        title: 'Provide service readiness information (optional)',
        tasks: [
          {
            id: 'laboratory-testing',
            title: 'Laboratory testing',
            status: 'NotStarted',
            firstPageId: 'diagnostic-test-required',
          },
        ],
      },
    ],
  },
  error: undefined,
  response: new Response(null, { status: 200 }),
} satisfies GetRecordTaskListResult

// The same list with the mandatory task completed. The optional service readiness task is still
// not started, to show that it does not hold back submission.
const mandatoryTasksCompleteTaskList = {
  ...taskList,
  data: {
    ...taskList.data,
    sections: [
      {
        ...taskList.data.sections[0],
        tasks: [{ ...taskList.data.sections[0].tasks[0], status: 'Completed' }],
      },
      taskList.data.sections[1],
    ],
  },
} satisfies GetRecordTaskListResult

const renderPage = async (recordId = '42', action?: string) => {
  const page = await RecordTaskListPage({
    params: Promise.resolve({ recordId }),
    searchParams: Promise.resolve({ action }),
  })
  render(page)
}

beforeEach(() => {
  vi.clearAllMocks()
  vi.mocked(createServerApiClient).mockResolvedValue(
    {} as Awaited<ReturnType<typeof createServerApiClient>>,
  )
  vi.mocked(getRecordTaskList).mockResolvedValue(taskList)
})

afterEach(cleanup)

describe('RecordTaskListPage', () => {
  it('renders the task list and links each task to its first page', async () => {
    await renderPage()

    expect(getRecordTaskList).toHaveBeenCalledWith(
      expect.objectContaining({ path: { recordId: 42 } }),
    )
    expect(screen.getByRole('heading', { level: 1, name: 'Record 42: GC123' })).toBeInTheDocument()
    expect(
      screen.getByText('COVID-19, 18+ who do not require supplemental oxygen'),
    ).toBeInTheDocument()
    expect(screen.getByText('Draft')).toBeInTheDocument()
    expect(screen.getByText('You have completed 0 of 1 mandatory tasks')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Skip to first incomplete task' })).toHaveAttribute(
      'href',
      '/portal/records/42/indication',
    )
    expect(
      screen.getByRole('heading', {
        level: 2,
        name: 'Provide indication and development information',
      }),
    ).toBeInTheDocument()
    expect(
      screen.getByRole('heading', {
        level: 2,
        name: 'Provide service readiness information (optional)',
      }),
    ).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Back' })).toHaveAttribute(
      'href',
      '/portal/organisations/7/records/42',
    )
    expect(screen.getByRole('link', { name: 'Indication details' })).toHaveAttribute(
      'href',
      '/portal/records/42/indication',
    )
    expect(screen.getByRole('link', { name: 'Laboratory testing' })).toHaveAttribute(
      'href',
      '/portal/records/42/diagnostic-test-required',
    )
    expect(screen.getByText('In progress')).toBeInTheDocument()
    expect(screen.getByText('Not yet started')).toBeInTheDocument()
  })

  it('does not offer review and submit while a mandatory task is incomplete', async () => {
    await renderPage()

    expect(screen.queryByRole('link', { name: 'Review and submit record' })).not.toBeInTheDocument()
  })

  it('offers review and submit once every mandatory task is complete', async () => {
    vi.mocked(getRecordTaskList).mockResolvedValue(mandatoryTasksCompleteTaskList)

    await renderPage()

    expect(screen.getByRole('link', { name: 'Review and submit record' })).toHaveAttribute(
      'href',
      '/portal/records/42/check-your-answers',
    )
  })

  describe('PIM and EAMS when they cannot be done', () => {
    const withPimStatus = (status: 'NotApplicable' | 'CannotStart') =>
      ({
        ...mandatoryTasksCompleteTaskList,
        data: {
          ...mandatoryTasksCompleteTaskList.data,
          sections: [
            ...mandatoryTasksCompleteTaskList.data.sections,
            {
              id: 'special-designation',
              title: 'Provide special designation information',
              tasks: [
                {
                  id: 'pim-and-eams',
                  title:
                    'Promising Innovative Medicine (PIM) and Early Access to Medicines Scheme (EAMS)',
                  status,
                  firstPageId: 'pim-designation',
                },
              ],
            },
          ],
        },
      }) satisfies GetRecordTaskListResult

    it('shows a not applicable task as plain text with no link, and still lets the user submit', async () => {
      vi.mocked(getRecordTaskList).mockResolvedValue(withPimStatus('NotApplicable'))

      await renderPage()

      expect(screen.getByText('Cannot start yet')).toBeInTheDocument()
      expect(screen.getByText('You have completed 1 of 1 mandatory tasks')).toBeInTheDocument()
      expect(screen.getByText(/Promising Innovative Medicine/)).toBeInTheDocument()
      expect(screen.queryByRole('link', { name: /Promising Innovative Medicine/ })).toBeNull()
      expect(screen.getByRole('link', { name: 'Review and submit record' })).toBeInTheDocument()
    })

    it('shows a task that cannot start yet as plain text with no link, and holds back submission', async () => {
      vi.mocked(getRecordTaskList).mockResolvedValue(withPimStatus('CannotStart'))

      await renderPage()

      expect(screen.getByText('Cannot start yet')).toBeInTheDocument()
      expect(screen.queryByRole('link', { name: /Promising Innovative Medicine/ })).toBeNull()
      expect(
        screen.queryByRole('link', { name: 'Review and submit record' }),
      ).not.toBeInTheDocument()
    })
  })

  it('links back to the organisation records list', async () => {
    await renderPage()

    expect(screen.getByRole('link', { name: 'Return to my records' })).toHaveAttribute(
      'href',
      '/portal/organisations/7/records',
    )
  })

  it('shows the draft saved alert when the draft-saved action is set', async () => {
    await renderPage('42', 'draft-saved')

    expect(
      screen.getByRole('heading', { level: 3, name: 'Draft record saved' }),
    ).toBeInTheDocument()
  })

  it('does not show the draft saved alert without the draft-saved action', async () => {
    await renderPage()

    expect(
      screen.queryByRole('heading', { level: 3, name: 'Draft record saved' }),
    ).not.toBeInTheDocument()
  })

  it('does not show the alert for other actions', async () => {
    await renderPage('42', 'discarded')

    expect(
      screen.queryByRole('heading', { level: 3, name: 'Record successfully discarded' }),
    ).not.toBeInTheDocument()
  })

  it('returns not found for an invalid record ID', async () => {
    await expect(renderPage('abc')).rejects.toThrow('NEXT_NOT_FOUND')
    expect(getRecordTaskList).not.toHaveBeenCalled()
  })

  it('returns not found when the API says so', async () => {
    vi.mocked(getRecordTaskList).mockResolvedValue({
      data: undefined,
      response: { status: 404 },
    } as never)

    await expect(renderPage()).rejects.toThrow('NEXT_NOT_FOUND')
  })

  it('shows an error when the task list cannot be loaded', async () => {
    vi.mocked(getRecordTaskList).mockResolvedValue({
      data: undefined,
      response: { status: 500 },
    } as never)

    await renderPage()

    expect(
      screen.getByRole('heading', { level: 1, name: 'Unable to load task list' }),
    ).toBeInTheDocument()
    expect(
      screen.getByText('There was a problem retrieving this task list. Please try again later.'),
    ).toBeInTheDocument()
  })
})
