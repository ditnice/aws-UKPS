import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { getCurrentUserOrganisations } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { redirect } from '@/test-utils/nextNavigation'

import SelectOrganisationPage from './page'

const mocks = { redirect }

vi.mock('@/client/generated', () => ({
  getCurrentUserOrganisations: vi.fn(),
}))

vi.mock('@/client/server-api', () => ({
  createServerApiClient: vi.fn(),
}))

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

// Isolate organisation-list prop wiring; selection behaviour uses the real table in its tests.
vi.mock('./_components/SelectOrganisationTable', () => ({
  SelectOrganisationTable: ({
    organisations,
    returnTo,
  }: {
    organisations: { id: number }[]
    returnTo?: string
  }) => (
    <div data-testid="select-organisation-table" data-return-to={returnTo}>
      {organisations.length}
    </div>
  ),
}))

type GetCurrentUserOrganisationsResult = Awaited<ReturnType<typeof getCurrentUserOrganisations>>

const mockOrganisations = (result: Partial<GetCurrentUserOrganisationsResult>) =>
  vi
    .mocked(getCurrentUserOrganisations)
    .mockResolvedValue(result as GetCurrentUserOrganisationsResult)

const renderPage = (searchParams: { returnTo?: string } = {}) =>
  SelectOrganisationPage({ searchParams: Promise.resolve(searchParams) })

beforeEach(() => {
  vi.mocked(createServerApiClient).mockResolvedValue(
    {} as Awaited<ReturnType<typeof createServerApiClient>>,
  )
})
// Direct invocation checks orchestration and the returned synchronous tree only.
// It does not exercise Next.js rendering, redirects or browser-history behaviour.
describe('SelectOrganisationPage (direct invocation)', () => {
  it('renders the organisation table when the user has multiple organisations', async () => {
    mockOrganisations({
      data: [
        { id: 1, organisationName: 'First organisation' },
        { id: 2, organisationName: 'Second organisation' },
      ],
      error: undefined,
    })

    render(await renderPage())

    expect(screen.getByTestId('select-organisation-table').textContent).toBe('2')
    expect(mocks.redirect).not.toHaveBeenCalled()
  })

  it('requests a replacement redirect to records when the user has a single organisation', async () => {
    mockOrganisations({
      data: [{ id: 7, organisationName: 'Only organisation' }],
      error: undefined,
    })

    await expect(renderPage()).rejects.toThrow('NEXT_REDIRECT')

    expect(mocks.redirect).toHaveBeenCalledWith('/portal/organisations/7/records', 'replace')
  })

  it('passes a safe returnTo to the organisation table', async () => {
    mockOrganisations({
      data: [
        { id: 1, organisationName: 'First organisation' },
        { id: 2, organisationName: 'Second organisation' },
      ],
      error: undefined,
    })

    render(await renderPage({ returnTo: '/portal/organisations/2/records' }))

    expect(screen.getByTestId('select-organisation-table')).toHaveAttribute(
      'data-return-to',
      '/portal/organisations/2/records',
    )
  })

  it('ignores an unsafe returnTo', async () => {
    mockOrganisations({
      data: [
        { id: 1, organisationName: 'First organisation' },
        { id: 2, organisationName: 'Second organisation' },
      ],
      error: undefined,
    })

    render(await renderPage({ returnTo: 'https://example.com/portal' }))

    expect(screen.getByTestId('select-organisation-table')).not.toHaveAttribute('data-return-to')
  })

  it('requests a replacement redirect to returnTo when the user has a single organisation', async () => {
    mockOrganisations({
      data: [{ id: 7, organisationName: 'Only organisation' }],
      error: undefined,
    })

    await expect(renderPage({ returnTo: '/portal/organisations/7/records/3' })).rejects.toThrow(
      'NEXT_REDIRECT',
    )

    expect(mocks.redirect).toHaveBeenCalledWith('/portal/organisations/7/records/3', 'replace')
  })

  it('shows a message when the user has no organisations', async () => {
    mockOrganisations({ data: [], error: undefined })

    render(await renderPage())

    expect(
      screen.getByText('You do not currently have access to manage any organisations.'),
    ).toBeInTheDocument()
    expect(screen.queryByTestId('select-organisation-table')).toBeNull()
  })

  it('shows an error when the organisations cannot be retrieved', async () => {
    mockOrganisations({ data: undefined, error: { title: 'error' } })

    render(await renderPage())

    expect(screen.getByRole('alert')).toHaveTextContent(
      'An error occurred when retrieving your organisations. Please try again later.',
    )
    expect(screen.queryByTestId('select-organisation-table')).toBeNull()
  })
})
