import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { postAuthVerifyMfa, verifyMultiFactorAuthenticationUpdate } from '@/client/generated'
import { errorMessages } from '@/lib/form/errorMessages'

import SetupMfaForm from './SetupMfaForm'

vi.mock('@/client/generated', () => ({
  postAuthVerifyMfa: vi.fn(),
  verifyMultiFactorAuthenticationUpdate: vi.fn(),
}))

// Render a lightweight stand-in so we can assert on the props passed to the QR code
vi.mock('qrcode.react', () => ({
  QRCodeSVG: ({ value, ...rest }: { value: string; 'aria-label'?: string }) => (
    <svg data-testid="qr-code" data-value={value} aria-label={rest['aria-label']} />
  ),
}))

const mockedPostAuthVerifyMfa = vi.mocked(postAuthVerifyMfa)
const mockedVerifyUpdate = vi.mocked(verifyMultiFactorAuthenticationUpdate)

const SECRET = 'JBSWY3DPEHPK3PXP'
const otpAuthUri = `otpauth://totp/Example:user@example.com?secret=${SECRET}&issuer=Example`

const initialiseSetup = {
  type: 'initialise' as const,
  otpAuthUri,
  setupToken: 'setup-token-123',
  authenticationSession: 'auth-session-456',
}

const updateSetup = {
  type: 'update' as const,
  otpAuthUri,
}

const successResponse = { error: undefined, response: { status: 200 } }

// The generated client's return types are large unions; the component only reads
// `error` and `response.status`, so a loose cast keeps the tests readable.
const asClientResult = (value: unknown) => value as never

async function enterCodeAndSubmit(code?: string) {
  const user = userEvent.setup()
  if (code !== undefined) {
    await user.type(screen.getByLabelText(/enter your authentication code/i), code)
  }
  await user.click(screen.getByRole('button', { name: /continue/i }))
}

describe('SetupMfaForm', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('rendering', () => {
    it('renders the QR code using the otpAuthUri', () => {
      render(<SetupMfaForm setup={initialiseSetup} />)

      const qr = screen.getByTestId('qr-code')
      expect(qr).toHaveAttribute('data-value', otpAuthUri)
      expect(qr).toHaveAttribute('aria-label', 'QR code for authenticator app setup')
    })

    it('shows the manual setup key extracted from the otpAuthUri', () => {
      render(<SetupMfaForm setup={initialiseSetup} />)

      expect(screen.getByText(/enter this set up key/i)).toBeInTheDocument()
      expect(screen.getByText(SECRET)).toBeInTheDocument()
    })

    it('renders an empty manual key when the URI has no secret parameter', () => {
      render(
        <SetupMfaForm
          setup={{ ...initialiseSetup, otpAuthUri: 'otpauth://totp/Example:user@example.com' }}
        />,
      )

      expect(screen.queryByText(SECRET)).not.toBeInTheDocument()
    })

    it('renders the code input and continue button', () => {
      render(<SetupMfaForm setup={initialiseSetup} />)

      expect(screen.getByLabelText(/enter your authentication code/i)).toBeInTheDocument()
      expect(
        screen.getByText(/enter the 6-digit authentication code shown in your authenticator app/i),
      ).toBeInTheDocument()
      expect(screen.getByRole('button', { name: /continue/i })).toBeInTheDocument()
    })

    it('shows the alternative method link by default', () => {
      render(<SetupMfaForm setup={initialiseSetup} />)

      expect(
        screen.getByRole('link', { name: /i want to use a different method/i }),
      ).toHaveAttribute('href', '/')
    })

    it('hides the alternative method link when hideAlternativeOptions is true', () => {
      render(<SetupMfaForm setup={initialiseSetup} hideAlternativeOptions />)

      expect(
        screen.queryByRole('link', { name: /i want to use a different method/i }),
      ).not.toBeInTheDocument()
    })
  })

  describe('validation', () => {
    it('shows a required error and does not call the API when the code is empty', async () => {
      render(<SetupMfaForm setup={initialiseSetup} />)

      await enterCodeAndSubmit()

      expect(await screen.findByText(errorMessages.securityCodeRequired)).toBeInTheDocument()
      expect(mockedPostAuthVerifyMfa).not.toHaveBeenCalled()
      expect(mockedVerifyUpdate).not.toHaveBeenCalled()
    })

    it('shows a required error when the code is only whitespace', async () => {
      render(<SetupMfaForm setup={initialiseSetup} />)

      await enterCodeAndSubmit('   ')

      expect(await screen.findByText(errorMessages.securityCodeRequired)).toBeInTheDocument()
      expect(mockedPostAuthVerifyMfa).not.toHaveBeenCalled()
    })

    it.each([
      ['too short', '12345'],
      ['too long', '1234567'],
      ['non-numeric', 'abcdef'],
      ['mixed characters', '12a456'],
    ])('shows a format error when the code is %s', async (_label, code) => {
      render(<SetupMfaForm setup={initialiseSetup} />)

      await enterCodeAndSubmit(code)

      expect(await screen.findByText(errorMessages.securityCodeFormat)).toBeInTheDocument()
      expect(mockedPostAuthVerifyMfa).not.toHaveBeenCalled()
      expect(mockedVerifyUpdate).not.toHaveBeenCalled()
    })
  })

  describe('initialise flow', () => {
    it('calls postAuthVerifyMfa with the session, setup token and code', async () => {
      mockedPostAuthVerifyMfa.mockResolvedValue(asClientResult(successResponse))
      const onSuccess = vi.fn()
      render(<SetupMfaForm setup={initialiseSetup} onSuccess={onSuccess} />)

      await enterCodeAndSubmit('123456')

      await waitFor(() => expect(mockedPostAuthVerifyMfa).toHaveBeenCalledTimes(1))
      expect(mockedPostAuthVerifyMfa).toHaveBeenCalledWith({
        body: {
          authenticationSession: 'auth-session-456',
          code: '123456',
          setupToken: 'setup-token-123',
        },
        credentials: 'include',
      })
      expect(mockedVerifyUpdate).not.toHaveBeenCalled()
      await waitFor(() => expect(onSuccess).toHaveBeenCalledTimes(1))
    })

    it.each([
      ['spaces', '123 456'],
      ['hyphens', '123-456'],
      ['spaces and hyphens', ' 12-34 56 '],
    ])('normalises a code containing %s before sending', async (_label, input) => {
      mockedPostAuthVerifyMfa.mockResolvedValue(asClientResult(successResponse))
      render(<SetupMfaForm setup={initialiseSetup} />)

      await enterCodeAndSubmit(input)

      await waitFor(() => expect(mockedPostAuthVerifyMfa).toHaveBeenCalledTimes(1))
      expect(mockedPostAuthVerifyMfa).toHaveBeenCalledWith(
        expect.objectContaining({
          body: expect.objectContaining({ code: '123456' }),
        }),
      )
    })
  })

  describe('update flow', () => {
    it('calls verifyMultiFactorAuthenticationUpdate with only the code', async () => {
      mockedVerifyUpdate.mockResolvedValue(asClientResult(successResponse))
      const onSuccess = vi.fn()
      render(<SetupMfaForm setup={updateSetup} onSuccess={onSuccess} />)

      await enterCodeAndSubmit('654321')

      await waitFor(() => expect(mockedVerifyUpdate).toHaveBeenCalledTimes(1))
      expect(mockedVerifyUpdate).toHaveBeenCalledWith({
        body: { code: '654321' },
        credentials: 'include',
      })
      expect(mockedPostAuthVerifyMfa).not.toHaveBeenCalled()
      await waitFor(() => expect(onSuccess).toHaveBeenCalledTimes(1))
    })

    it('normalises the code before sending', async () => {
      mockedVerifyUpdate.mockResolvedValue(asClientResult(successResponse))
      render(<SetupMfaForm setup={updateSetup} />)

      await enterCodeAndSubmit('654 321')

      await waitFor(() => expect(mockedVerifyUpdate).toHaveBeenCalledTimes(1))
      expect(mockedVerifyUpdate).toHaveBeenCalledWith({
        body: { code: '654321' },
        credentials: 'include',
      })
    })
  })

  describe('success handling', () => {
    it('does not throw when onSuccess is not provided', async () => {
      mockedPostAuthVerifyMfa.mockResolvedValue(asClientResult(successResponse))
      render(<SetupMfaForm setup={initialiseSetup} />)

      await enterCodeAndSubmit('123456')

      await waitFor(() => expect(mockedPostAuthVerifyMfa).toHaveBeenCalledTimes(1))
      expect(screen.queryByText(/could not verify/i)).not.toBeInTheDocument()
    })
  })

  describe('error handling', () => {
    it('shows an incorrect code error on a 400 response', async () => {
      const onSuccess = vi.fn()
      mockedPostAuthVerifyMfa.mockResolvedValue(
        asClientResult({ error: { detail: 'Bad request' }, response: { status: 400 } }),
      )
      render(<SetupMfaForm setup={initialiseSetup} onSuccess={onSuccess} />)

      await enterCodeAndSubmit('123456')

      expect(await screen.findByText(errorMessages.incorrectMfaCode)).toBeInTheDocument()
      expect(onSuccess).not.toHaveBeenCalled()
    })

    it('shows the server-provided detail for other API errors', async () => {
      const onSuccess = vi.fn()
      mockedPostAuthVerifyMfa.mockResolvedValue(
        asClientResult({ error: { detail: 'Too many attempts' }, response: { status: 429 } }),
      )
      render(<SetupMfaForm setup={initialiseSetup} onSuccess={onSuccess} />)

      await enterCodeAndSubmit('123456')

      expect(await screen.findByText('Too many attempts')).toBeInTheDocument()
      expect(onSuccess).not.toHaveBeenCalled()
    })

    it('shows a generic message when the API error has no detail', async () => {
      mockedPostAuthVerifyMfa.mockResolvedValue(
        asClientResult({ error: {}, response: { status: 500 } }),
      )
      render(<SetupMfaForm setup={initialiseSetup} />)

      await enterCodeAndSubmit('123456')

      expect(
        await screen.findByText('We could not verify your authentication code. Try again later.'),
      ).toBeInTheDocument()
    })

    it('shows a generic message when the request throws', async () => {
      const onSuccess = vi.fn()
      mockedPostAuthVerifyMfa.mockRejectedValue(new Error('Network down'))
      render(<SetupMfaForm setup={initialiseSetup} onSuccess={onSuccess} />)

      await enterCodeAndSubmit('123456')

      expect(
        await screen.findByText('We could not verify your authentication code. Try again later.'),
      ).toBeInTheDocument()
      expect(onSuccess).not.toHaveBeenCalled()
    })

    it('shows errors from the update flow as well', async () => {
      mockedVerifyUpdate.mockResolvedValue(
        asClientResult({ error: { detail: 'Session expired' }, response: { status: 401 } }),
      )
      render(<SetupMfaForm setup={updateSetup} />)

      await enterCodeAndSubmit('123456')

      expect(await screen.findByText('Session expired')).toBeInTheDocument()
    })

    it('clears a previous submit error when the user retries', async () => {
      const user = userEvent.setup()
      mockedPostAuthVerifyMfa
        .mockResolvedValueOnce(
          asClientResult({ error: { detail: 'Server hiccup' }, response: { status: 500 } }),
        )
        .mockResolvedValueOnce(asClientResult(successResponse))
      render(<SetupMfaForm setup={initialiseSetup} />)

      await user.type(screen.getByLabelText(/enter your authentication code/i), '123456')
      await user.click(screen.getByRole('button', { name: /continue/i }))
      expect(await screen.findByText('Server hiccup')).toBeInTheDocument()

      await user.click(screen.getByRole('button', { name: /continue/i }))

      await waitFor(() => expect(screen.queryByText('Server hiccup')).not.toBeInTheDocument())
      expect(mockedPostAuthVerifyMfa).toHaveBeenCalledTimes(2)
    })
  })
})
