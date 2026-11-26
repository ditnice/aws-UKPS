import { getUserDetailsWithinOrganisationAdminView, getUsersMe } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'

export const getUser = async (organisationId: number, userId: number) => {
  const client = await createServerApiClient()
  const { data: me, error: getMeError } = await getUsersMe({ client })
  if (getMeError || !me) {
    return { user: me, error: getMeError }
  }
  if (me && me.organisationId == organisationId && me.userId == userId) {
    return { user: me }
  }

  const { data: user, error } = await getUserDetailsWithinOrganisationAdminView({
    path: { userId, organisationId },
    client,
  })

  return { user, error }
}
