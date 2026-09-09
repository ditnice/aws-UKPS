'use server'

import { revalidatePath } from 'next/cache'

import { updateUserRole } from '@/client/generated/sdk.gen'
import type { UserRole } from '@/client/generated/types.gen'
import { createServerApiClient } from '@/client/server-api'

// Super users are managed via a different flow, so only standard and champion users
// can be switched between roles from here.
type SwitchableRole = Exclude<UserRole, 'Super'>

type ChangeUserPermissionsResult = { status: 'success' } | { status: 'error'; message: string }

export async function changeUserPermissionsAction(
  organisationId: number,
  userId: number,
  membershipId: number,
  userRole: SwitchableRole,
): Promise<ChangeUserPermissionsResult> {
  const apiClient = await createServerApiClient()

  const { error } = await updateUserRole({
    client: apiClient,
    path: { organisationId, membershipId },
    body: { userRole },
  })

  if (error) {
    return {
      status: 'error',
      message: "There was a problem changing this user's permissions. Please try again later.",
    }
  }

  revalidatePath(`/portal/organisations/${organisationId}`)
  revalidatePath(`/portal/organisations/${organisationId}/manage-user-access/${userId}`)
  revalidatePath(
    `/portal/organisations/${organisationId}/manage-user-access/${userId}/change-permissions`,
  )

  return { status: 'success' }
}
