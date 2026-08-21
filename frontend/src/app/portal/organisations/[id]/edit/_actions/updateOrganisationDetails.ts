'use server'

import { revalidatePath } from 'next/cache'

import { updateOrganisationDetails } from '@/client/generated/sdk.gen'
import type { UpdateOrganisationDetailsDto } from '@/client/generated/types.gen'
import { createServerApiClient } from '@/client/server-api'

export type UpdateOrganisationDetailsResult =
  { status: 'success' } | { status: 'error'; message: string }

export async function updateOrganisationDetailsAction(
  organisationId: number,
  values: UpdateOrganisationDetailsDto,
): Promise<UpdateOrganisationDetailsResult> {
  const apiClient = await createServerApiClient()

  const { error } = await updateOrganisationDetails({
    client: apiClient,
    path: { id: organisationId },
    body: values,
  })

  if (error) {
    return {
      status: 'error',
      message: 'There was a problem updating the organisation. Please try again later.',
    }
  }

  revalidatePath(`/portal/organisations/${organisationId}`)
  revalidatePath(`/portal/organisations/${organisationId}/edit`)

  return { status: 'success' }
}
