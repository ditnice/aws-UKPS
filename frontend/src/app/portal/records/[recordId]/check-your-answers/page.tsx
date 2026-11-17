import { notFound } from 'next/navigation'

import { getRecordCheckAnswers } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { Alert } from '@/components/Alert/Alert'
import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { SummaryCard } from '@/components/SummaryCard/SummaryCard'
import {
  SummaryList,
  SummaryListAction,
  SummaryListRow,
} from '@/components/SummaryList/SummaryList'
import { Tag } from '@/components/Tag/Tag'
import { formatAnswer } from '@/lib/dynamic-form/answerDisplay'
import { checkAnswersReturnTo, pageHref, taskListHref } from '@/lib/dynamic-form/links'
import { parsePositiveInteger } from '@/lib/valueParsing'

import { CheckAnswersActions } from './_components/CheckAnswersActions'

type CheckAnswersPageProps = {
  params: Promise<{ recordId: string }>
}

const CheckAnswersPage = async ({ params }: CheckAnswersPageProps) => {
  const { recordId: recordIdParam } = await params
  const recordId = parsePositiveInteger(recordIdParam)

  if (!recordId) {
    notFound()
  }

  const { data, error, response } = await getRecordCheckAnswers({
    client: await createServerApiClient(),
    path: { recordId },
  })

  if (response?.status === 404) {
    notFound()
  }

  if (!data || error) {
    return (
      <section>
        <PageHeader heading="Unable to load check your answers" />
        <ErrorState>
          There was a problem retrieving your answers. Please try again later.
        </ErrorState>
      </section>
    )
  }

  const errorsByQuestionId = new Map(data.errors.map((error) => [error.questionId, error]))

  return (
    <>
      {data.errors.length > 0 && (
        <Alert type="error">
          <h2>There is a problem</h2>
          <p>
            Change the information before you submit the record for quality assurance (QA) review:
          </p>
          <ul>
            {data.errors.map((error) => (
              <li key={error.questionId}>
                {' '}
                <a
                  href={`${pageHref({ recordId }, error.pageId)}?returnTo=${checkAnswersReturnTo}#${error.questionId}`}
                >
                  {error.message}
                </a>
              </li>
            ))}
          </ul>{' '}
        </Alert>
      )}

      <PageHeader
        backLink={<BackLink href={taskListHref(recordId)}>Back</BackLink>}
        heading="Check your answers before submitting your record for Quality Assurance (QA) review"
      />

      {data.sections.flatMap((section) =>
        section.tasks.map((task) => (
          <SummaryCard key={task.id} title={task.title}>
            <SummaryList variant="wide-value">
              {task.questions.map((question) => (
                <SummaryListRow
                  key={question.id}
                  label={question.label}
                  value={formatAnswer(question)}
                  tag={
                    errorsByQuestionId.get(question.id) && (
                      <Tag colour="red"> There is a problem </Tag>
                    )
                  }
                >
                  <SummaryListAction
                    href={`${pageHref({ recordId }, question.pageId)}?returnTo=${checkAnswersReturnTo}`}
                    visuallyHiddenText={question.label}
                  >
                    Change
                  </SummaryListAction>
                </SummaryListRow>
              ))}
            </SummaryList>
          </SummaryCard>
        )),
      )}

      {data.canSubmit && data.errors.length === 0 && (
        <CheckAnswersActions
          recordId={recordId}
          formVersion={data.formVersion}
          revisionHash={data.revisionHash}
          organisationId={data.organisationId}
        />
      )}
    </>
  )
}

export default CheckAnswersPage
