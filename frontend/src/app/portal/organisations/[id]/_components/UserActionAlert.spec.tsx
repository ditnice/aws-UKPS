import { cleanup, render } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import type { Client } from '@/client/generated/client'
import { getUserDetailsWithinOrganisation } from '@/client/generated/sdk.gen'
import type { UserInformationDto } from '@/client/generated/types.gen'

import { UserActionAlert } from './UserActionAlert'

import type { UserActionResult } from '../_lib/userActionAlert'

vi.mock('@/client/generated/sdk.gen', () => ({
  getUserDetailsWithinOrganisation: vi.fn(),
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

const apiClient = {} as Client

async function renderAlert(userAction: UserActionResult) {
  const { container } = render(await UserActionAlert({ apiClient, organisationId: 2, userAction }))
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
})

afterEach(cleanup)

describe('UserActionAlert', () => {
  it('looks the user up in the organisation the alert belongs to', async () => {
    await renderAlert({ action: 'invited', userId: 4 })

    expect(getUserDetailsWithinOrganisation).toHaveBeenCalledWith({
      client: apiClient,
      path: { organisationId: 2, userId: 4 },
    })
  })

  it('names the invited user in the alert', async () => {
    const { heading, message } = await renderAlert({ action: 'invited', userId: 4 })

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

    const { heading, message } = await renderAlert({ action: 'permissions-updated', userId: 4 })

    expect(heading).toBe('Permissions changed')
    expect(message).toBe('julie.brooks@example.com is now a champion user.')
  })

  it('announces success politely rather than interrupting', async () => {
    const { alert } = await renderAlert({ action: 'invited', userId: 4 })

    expect(alert?.getAttribute('aria-live')).toBe('polite')
    expect(alert?.getAttribute('role')).toBeNull()
  })

  it.each([
    [
      'invited',
      'Invitation sent',
      'We’ve sent an email to the new user with instructions to set up an account.',
    ],
    ['permissions-updated', 'Permissions changed', "The user's permissions have been updated."],
  ] as const)(
    'still confirms a %s action, without naming the user, when they cannot be read back',
    async (action, expectedHeading, expectedMessage) => {
      vi.mocked(getUserDetailsWithinOrganisation).mockResolvedValue({
        data: undefined,
        error: { status: 404 },
      })

      const { heading, message } = await renderAlert({ action, userId: 4 })

      expect(heading).toBe(expectedHeading)
      expect(message).toBe(expectedMessage)
    },
  )
})
