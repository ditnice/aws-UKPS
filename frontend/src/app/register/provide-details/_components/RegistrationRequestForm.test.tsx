import { act, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { vi } from 'vitest'

import {
  getOrganisationsPublicOptions,
  registerUser,
  type RegisterUserConfirmationDto,
} from '@/client/generated'
import { errorMessages } from '@/lib/form/errorMessages'
import { router } from '@/test-utils/nextNavigation'
import { fillInput } from '@/test-utils/userInteractions'

import { RegistrationRequestForm } from './RegistrationRequestForm'

const { push: mockPush } = router

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

vi.mock('@/client/generated', () => ({
  getOrganisationsPublicOptions: vi.fn(),
  registerUser: vi.fn(),
}))

const organisationLabel = 'Select the organisation you are requesting access for'

afterEach(() => {
  sessionStorage.clear()
})

const confirmation: RegisterUserConfirmationDto = {
  id: 42,
  organisationName: 'Test Organisation',
  fullName: 'Jane Smith',
  workEmail: 'jane.smith@example.com',
  phoneNumber: '07400123456',
}

beforeEach(() => {
  vi.mocked(getOrganisationsPublicOptions).mockResolvedValue({
    data: [{ id: 1, organisationName: 'Test Organisation' }],
    error: undefined,
  } as Awaited<ReturnType<typeof getOrganisationsPublicOptions>>)
  vi.mocked(registerUser).mockResolvedValue({
    data: undefined,
    error: { title: 'error' },
  } as Awaited<ReturnType<typeof registerUser>>)
})

let user: ReturnType<typeof userEvent.setup>

beforeEach(() => {
  user = userEvent.setup()
})

describe('RegistrationRequestForm', () => {
  it('shows the organisation required error when submitted without an organisation', async () => {
    render(<RegistrationRequestForm />)

    await user.click(screen.getByRole('button', { name: 'Submit request' }))

    expect(await screen.findByText(errorMessages.organisationRequired)).toBeInTheDocument()
  })

  it('shows the organisation required error when the placeholder is reselected', async () => {
    render(<RegistrationRequestForm />)

    await screen.findByRole('option', { name: 'Test Organisation' })
    const select = screen.getByLabelText(organisationLabel)

    await user.selectOptions(select, '1')
    await user.selectOptions(select, '')
    await user.click(screen.getByRole('button', { name: 'Submit request' }))

    expect(await screen.findByText(errorMessages.organisationRequired)).toBeInTheDocument()
    expect(screen.queryByText(/expected number/)).toBeNull()
  })

  it('submits the selected organisation id as a number', async () => {
    render(<RegistrationRequestForm />)

    await screen.findByRole('option', { name: 'Test Organisation' })

    await user.selectOptions(screen.getByLabelText(organisationLabel), '1')
    await fillInput(user, screen.getByLabelText('Full name'), 'Jane Smith')
    await fillInput(user, screen.getByLabelText('Work email address'), 'jane.smith@example.com')
    await fillInput(user, screen.getByLabelText('Phone number'), '07400 123456')
    await user.click(screen.getByRole('button', { name: 'Submit request' }))

    await waitFor(() =>
      expect(registerUser).toHaveBeenCalledWith({
        path: { organisationId: 1 },
        body: {
          fullName: 'Jane Smith',
          workEmail: 'jane.smith@example.com',
          phoneNumber: '07400 123456',
        },
      }),
    )
  })

  it('stores the confirmation and redirects after a successful registration request', async () => {
    vi.mocked(registerUser).mockResolvedValue({ data: confirmation, error: undefined })

    render(<RegistrationRequestForm />)
    await screen.findByRole('option', { name: 'Test Organisation' })
    await user.selectOptions(screen.getByLabelText(organisationLabel), '1')
    await fillInput(user, screen.getByLabelText('Full name'), 'Jane Smith')
    await fillInput(user, screen.getByLabelText('Work email address'), 'jane.smith@example.com')
    await fillInput(user, screen.getByLabelText('Phone number'), '07400123456')
    await user.click(screen.getByRole('button', { name: 'Submit request' }))

    await waitFor(() => {
      expect(sessionStorage.getItem('request_42')).toBe(JSON.stringify(confirmation))
      expect(mockPush).toHaveBeenCalledWith('/register/request-submitted/request_42')
    })
  })

  it('shows no organisation options when loading options fails', async () => {
    vi.mocked(getOrganisationsPublicOptions).mockResolvedValue({
      data: undefined,
      error: { title: 'Could not load organisations' },
    } as Awaited<ReturnType<typeof getOrganisationsPublicOptions>>)

    await act(async () => {
      render(<RegistrationRequestForm />)
      await Promise.resolve()
    })

    expect(screen.getByRole('option', { name: 'Choose organisation' })).toBeInTheDocument()
    expect(screen.getAllByRole('option')).toHaveLength(1)
    expect(screen.queryByRole('option', { name: 'Test Organisation' })).toBeNull()
  })
})
