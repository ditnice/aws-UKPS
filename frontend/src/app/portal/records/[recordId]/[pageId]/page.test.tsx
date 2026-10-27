import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { getRecordFormPage } from '@/client/generated'

import RecordFormRoute from './page'

import type { ReactElement } from 'react'

const mocks = vi.hoisted(() => ({
  notFound: vi.fn(() => {
    throw new Error('NEXT_NOT_FOUND')
  }),
}))

vi.mock('@/client/generated', () => ({ getRecordFormPage: vi.fn() }))
vi.mock('@/client/server-api', () => ({ createServerApiClient: vi.fn() }))
vi.mock('next/navigation', () => ({ notFound: mocks.notFound }))
vi.mock('@/components/DynamicForm/FormPage', () => ({
  FormPage: ({ page }: { page: { page: { id: string } } }) => (
    <div data-testid="form-page">{page.page.id}</div>
  ),
}))

const params = (recordId = '5', pageId = 'indication') => Promise.resolve({ recordId, pageId })

const renderRoute = async (routeParams = params()) =>
  render((await RecordFormRoute({ params: routeParams })) as ReactElement)

afterEach(() => {
  cleanup()
  vi.clearAllMocks()
})

describe('RecordFormRoute', () => {
  it.each([
    ['0', 'indication'],
    ['x', 'indication'],
    ['5', 'Not A Page'],
  ])('is not found for invalid parameters (%s, %s)', async (recordId, pageId) => {
    await expect(renderRoute(params(recordId, pageId))).rejects.toThrow('NEXT_NOT_FOUND')
    expect(getRecordFormPage).not.toHaveBeenCalled()
  })

  it('is not found when the API says so', async () => {
    vi.mocked(getRecordFormPage).mockResolvedValue({
      data: undefined,
      response: { status: 404 },
    } as never)

    await expect(renderRoute()).rejects.toThrow('NEXT_NOT_FOUND')
  })

  it('shows an error when the page cannot be loaded', async () => {
    vi.mocked(getRecordFormPage).mockResolvedValue({
      data: undefined,
      response: { status: 500 },
    } as never)

    await renderRoute()

    expect(
      screen.getByText('There was a problem retrieving this page. Please try again later.'),
    ).toBeInTheDocument()
  })

  it('renders the form page', async () => {
    vi.mocked(getRecordFormPage).mockResolvedValue({
      data: { page: { id: 'indication' }, revisionVersion: 1 },
      response: { status: 200 },
    } as never)

    await renderRoute()

    expect(screen.getByTestId('form-page')).toHaveTextContent('indication')
    expect(getRecordFormPage).toHaveBeenCalledWith(
      expect.objectContaining({ path: { recordId: 5, pageId: 'indication' } }),
    )
  })
})
