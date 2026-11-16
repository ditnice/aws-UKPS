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

vi.mock('@/client/server-api', () => ({ createServerApiClient: vi.fn() }))
vi.mock('next/navigation', () => ({ notFound: mocks.notFound }))
vi.mock('@/client/generated', async (importOriginal) => ({
  ...(await importOriginal<typeof import('@/client/generated')>()),
  getRecordFormPage: vi.fn(),
}))
vi.mock('@/components/DynamicForm/FormPage', async (importOriginal) => ({
  ...(await importOriginal<typeof import('@/components/DynamicForm/FormPage')>()),
  FormPage: ({
    page,
    returnToCheckAnswers,
  }: {
    page: { page: { id: string } }
    returnToCheckAnswers?: boolean
  }) => (
    <div data-return-to-check-answers={String(returnToCheckAnswers)} data-testid="form-page">
      {page.page.id}
    </div>
  ),
}))

const params = (recordId = '5', pageId = 'indication') => Promise.resolve({ recordId, pageId })

const searchParams = (returnTo?: string | string[]) => Promise.resolve({ returnTo })

const renderRoute = async (routeParams = params(), routeSearchParams = searchParams()) =>
  render(
    (await RecordFormRoute({
      params: routeParams,
      searchParams: routeSearchParams,
    })) as ReactElement,
  )

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

  it('tells the form page the user came from check your answers', async () => {
    vi.mocked(getRecordFormPage).mockResolvedValue({
      data: { page: { id: 'indication' }, revisionVersion: 1 },
      response: { status: 200 },
    } as never)

    await renderRoute(params(), searchParams('check-your-answers'))

    expect(screen.getByTestId('form-page')).toHaveAttribute('data-return-to-check-answers', 'true')
  })

  it.each([[undefined], ['elsewhere'], [['check-your-answers', 'elsewhere']]])(
    'does not return to check your answers for returnTo=%s',
    async (returnTo) => {
      vi.mocked(getRecordFormPage).mockResolvedValue({
        data: { page: { id: 'indication' }, revisionVersion: 1 },
        response: { status: 200 },
      } as never)

      await renderRoute(params(), searchParams(returnTo))

      expect(screen.getByTestId('form-page')).toHaveAttribute(
        'data-return-to-check-answers',
        'false',
      )
    },
  )
})
