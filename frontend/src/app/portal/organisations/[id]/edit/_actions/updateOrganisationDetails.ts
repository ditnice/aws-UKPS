'use server'

import { revalidatePath } from 'next/cache'

import { updateOrganisationDetails } from '@/client/generated/sdk.gen'
import type { UpdateOrganisationDetailsDto } from '@/client/generated/types.gen'
import { createServerApiClient } from '@/client/server-api'
import { positiveIdSchema } from '@/lib/validation/positiveId'

import { editOrganisationDetailsSchema } from '../_lib/organisationDetailsSchema'

export type UpdateOrganisationDetailsResult =
  { status: 'success' } | { status: 'error'; message: string }

export async function updateOrganisationDetailsAction(
  organisationId: number,
  values: UpdateOrganisationDetailsDto,
): Promise<UpdateOrganisationDetailsResult> {
  const parsedId = positiveIdSchema.safeParse(organisationId)
  const parsedValues = editOrganisationDetailsSchema.safeParse(values)

  if (!parsedId.success || !parsedValues.success) {
    return {
      status: 'error',
      message: 'Check the organisation details and try again.',
    }
  }

  const apiClient = await createServerApiClient()

  const { error } = await updateOrganisationDetails({
    client: apiClient,
    path: { id: parsedId.data },
    body: parsedValues.data,
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
