import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import Page from './page'

// ---- Mocks -----------------------------------------------------------------

const { redirectMock, getOtpCookieDataMock, generateOtpMock, buildMyDetailsHrefMock } = vi.hoisted(
  () => ({
    redirectMock: vi.fn(),
    getOtpCookieDataMock: vi.fn(),
    generateOtpMock: vi.fn(),
    buildMyDetailsHrefMock: vi.fn(),
  }),
)

// Real `redirect` throws a NEXT_REDIRECT error to halt rendering, so we mimic that.
vi.mock('next/navigation', () => ({
  redirect: (url: string) => {
    redirectMock(url)
    throw new Error('NEXT_REDIRECT')
  },
}))

vi.mock('./mfaHelpers', () => ({
  getOtpCookieData: getOtpCookieDataMock,
}))

vi.mock('./generateOtpAction', () => ({
  generateOtp: generateOtpMock,
}))

vi.mock('../navigation', () => ({
  buildMyDetailsHref: buildMyDetailsHrefMock,
}))

vi.mock('./SetupMfaFormWrapper', () => ({
  default: ({ redirectOnSuccess, setup }: { redirectOnSuccess: string; setup: unknown }) => (
    <div
      data-testid="setup-mfa-form-wrapper"
      data-redirect-on-success={redirectOnSuccess}
      data-setup={JSON.stringify(setup)}
    />
  ),
}))

vi.mock('@/components/PageHeader/PageHeader', () => ({
  PageHeader: ({ heading, backLink }: { heading: string; backLink: React.ReactNode }) => (
    <header>
      <h1>{heading}</h1>
      {backLink}
    </header>
  ),
}))

vi.mock('@/components/BackLink/BackLink', () => ({
  BackLink: ({ href }: { href: string }) => <a href={href}>Back</a>,
}))

vi.mock('@/components/Button/Button', () => ({
  Button: ({
    children,
    buttonType,
  }: {
    children: React.ReactNode
    buttonType: 'submit' | 'button'
  }) => <button type={buttonType}>{children}</button>,
}))

vi.mock('@/components/Placeholder/ErrorState', () => ({
  ErrorState: ({ children }: { children: React.ReactNode }) => <div role="alert">{children}</div>,
}))

// ---- Helpers ---------------------------------------------------------------

type SearchParams = { error?: string; verify?: string }

// Async server components are just async functions: await them, then render the JSX.
const renderPage = async (searchParams: SearchParams = {}) => {
  const ui = await Page({ searchParams: Promise.resolve(searchParams) })
  return render(ui)
}

// ---- Tests -----------------------------------------------------------------

describe('Setup MFA page', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    buildMyDetailsHrefMock.mockImplementation((params?: { mfaUpdated?: string }) =>
      params?.mfaUpdated ? `/my-details?mfaUpdated=${params.mfaUpdated}` : '/my-details',
    )
  })

  describe('initial step (no verify param)', () => {
    it('renders the page header with heading and back link to My Details', async () => {
      await renderPage()

      expect(
        screen.getByRole('heading', { level: 1, name: 'Update two-factor authentication' }),
      ).toBeInTheDocument()
      expect(screen.getByRole('link', { name: 'Back' })).toHaveAttribute('href', '/my-details')
    })

    it('renders the explanatory copy', async () => {
      await renderPage()

      expect(
        screen.getByText(/we will first generate a one-time passcode \(OTP\)/i),
      ).toBeInTheDocument()
    })

    it('renders a submit button to generate the OTP', async () => {
      const { container } = await renderPage()

      const button = screen.getByRole('button', { name: 'Generate OTP' })
      expect(button).toHaveAttribute('type', 'submit')
      expect(container.querySelector('form')).toContainElement(button)
    })

    it('does not render the MFA setup form or look up the OTP cookie', async () => {
      await renderPage()

      expect(screen.queryByTestId('setup-mfa-form-wrapper')).not.toBeInTheDocument()
      expect(getOtpCookieDataMock).not.toHaveBeenCalled()
    })

    it('does not trigger OTP generation or a redirect on render', async () => {
      await renderPage()

      expect(generateOtpMock).not.toHaveBeenCalled()
      expect(redirectMock).not.toHaveBeenCalled()
    })

    it('does not render an error when there is no error param', async () => {
      await renderPage()

      expect(screen.queryByRole('alert')).not.toBeInTheDocument()
    })
  })

  describe('error handling', () => {
    it('shows the specific message for a known error code', async () => {
      await renderPage({ error: 'generate-failed' })

      expect(screen.getByRole('alert')).toHaveTextContent(
        'We could not generate a one-time passcode. Please try again, and contact support if the problem continues.',
      )
    })

    it('falls back to a generic message for an unknown error code', async () => {
      await renderPage({ error: 'something-unexpected' })

      expect(screen.getByRole('alert')).toHaveTextContent('Something went wrong. Please try again.')
    })

    it('still renders the generate form alongside the error', async () => {
      await renderPage({ error: 'generate-failed' })

      expect(screen.getByRole('button', { name: 'Generate OTP' })).toBeInTheDocument()
    })
  })

  describe('verify step (verify param present)', () => {
    const setup = { secret: 'ABC123', challengeId: 'challenge-1' }

    it('renders the setup form wrapper with the cookie data and success redirect', async () => {
      getOtpCookieDataMock.mockResolvedValue(setup)

      await renderPage({ verify: '1' })

      const wrapper = screen.getByTestId('setup-mfa-form-wrapper')
      expect(wrapper).toHaveAttribute('data-redirect-on-success', '/my-details?mfaUpdated=primary')
      expect(wrapper).toHaveAttribute('data-setup', JSON.stringify(setup))
      expect(buildMyDetailsHrefMock).toHaveBeenCalledWith({ mfaUpdated: 'primary' })
    })

    it('keeps the page header and back link', async () => {
      getOtpCookieDataMock.mockResolvedValue(setup)

      await renderPage({ verify: '1' })

      expect(
        screen.getByRole('heading', { name: 'Update two-factor authentication' }),
      ).toBeInTheDocument()
      expect(screen.getByRole('link', { name: 'Back' })).toHaveAttribute('href', '/my-details')
    })

    it('does not render the generate OTP form or intro copy', async () => {
      getOtpCookieDataMock.mockResolvedValue(setup)

      await renderPage({ verify: '1' })

      expect(screen.queryByRole('button', { name: 'Generate OTP' })).not.toBeInTheDocument()
      expect(screen.queryByText(/we will first generate/i)).not.toBeInTheDocument()
    })

    it('redirects back to the start when there is no OTP challenge (expired or direct visit)', async () => {
      getOtpCookieDataMock.mockResolvedValue(null)

      await expect(renderPage({ verify: '1' })).rejects.toThrow('NEXT_REDIRECT')

      expect(redirectMock).toHaveBeenCalledTimes(1)
      expect(redirectMock).toHaveBeenCalledWith('?')
    })

    it('prioritises the verify step over an error param', async () => {
      getOtpCookieDataMock.mockResolvedValue(setup)

      await renderPage({ verify: '1', error: 'generate-failed' })

      expect(screen.getByTestId('setup-mfa-form-wrapper')).toBeInTheDocument()
      expect(screen.queryByRole('alert')).not.toBeInTheDocument()
    })
  })
})
