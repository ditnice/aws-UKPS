import { unstable_rethrow } from 'next/navigation'
import 'server-only'

import { getUsersMe } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import type { UserOrganisationContext } from '@/components/Layout/Header/Header'

// Returns undefined when the user has not selected an organisation. The plain fetch replaces the
// server client's redirecting fetch, which would otherwise loop on the organisation select page.
export async function getUserOrganisationContext(): Promise<UserOrganisationContext | undefined> {
  try {
    const { data } = await getUsersMe({
      client: await createServerApiClient(),
      fetch: globalThis.fetch,
    })

    return data
      ? {
          organisationId: data.organisationId,
          organisationName: data.organisationName,
          hasMultipleOrganisations: data.hasMultipleOrganisations,
          userRole: data.userRole,
        }
      : undefined
  } catch (error) {
    unstable_rethrow(error)
    return undefined
  }
}
