import { redirect } from 'next/navigation'

import { getUsersMe } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { ErrorState } from '@/components/Placeholder/ErrorState'

import {
  buildRecordActionHref,
  parseRecordAction,
  type RecordActionSearchParams,
} from './organisations/[id]/records/_lib/recordActionAlert'

export const dynamic = 'force-dynamic'

type PortalDashboardProps = {
  searchParams: Promise<RecordActionSearchParams>
}

export default async function PortalDashboard({ searchParams }: PortalDashboardProps) {
  const action = parseRecordAction(await searchParams)
  const { data: me, error } = await getUsersMe({ client: await createServerApiClient() })

  if (!me || error) {
    return <ErrorState>Unable to load the data for the current user.</ErrorState>
  }

  return redirect(
    action
      ? buildRecordActionHref(me.organisationId, action)
      : `/portal/organisations/${me.organisationId}/records`,
  )
}
