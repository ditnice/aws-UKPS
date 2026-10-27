import { notFound } from 'next/navigation'

import { getRecordFormPage } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { FormPage } from '@/components/DynamicForm/FormPage'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { parsePositiveInteger } from '@/lib/valueParsing'

const pageIdPattern = /^[a-z0-9]+(-[a-z0-9]+)*$/

type RecordFormRouteProps = {
  params: Promise<{ recordId: string; pageId: string }>
}

/** A page of a record's content form, described by the backend. */
const RecordFormRoute = async ({ params }: RecordFormRouteProps) => {
  const { recordId: recordIdParam, pageId } = await params
  const recordId = parsePositiveInteger(recordIdParam)

  if (!recordId || !pageIdPattern.test(pageId)) {
    notFound()
  }

  const { data: page, response } = await getRecordFormPage({
    client: await createServerApiClient(),
    path: { recordId, pageId },
  })

  if (response?.status === 404) {
    notFound()
  }

  if (!page) {
    return (
      <section>
        <PageHeader heading="Unable to load page" />
        <ErrorState>There was a problem retrieving this page. Please try again later.</ErrorState>
      </section>
    )
  }

  // Keyed by page and revision version so navigating between pages starts a fresh form.
  return <FormPage key={`${page.page.id}-${page.revisionVersion}`} page={page} />
}

export default RecordFormRoute
