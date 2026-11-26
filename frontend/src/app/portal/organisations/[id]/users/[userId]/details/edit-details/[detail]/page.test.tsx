import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import {
  fakePendingWorkEmailChangeDto,
  fakeUserInformationDto,
} from '@/client/generated/@faker-js/faker.gen'
import { errorMessages } from '@/lib/form/errorMessages'

import { getUser } from '../../dataRetrieval'

import { findTypedPageProps } from './navigation'
import Page from './page'
import PendingEmailUpdateAlert from './PendingEmailUpdateAlert'

// ---- Mocks -----------------------------------------------------------------

vi.mock('next/navigation', () => ({
  notFound: vi.fn(() => {
    // Mirrors Next.js behaviour: notFound() throws to halt rendering
    throw new Error('NEXT_NOT_FOUND')
  }),
}))

vi.mock('../../dataRetrieval', () => ({
  getUser: vi.fn(),
}))

vi.mock('./navigation', () => ({
  findTypedPageProps: vi.fn(),
}))

vi.mock('./PendingEmailUpdateAlert', () => ({
  default: vi.fn(() => null),
}))

vi.mock('./editUserEmail/CancelPendingEmailChange', () => ({
  default: () => <div data-testid="cancel-pending-email-change" />,
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

const PendingEmailUpdateAlertMock = vi.mocked(PendingEmailUpdateAlert)

// ---- Helpers ---------------------------------------------------------------

const mockUser = {
  userId: 'user-123',
  fullName: 'Jane Doe',
  workEmail: 'jane.doe@example.com',
  workTelephone: '07123 456789',
}

const returnLink = '/portal/organisations/7/users/42/details'
const onSuccessFlag = 'updated'
const onSuccessRedirect = `${returnLink}?${onSuccessFlag}=true`

const setDetail = (detail: string | undefined) => {
  vi.mocked(findTypedPageProps).mockResolvedValue({
    organisationId: 7,
    userId: 42,
    detail,
    returnLink,
    onSuccessFlag,
    owner: 'not-me',
  } as unknown as Awaited<ReturnType<typeof findTypedPageProps>>)
}

const mockGetUser = (result: { user?: unknown; error?: unknown }) => {
  vi.mocked(getUser).mockResolvedValue(result as Awaited<ReturnType<typeof getUser>>)
}

/**
 * Async server components can't be rendered directly, so we await the
 * component function and render the React element it resolves to.
 */
const renderPage = async () => {
  const ui = await Page({
    params: Promise.resolve({ id: '7', userId: '42', detail: 'email' }),
    searchParams: Promise.resolve({}),
  })
  return render(ui)
}

const getFormProps = (testId: string) => JSON.parse(screen.getByTestId(testId).textContent ?? '{}')

// ---- Tests -----------------------------------------------------------------

describe('Edit user detail page', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    setDetail('fullName')
    mockGetUser({ user: mockUser })
  })

  describe('when no detail param is provided', () => {
    it('calls notFound and does not fetch the user', async () => {
      const { notFound } = await import('next/navigation')
      setDetail(undefined)

      await expect(renderPage()).rejects.toThrow('NEXT_NOT_FOUND')

      expect(notFound).toHaveBeenCalledTimes(1)
      expect(getUser).not.toHaveBeenCalled()
    })
  })

  describe('data fetching', () => {
    it('fetches the user for the organisation and user id', async () => {
      await renderPage()

      expect(getUser).toHaveBeenCalledTimes(1)
      expect(getUser).toHaveBeenCalledWith(7, 42)
    })
  })

  describe('error handling', () => {
    it('renders the error state when the request returns an error', async () => {
      mockGetUser({ user: mockUser, error: { message: 'boom' } })

      await renderPage()

      expect(screen.getByTestId('error-state')).toHaveTextContent(
        errorMessages.failedToRetrieveCurrentUser,
      )
      expect(screen.queryByTestId('edit-user-detail-form')).not.toBeInTheDocument()
      expect(screen.queryByTestId('edit-user-email-form')).not.toBeInTheDocument()
    })

    it('renders the error state when no user data is returned', async () => {
      mockGetUser({ user: undefined })

      await renderPage()

      expect(screen.getByTestId('error-state')).toHaveTextContent(
        errorMessages.failedToRetrieveCurrentUser,
      )
    })

    it('still renders the page header with the correct label on error', async () => {
      setDetail('email')
      mockGetUser({ user: undefined, error: { message: 'boom' } })

      await renderPage()

      expect(screen.getByRole('heading', { name: 'Edit work email address' })).toBeInTheDocument()
      expect(screen.getByTestId('back-link')).toHaveAttribute('href', returnLink)
    })
  })

  describe('page header', () => {
    it.each([
      ['fullName', 'Edit full name'],
      ['contactNumber', 'Edit contact number'],
      ['email', 'Edit work email address'],
    ])('renders the heading for %s', async (detail, heading) => {
      setDetail(detail)

      await renderPage()

      expect(screen.getByRole('heading', { name: heading })).toBeInTheDocument()
    })

    it('renders a back link to the return link', async () => {
      await renderPage()

      expect(screen.getByTestId('back-link')).toHaveAttribute('href', returnLink)
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
        onSuccessRedirect,
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
        onSuccessRedirect,
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
        onSuccessRedirect,
      })
      expect(screen.queryByTestId('edit-user-detail-form')).not.toBeInTheDocument()
      expect(screen.queryByTestId('error-state')).not.toBeInTheDocument()
    })
  })

  describe('pending email update', () => {
    it('renders pending email update alert when pending email', async () => {
      setDetail('email')

      const pendingWorkEmail = { ...fakePendingWorkEmailChangeDto(), verifiedByEmail: false }
      mockGetUser({
        user: { ...fakeUserInformationDto(), pendingWorkEmail, userId: 123 },
      })

      await renderPage()

      expect(PendingEmailUpdateAlertMock).toHaveBeenCalledTimes(1)
      expect(PendingEmailUpdateAlertMock.mock.calls[0][0]).toEqual({
        ...pendingWorkEmail,
        organisationId: 7,
        userId: 42,
        detail: 'email',
        returnLink,
        onSuccessFlag,
        owner: 'not-me',
      })
    })
  })
})
