import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { getUsersMe } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { errorMessages } from '@/lib/form/errorMessages'

import { findTypedPageProps } from './navigation'
import Page from './page'

// ---- Mocks -----------------------------------------------------------------

vi.mock('next/navigation', () => ({
  notFound: vi.fn(() => {
    // Mirrors Next.js behaviour: notFound() throws to halt rendering
    throw new Error('NEXT_NOT_FOUND')
  }),
}))

vi.mock('@/client/generated', () => ({
  getUsersMe: vi.fn(),
}))

vi.mock('@/client/server-api', () => ({
  createServerApiClient: vi.fn(),
}))

vi.mock('./navigation', () => ({
  findTypedPageProps: vi.fn(),
}))

vi.mock('@/components/BackLink/BackLink', () => ({
  BackLink: ({ href }: { href: string }) => (
    <a data-testid="back-link" href={href}>
      Back
    </a>
  ),
}))

vi.mock('@/components/PageHeader/PageHeader', () => ({
  PageHeader: ({ heading, backLink }: { heading: string; backLink: React.ReactNode }) => (
    <header>
      {backLink}
      <h1>{heading}</h1>
    </header>
  ),
}))

vi.mock('@/components/Placeholder/ErrorState', () => ({
  ErrorState: ({ children }: React.PropsWithChildren) => (
    <div data-testid="error-state">{children}</div>
  ),
}))

vi.mock('./editUserDetailValue/EditUserDetailForm', () => ({
  default: (props: Record<string, unknown>) => (
    <div data-testid="edit-user-detail-form">{JSON.stringify(props)}</div>
  ),
}))

vi.mock('./editUserEmail/EditUserEmailForm', () => ({
  default: (props: Record<string, unknown>) => (
    <div data-testid="edit-user-email-form">{JSON.stringify(props)}</div>
  ),
}))

// ---- Helpers ---------------------------------------------------------------

const mockClient = { id: 'mock-client' }

const mockUser = {
  userId: 'user-123',
  fullName: 'Jane Doe',
  workEmail: 'jane.doe@example.com',
  workTelephone: '07123 456789',
}

const setDetail = (detail: string | undefined) => {
  vi.mocked(findTypedPageProps).mockResolvedValue({
    params: { detail },
  } as Awaited<ReturnType<typeof findTypedPageProps>>)
}

const mockGetUsersMe = (result: { data?: unknown; error?: unknown }) => {
  vi.mocked(getUsersMe).mockResolvedValue(result as Awaited<ReturnType<typeof getUsersMe>>)
}

/**
 * Async server components can't be rendered directly, so we await the
 * component function and render the React element it resolves to.
 */
const renderPage = async () => {
  const ui = await Page({} as Parameters<typeof Page>[0])
  return render(ui)
}

const getFormProps = (testId: string) => JSON.parse(screen.getByTestId(testId).textContent ?? '{}')

// ---- Tests -----------------------------------------------------------------

describe('Edit user detail page', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(createServerApiClient).mockResolvedValue(
      mockClient as unknown as Awaited<ReturnType<typeof createServerApiClient>>,
    )
    setDetail('fullName')
    mockGetUsersMe({ data: mockUser })
  })

  describe('when no detail param is provided', () => {
    it('calls notFound and does not fetch the current user', async () => {
      const { notFound } = await import('next/navigation')
      setDetail(undefined)

      await expect(renderPage()).rejects.toThrow('NEXT_NOT_FOUND')

      expect(notFound).toHaveBeenCalledTimes(1)
      expect(createServerApiClient).not.toHaveBeenCalled()
      expect(getUsersMe).not.toHaveBeenCalled()
    })
  })

  describe('data fetching', () => {
    it('creates a server api client and passes it to getUsersMe', async () => {
      await renderPage()

      expect(createServerApiClient).toHaveBeenCalledTimes(1)
      expect(getUsersMe).toHaveBeenCalledWith({ client: mockClient })
    })
  })

  describe('error handling', () => {
    it('renders the error state when the request returns an error', async () => {
      mockGetUsersMe({ data: mockUser, error: { message: 'boom' } })

      await renderPage()

      expect(screen.getByTestId('error-state')).toHaveTextContent(
        errorMessages.failedToRetrieveCurrentUser,
      )
      expect(screen.queryByTestId('edit-user-detail-form')).not.toBeInTheDocument()
      expect(screen.queryByTestId('edit-user-email-form')).not.toBeInTheDocument()
    })

    it('renders the error state when no user data is returned', async () => {
      mockGetUsersMe({ data: undefined })

      await renderPage()

      expect(screen.getByTestId('error-state')).toHaveTextContent(
        errorMessages.failedToRetrieveCurrentUser,
      )
    })

    it('still renders the page header with the correct label on error', async () => {
      setDetail('email')
      mockGetUsersMe({ data: undefined, error: { message: 'boom' } })

      await renderPage()

      expect(screen.getByRole('heading', { name: 'Update Email address' })).toBeInTheDocument()
      expect(screen.getByTestId('back-link')).toHaveAttribute('href', '/portal/user/me')
    })
  })

  describe('page header', () => {
    it.each([
      ['fullName', 'Update Full name'],
      ['contactNumber', 'Update Contact number'],
      ['email', 'Update Email address'],
    ])('renders the heading for %s', async (detail, heading) => {
      setDetail(detail)

      await renderPage()

      expect(screen.getByRole('heading', { name: heading })).toBeInTheDocument()
    })

    it('renders a back link to the user profile page', async () => {
      await renderPage()

      expect(screen.getByTestId('back-link')).toHaveAttribute('href', '/portal/user/me')
    })
  })

  describe('form selection', () => {
    it('renders EditUserDetailForm for the full name', async () => {
      setDetail('fullName')

      await renderPage()

      expect(getFormProps('edit-user-detail-form')).toEqual({
        label: 'Full name',
        userId: mockUser.userId,
        property: 'FullName',
        initialValue: mockUser.fullName,
      })
      expect(screen.queryByTestId('edit-user-email-form')).not.toBeInTheDocument()
      expect(screen.queryByTestId('error-state')).not.toBeInTheDocument()
    })

    it('renders EditUserDetailForm for the contact number', async () => {
      setDetail('contactNumber')

      await renderPage()

      expect(getFormProps('edit-user-detail-form')).toEqual({
        label: 'Contact number',
        userId: mockUser.userId,
        property: 'WorkPhone',
        initialValue: mockUser.workTelephone,
      })
      expect(screen.queryByTestId('edit-user-email-form')).not.toBeInTheDocument()
      expect(screen.queryByTestId('error-state')).not.toBeInTheDocument()
    })

    it('renders EditUserEmailForm for the email address', async () => {
      setDetail('email')

      await renderPage()

      expect(getFormProps('edit-user-email-form')).toEqual({
        userId: mockUser.userId,
        initialValue: mockUser.workEmail,
      })
      expect(screen.queryByTestId('edit-user-detail-form')).not.toBeInTheDocument()
      expect(screen.queryByTestId('error-state')).not.toBeInTheDocument()
    })
  })
})
