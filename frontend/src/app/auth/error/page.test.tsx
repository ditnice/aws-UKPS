import { render, screen, within } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { getCurrentOrganisationChampions } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { redirect } from '@/test-utils/nextNavigation'

import MembershipDeactivated, { dynamic } from './page'

vi.mock('@/client/generated', () => ({
  AuthenticationFailCode: {
    MEMBERSHIP_DEACTIVATED: 'MembershipDeactivated',
    ORGANISATION_NOT_ACTIVE: 'OrganisationNotActive',
  },
  getCurrentOrganisationChampions: vi.fn(),
}))

vi.mock('@/client/server-api', () => ({
  createServerApiClient: vi.fn(),
}))

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

type ChampionResult = Awaited<ReturnType<typeof getCurrentOrganisationChampions<false>>>

const mockChampions = (result: {
  data?: ChampionResult['data']
  error?: ChampionResult['error']
  response?: Response
}) => vi.mocked(getCurrentOrganisationChampions).mockResolvedValue(result as ChampionResult)

const renderPage = async (code = 'MembershipDeactivated') =>
  render(await MembershipDeactivated({ searchParams: Promise.resolve({ code }) }))

beforeEach(() => {
  vi.mocked(createServerApiClient).mockResolvedValue(
    {} as Awaited<ReturnType<typeof createServerApiClient>>,
  )
})

// Direct invocation checks page orchestration and its returned synchronous tree.
// The actual recovery-request redirect exception is covered in server-api.test.ts.
describe('MembershipDeactivated (direct invocation)', () => {
  it('renders dynamically so champion contacts are not cached as a static page', () => {
    expect(dynamic).toBe('force-dynamic')
  })

  it('shows a retrieval error when champion data is unavailable', async () => {
    mockChampions({ data: undefined, error: undefined })

    await renderPage()

    expect(
      screen.getByRole('heading', { name: 'Your account has been deactivated' }),
    ).toBeInTheDocument()
    expect(screen.getByTestId('champion-retrieval-error')).toHaveTextContent(
      "An error occurred when retrieving your organisation's champion users. Please try again later.",
    )
    expect(screen.getByRole('alert')).toBeInTheDocument()
  })

  it('shows a retrieval error when no champion contacts are returned', async () => {
    mockChampions({ data: [], error: undefined })

    await renderPage()

    expect(screen.getByTestId('champion-retrieval-error')).toBeInTheDocument()
    expect(screen.getByRole('alert')).toBeInTheDocument()
    expect(screen.queryByTestId('multiple-champions')).not.toBeInTheDocument()
    expect(screen.queryByRole('list')).not.toBeInTheDocument()
  })

  it('prioritises a retrieval error over an empty champion list', async () => {
    mockChampions({ data: [], error: { title: 'Forbidden' } })

    await renderPage()

    expect(screen.getByTestId('champion-retrieval-error')).toBeInTheDocument()
  })

  it('shows a single champion email as a mailto link', async () => {
    mockChampions({ data: ['champion@example.com'], error: undefined })

    await renderPage()

    const contact = screen.getByTestId('single-champion')
    expect(contact).toHaveTextContent(
      "Contact your organisation's champion user to reactivate your account:",
    )
    expect(within(contact).getByRole('link', { name: 'champion@example.com' })).toHaveAttribute(
      'href',
      'mailto:champion@example.com',
    )
    expect(createServerApiClient).toHaveBeenCalledOnce()
    const client = await vi.mocked(createServerApiClient).mock.results[0].value
    expect(getCurrentOrganisationChampions).toHaveBeenCalledOnce()
    expect(getCurrentOrganisationChampions).toHaveBeenCalledWith({ client })
    expect(screen.queryByRole('list')).not.toBeInTheDocument()
  })

  it('lists several champion emails with one mailto link per list item', async () => {
    const emails = ['first.champion@example.com', 'second.champion@example.com']
    mockChampions({ data: emails, error: undefined })

    await renderPage()

    const contacts = screen.getByTestId('multiple-champions')
    expect(contacts).toHaveTextContent(
      "Contact one of your organisation's champion users to reactivate your account. Their email addresses are:",
    )
    const items = within(contacts).getAllByRole('listitem')
    expect(items).toHaveLength(emails.length)
    emails.forEach((email, index) => {
      expect(within(items[index]).getByRole('link', { name: email })).toHaveAttribute(
        'href',
        `mailto:${email}`,
      )
    })
  })

  it('renders the retrieval error without redirecting when the endpoint returns 401', async () => {
    mockChampions({
      data: undefined,
      error: { code: 'MembershipDeactivated' },
      response: new Response(null, { status: 401 }),
    })

    await renderPage()

    expect(screen.getByTestId('champion-retrieval-error')).toBeInTheDocument()
    expect(redirect).not.toHaveBeenCalled()
  })

  it('does not request champion contacts for an inactive organisation', async () => {
    await renderPage('OrganisationNotActive')

    expect(
      screen.getByRole('heading', { name: 'Your organisation is not active' }),
    ).toBeInTheDocument()
    expect(
      screen.getByText("Please contact your organisation's champion user for more information."),
    ).toBeInTheDocument()
    expect(getCurrentOrganisationChampions).not.toHaveBeenCalled()
    expect(createServerApiClient).not.toHaveBeenCalled()
  })

  it('does not request champion contacts for an unknown authentication code', async () => {
    await renderPage('UnknownCode')

    expect(
      screen.getByRole('heading', { name: 'An unexpected authentication error has occurred.' }),
    ).toBeInTheDocument()
    expect(getCurrentOrganisationChampions).not.toHaveBeenCalled()
    expect(createServerApiClient).not.toHaveBeenCalled()
  })
})
