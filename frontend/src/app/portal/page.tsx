import { redirect } from 'next/navigation'

import { getUsersMe } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { ErrorState } from '@/components/Placeholder/ErrorState'

export default async function PortalDashboard() {
  const { data: me, error } = await getUsersMe({ client: await createServerApiClient() })

  if (!me || error) {
    return <ErrorState>Unable to load the data for the current user.</ErrorState>
  }

  return redirect(`/portal/organisations/${me.organisationId}/records`)
}
