import { notFound } from 'next/navigation'

import { getRecordPage } from '@/client/generated/sdk.gen'
import { createServerApiClient } from '@/client/server-api'
import { FormPage } from '@/components/DynamicForm/FormPage'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { parsePositiveInteger } from '@/lib/valueParsing'

interface Props {
  params: Promise<{ recordId: string; revisionId: string; pageId: string }>
}

export default async function RecordFormPage({ params }: Props) {
  const { recordId: recordIdParam, revisionId: revisionIdParam, pageId } = await params
  const recordId = parsePositiveInteger(recordIdParam)
  const revisionId = parsePositiveInteger(revisionIdParam)

  if (recordId === null || revisionId === null) {
    notFound()
  }

  const apiClient = await createServerApiClient()
  const { data: page, response } = await getRecordPage({
    client: apiClient,
    path: { recordId, revisionId, pageId },
  })

  if (response?.status === 404) {
    notFound()
  }

  if (!page) {
    return (
      <>
        <PageHeader heading="Record" />
        <ErrorState>There was a problem loading this page. Please try again later.</ErrorState>
      </>
    )
  }

  // Key by page so navigating between pages resets the form state.
  return <FormPage key={page.page.id} page={page} recordId={recordId} revisionId={revisionId} />
}
