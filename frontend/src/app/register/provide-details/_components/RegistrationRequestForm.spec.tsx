import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { vi } from 'vitest'

import { getOrganisationsPublicOptions, registerUser } from '@/client/generated'
import { errorMessages } from '@/lib/form/errorMessages'

import { RegistrationRequestForm } from './RegistrationRequestForm'

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    push: vi.fn(),
  }),
}))

vi.mock('@/client/generated', () => ({
  getOrganisationsPublicOptions: vi.fn(),
  registerUser: vi.fn(),
}))

const organisationLabel = 'Select the organisation you are requesting access for'

afterEach(cleanup)

beforeEach(() => {
  vi.clearAllMocks()
  vi.mocked(getOrganisationsPublicOptions).mockResolvedValue({
    data: [{ id: 1, organisationName: 'Test Organisation' }],
    error: undefined,
  } as Awaited<ReturnType<typeof getOrganisationsPublicOptions>>)
  vi.mocked(registerUser).mockResolvedValue({
    data: undefined,
    error: { title: 'error' },
  } as Awaited<ReturnType<typeof registerUser>>)
})

describe('RegistrationRequestForm', () => {
  it('shows the organisation required error when submitted without an organisation', async () => {
    render(<RegistrationRequestForm />)

    fireEvent.click(screen.getByRole('button', { name: 'Submit request' }))

    expect(await screen.findByText(errorMessages.organisationRequired)).toBeDefined()
  })

  it('shows the organisation required error when the placeholder is reselected', async () => {
    render(<RegistrationRequestForm />)

    await screen.findByRole('option', { name: 'Test Organisation' })
    const select = screen.getByLabelText(organisationLabel)

    fireEvent.change(select, { target: { value: '1' } })
    fireEvent.change(select, { target: { value: '' } })
    fireEvent.click(screen.getByRole('button', { name: 'Submit request' }))

    expect(await screen.findByText(errorMessages.organisationRequired)).toBeDefined()
    expect(screen.queryByText(/expected number/)).toBeNull()
  })

  it('submits the selected organisation id as a number', async () => {
    render(<RegistrationRequestForm />)

    await screen.findByRole('option', { name: 'Test Organisation' })

    fireEvent.change(screen.getByLabelText(organisationLabel), { target: { value: '1' } })
    fireEvent.change(screen.getByLabelText('Full name'), { target: { value: 'Jane Smith' } })
    fireEvent.change(screen.getByLabelText('Work email address'), {
      target: { value: 'jane.smith@example.com' },
    })
    fireEvent.change(screen.getByLabelText('Phone number'), { target: { value: '07400 123456' } })
    fireEvent.click(screen.getByRole('button', { name: 'Submit request' }))

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
})
