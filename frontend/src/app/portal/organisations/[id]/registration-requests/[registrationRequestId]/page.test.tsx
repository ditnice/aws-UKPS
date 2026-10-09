import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import type { RegisterUserConfirmationDto } from '@/client/generated'
import { notFound } from '@/test-utils/nextNavigation'

import ApproveUser from './approve/page'
import RejectUser from './reject/page'

import type { ModifyUserMembershipRequestControlsProps } from './ModifyUserMembershipRequestControls'
import type { UserMembershipRetrievalWrapperProps } from './UserMembershipRetrievalWrapper'

const mocks = vi.hoisted(() => ({
  retrieve: vi.fn(),
  controls: vi.fn(),
}))

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))
vi.mock('./UserMembershipRetrievalWrapper', () => ({
  default: (props: UserMembershipRetrievalWrapperProps) => {
    mocks.retrieve(props)
    const request: RegisterUserConfirmationDto = {
      requestGuid: props.requestGuid,
      organisationName: 'Test organisation',
      fullName: 'Test user',
      workEmail: 'test@example.com',
      phoneNumber: '07845796823',
    }
    return props.children(request)
  },
}))
vi.mock('./ModifyUserMembershipRequestControls', () => ({
  default: (props: ModifyUserMembershipRequestControlsProps) => {
    mocks.controls(props)
    return <a href={props.successLink}>Success</a>
  },
}))

// Direct invocation checks GUID orchestration, not Next.js rendering or routing.
describe.each([
  ['approve', ApproveUser, 'approved-request'],
  ['reject', RejectUser, 'rejected-request'],
] as const)('%s membership request page (direct invocation)', (_action, Page, successAction) => {
  const requestGuid = 'e52c7f89-e182-41b2-bbdc-69a0fa9f034d'

  it('preserves the GUID through retrieval, mutation and the success link', async () => {
    render(await Page({ params: Promise.resolve({ id: '2', registrationRequestId: requestGuid }) }))

    expect(mocks.retrieve).toHaveBeenCalledWith(
      expect.objectContaining({ organisationId: 2, requestGuid }),
    )
    expect(mocks.controls).toHaveBeenCalledWith(
      expect.objectContaining({ organisationId: 2, requestGuid }),
    )
    expect(screen.getByRole('link', { name: 'Success' }).getAttribute('href')).toBe(
      `/portal/organisations/2?action=${successAction}&userRequestId=${requestGuid}`,
    )
    expect(notFound).not.toHaveBeenCalled()
  })

  it.each(['2', 'not-a-guid', '', `${requestGuid}extra`])(
    'rejects invalid request identifier %j',
    async (registrationRequestId) => {
      await expect(
        Page({ params: Promise.resolve({ id: '2', registrationRequestId }) }),
      ).rejects.toThrow('NEXT_NOT_FOUND')
      expect(mocks.retrieve).not.toHaveBeenCalled()
    },
  )

  it.each(['0', '-1', '1.5', 'invalid'])(
    'rejects invalid organisation identifier %j',
    async (id) => {
      await expect(
        Page({ params: Promise.resolve({ id, registrationRequestId: requestGuid }) }),
      ).rejects.toThrow('NEXT_NOT_FOUND')
    },
  )
})
