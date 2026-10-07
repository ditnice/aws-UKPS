import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { getCurrentUserOrganisations } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'

import SelectOrganisationPage from './page'

const mocks = vi.hoisted(() => ({
  redirect: vi.fn(() => {
    throw new Error('NEXT_REDIRECT')
  }),
}))

vi.mock('@/client/generated', () => ({
  getCurrentUserOrganisations: vi.fn(),
}))

vi.mock('@/client/server-api', () => ({
  createServerApiClient: vi.fn(),
}))

vi.mock('next/navigation', () => ({
  redirect: mocks.redirect,
  RedirectType: { push: 'push', replace: 'replace' },
}))

vi.mock('@nice-digital/nds-page-header', () => ({
  PageHeader: ({ heading }: { heading: string }) => <h1>{heading}</h1>,
}))

vi.mock('./_components/SelectOrganisationTable', () => ({
  SelectOrganisationTable: ({ organisations }: { organisations: { id: number }[] }) => (
    <div data-testid="select-organisation-table">{organisations.length}</div>
  ),
}))

type GetCurrentUserOrganisationsResult = Awaited<ReturnType<typeof getCurrentUserOrganisations>>

const mockOrganisations = (result: Partial<GetCurrentUserOrganisationsResult>) =>
  vi
    .mocked(getCurrentUserOrganisations)
    .mockResolvedValue(result as GetCurrentUserOrganisationsResult)

beforeEach(() => {
  vi.mocked(createServerApiClient).mockResolvedValue(
    {} as Awaited<ReturnType<typeof createServerApiClient>>,
  )
})
describe('SelectOrganisationPage', () => {
  it('renders the organisation table when the user has multiple organisations', async () => {
    mockOrganisations({
      data: [
        { id: 1, organisationName: 'First organisation' },
        { id: 2, organisationName: 'Second organisation' },
      ],
      error: undefined,
    })

    render(await SelectOrganisationPage())

    expect(screen.getByTestId('select-organisation-table').textContent).toBe('2')
    expect(mocks.redirect).not.toHaveBeenCalled()
  })

  it('replaces the history entry with the records page when the user has a single organisation', async () => {
    mockOrganisations({
      data: [{ id: 7, organisationName: 'Only organisation' }],
      error: undefined,
    })

    await expect(SelectOrganisationPage()).rejects.toThrow('NEXT_REDIRECT')

    expect(mocks.redirect).toHaveBeenCalledWith('/portal/organisations/7/records', 'replace')
  })

  it('shows a message when the user has no organisations', async () => {
    mockOrganisations({ data: [], error: undefined })

    render(await SelectOrganisationPage())

    expect(screen.getByTestId('no-organisations')).toBeTruthy()
    expect(screen.queryByTestId('select-organisation-table')).toBeNull()
  })

  it('shows an error when the organisations cannot be retrieved', async () => {
    mockOrganisations({ data: undefined, error: { title: 'error' } })

    render(await SelectOrganisationPage())

    expect(screen.getByTestId('organisation-retrieval-error')).toBeTruthy()
    expect(screen.queryByTestId('select-organisation-table')).toBeNull()
  })
})
