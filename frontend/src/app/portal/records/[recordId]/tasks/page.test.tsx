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
            status: 'in-progress',
            firstPageId: 'indication',
          },
          {
            id: 'bnf-chapter',
            title: 'BNF chapter',
            status: 'not-started',
            firstPageId: 'bnf-chapter',
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
    expect(screen.getByRole('link', { name: 'Back' })).toHaveAttribute(
      'href',
      '/portal/organisations/7/records/42',
    )
    expect(screen.getByRole('link', { name: 'Indication details' })).toHaveAttribute(
      'href',
      '/portal/records/42/indication',
    )
    expect(screen.getByRole('link', { name: 'BNF chapter' })).toHaveAttribute(
      'href',
      '/portal/records/42/bnf-chapter',
    )
  })

  it('returns not found for an invalid record ID', async () => {
    await expect(renderPage('abc')).rejects.toThrow('NEXT_NOT_FOUND')
  })
})
