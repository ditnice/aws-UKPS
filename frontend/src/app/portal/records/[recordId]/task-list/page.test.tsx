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

// The Medicine form has one task today. The second section is illustrative, to show that every
// section and task is rendered.
const taskList = {
  data: {
    recordId: 42,
    organisationId: 7,
    sections: [
      {
        id: 'indication-details',
        title: 'Indication details',
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
        id: 'treatment-details',
        title: 'Treatment details',
        tasks: [
          {
            id: 'formulation',
            title: 'Formulation',
            status: 'NotStarted',
            firstPageId: 'formulation-type',
          },
        ],
      },
    ],
  },
  error: undefined,
  response: new Response(null, { status: 200 }),
} satisfies GetRecordTaskListResult

const renderPage = async (recordId = '42') => {
  const page = await RecordTaskListPage({ params: Promise.resolve({ recordId }) })
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
    expect(screen.getByRole('heading', { level: 1, name: 'Task list' })).toBeInTheDocument()
    expect(
      screen.getByRole('heading', { level: 2, name: 'Indication details' }),
    ).toBeInTheDocument()
    expect(screen.getByRole('heading', { level: 2, name: 'Treatment details' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Back' })).toHaveAttribute(
      'href',
      '/portal/organisations/7/records/42',
    )
    expect(screen.getByRole('link', { name: 'Indication details' })).toHaveAttribute(
      'href',
      '/portal/records/42/indication',
    )
    expect(screen.getByRole('link', { name: 'Formulation' })).toHaveAttribute(
      'href',
      '/portal/records/42/formulation-type',
    )
    expect(screen.getByText('In progress')).toBeInTheDocument()
    expect(screen.getByText('Not yet started')).toBeInTheDocument()
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
