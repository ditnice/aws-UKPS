import { notFound } from 'next/navigation'

import { RecordType } from '@/client/generated'
import { parsePositiveInteger } from '@/lib/valueParsing'

export type StatusChangeRouteProps = {
  params: Promise<{ id: string; recordId: string }>
  searchParams: Promise<{ recordType?: string | string[] }>
}

/** Reads a status change page's route, or responds not found when it is invalid. */
export async function parseStatusChangeRoute({ params, searchParams }: StatusChangeRouteProps) {
  const { id: organisationId, recordId: recordIdParam } = await params
  const { recordType: recordTypeParam } = await searchParams
  const recordId = parsePositiveInteger(recordIdParam)
  const recordType = Object.values(RecordType).find((type) => type === recordTypeParam)

  if (!recordId || !recordType) {
    notFound()
  }

  return { organisationId, recordId, recordType }
}
