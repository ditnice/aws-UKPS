import { notFound } from 'next/navigation'

import { getRecordFormPage } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { checkAnswersReturnTo, FormPage } from '@/components/DynamicForm/FormPage'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { parsePositiveInteger } from '@/lib/valueParsing'

const pageIdPattern = /^[a-z0-9]+(-[a-z0-9]+)*$/

type RecordFormRouteProps = {
  params: Promise<{ recordId: string; pageId: string }>
  searchParams: Promise<{ returnTo?: string | string[] }>
}

/** A page of a record's content form, described by the backend. */
const RecordFormRoute = async ({ params, searchParams }: RecordFormRouteProps) => {
  const { recordId: recordIdParam, pageId } = await params
  const { returnTo } = await searchParams
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

  // Keyed by page so navigating between pages starts a fresh form.
  return (
    <FormPage
      key={page.page.id}
      page={page}
      returnToCheckAnswers={returnTo === checkAnswersReturnTo}
    />
  )
}

export default RecordFormRoute
