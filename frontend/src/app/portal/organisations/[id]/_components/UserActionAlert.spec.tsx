import { cleanup, render, waitFor } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { fakeRegisterUserConfirmationDto } from '@/client/generated/@faker-js/faker.gen'
import type { Client } from '@/client/generated/client'
import {
  getUserDetailsWithinOrganisation,
  getUserRegistrationById,
} from '@/client/generated/sdk.gen'
import type { RegisterUserConfirmationDto, UserInformationDto } from '@/client/generated/types.gen'

import { UserActionAlert } from './UserActionAlert'

import type { UserActionResult } from '../_lib/userActionAlert'

vi.mock('@/client/generated/sdk.gen', () => ({
  getUserDetailsWithinOrganisation: vi.fn(),
  getUserRegistrationById: vi.fn(),
}))
vi.mock('@/client/server-api', () => ({
  createServerApiClient: vi.fn().mockResolvedValue({}),
}))

const user: UserInformationDto = {
  userId: 4,
  fullName: 'Julie Brooks',
  workTelephone: '01234 567890',
  workEmail: 'julie.brooks@example.com',
  organisationMembershipId: 9,
  organisationId: 2,
  organisationName: 'Example Pharma',
  userRole: 'Standard',
}

const registration: RegisterUserConfirmationDto = fakeRegisterUserConfirmationDto()

const apiClient = {} as Client

async function renderAlert(userAction: UserActionResult) {
  const { container } = render(await UserActionAlert({ organisationId: 2, userAction }))
  const alert = container.querySelector('[data-component^="alert"]')

  return {
    alert,
    heading: alert?.querySelector('h3')?.textContent,
    message: alert?.querySelector('p')?.textContent,
  }
}

beforeEach(() => {
  vi.clearAllMocks()
  vi.mocked(getUserDetailsWithinOrganisation).mockResolvedValue({ data: user, error: undefined })
  vi.mocked(getUserRegistrationById).mockResolvedValue({ data: registration, error: undefined })
})

afterEach(cleanup)

describe('UserActionAlert', () => {
  it('looks the user up in the organisation the alert belongs to', async () => {
    await renderAlert({ type: 'user', action: 'invited', userId: 4 })

    expect(getUserDetailsWithinOrganisation).toHaveBeenCalledWith({
      client: apiClient,
      path: { organisationId: 2, userId: 4 },
    })
  })

  it('names the invited user in the alert', async () => {
    const { heading, message } = await renderAlert({ type: 'user', action: 'invited', userId: 4 })

    expect(heading).toBe('Invitation sent')
    expect(message).toBe(
      'We’ve sent an email to julie.brooks@example.com with instructions to set up an account.',
    )
  })

  it('names the user and their new role after a permissions change', async () => {
    vi.mocked(getUserDetailsWithinOrganisation).mockResolvedValue({
      data: { ...user, userRole: 'Champion' },
      error: undefined,
    })

    const { heading, message } = await renderAlert({
      type: 'user',
      action: 'permissions-updated',
      userId: 4,
    })

    expect(heading).toBe('Permissions changed')
    expect(message).toBe('julie.brooks@example.com is now a champion user.')
  })

  it('names the user that put in the request after approval', async () => {
    const { heading, message } = await renderAlert({
      type: 'request',
      action: 'approved-request',
      userRequestId: 4,
    })

    expect(heading).toBe('Approval Email Sent')
    expect(message).toBe(
      `We’ve sent an email to ${registration.workEmail} notifying them that their request has been approved and instructions to set up an account.`,
    )
  })

  it('names the user that put in the request after rejected', async () => {
    const { heading, message } = await renderAlert({
      type: 'request',
      action: 'rejected-request',
      userRequestId: 4,
    })

    expect(heading).toBe('Rejection Email Sent')
    expect(message).toBe(
      `We’ve sent an email to ${registration.workEmail} notifying them that their request has been rejected.`,
    )
  })

  it('announces success politely rather than interrupting', async () => {
    const { alert } = await renderAlert({ type: 'user', action: 'invited', userId: 4 })

    expect(alert?.getAttribute('aria-live')).toBe('polite')
    expect(alert?.getAttribute('role')).toBeNull()
  })

  it.each([
    [
      { type: 'user', action: 'invited', userId: 4 },
      'Invitation sent',
      'We’ve sent an email to the new user with instructions to set up an account.',
    ],
    [
      { type: 'user', action: 'permissions-updated', userId: 4 },
      'Permissions changed',
      "The user's permissions have been updated.",
    ],
    [
      { type: 'request', action: 'approved-request', userRequestId: 4 },
      'Approval Email Sent',
      'We’ve sent an email to the new user notifying them that their request has been approved and instructions to set up an account.',
    ],
    [
      { type: 'request', action: 'rejected-request', userRequestId: 4 },
      'Rejection Email Sent',
      'We’ve sent an email to the user notifying them that their request has been rejected.',
    ],
  ] as const)(
    'still confirms a %s action, without naming the user, when they cannot be read back',
    async (userAction, expectedHeading, expectedMessage) => {
      vi.mocked(getUserDetailsWithinOrganisation).mockResolvedValue({
        data: undefined,
        error: { status: 404 },
      })
      vi.mocked(getUserRegistrationById).mockResolvedValue({
        data: undefined,
        error: { status: 404 },
      })

      const { heading, message } = await renderAlert(userAction)

      expect(heading).toBe(expectedHeading)
      expect(message).toBe(expectedMessage)
    },
  )
})
