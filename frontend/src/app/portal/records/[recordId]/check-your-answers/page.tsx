import Link from 'next/link'
import { notFound } from 'next/navigation'

import { getRecordCheckAnswers, ProblemDetails } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { Alert } from '@/components/Alert/Alert'
import { BackLink } from '@/components/BackLink/BackLink'
import { Button } from '@/components/Button/Button'
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
import {
  addGroupItemToken,
  checkAnswersReturnTo,
  groupItemAnchor,
  pageHref,
  taskListHref,
} from '@/lib/dynamic-form/links'
import { parsePositiveInteger } from '@/lib/valueParsing'

import { CheckAnswersActions } from './_components/CheckAnswersActions'
import { GroupItemCards } from './_components/GroupItemCards'

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

  if (response?.status === 403 || response?.status === 409) {
    const { detail } = (error ?? {}) as ProblemDetails
    return (
      <section>
        <PageHeader
          backLink={<BackLink href={taskListHref(recordId)}>Back</BackLink>}
          heading="This record can't be submitted"
        />
        <ErrorState>
          {detail ?? 'Return to the task list to see the status of this record.'}
        </ErrorState>
      </section>
    )
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
                <a
                  href={`${pageHref({ recordId }, error.pageId)}?returnTo=${checkAnswersReturnTo}#${error.questionId}`}
                >
                  {error.message}
                </a>
              </li>
            ))}
          </ul>
        </Alert>
      )}

      <PageHeader
        backLink={<BackLink href={taskListHref(recordId)}>Back</BackLink>}
        heading="Check your answers before submitting your record for Quality Assurance (QA) review"
      />

      {data.sections.flatMap((section) =>
        section.tasks.map((task) => {
          const groups = task.questions.filter((question) => question.items)
          const rows = task.questions.filter((question) => !question.items)

          return (
            <SummaryCard key={task.id} title={task.title}>
              {groups.map((group) => (
                <GroupItemCards key={group.id} question={group} recordId={recordId} />
              ))}
              <SummaryList variant="wide-value">
                {rows.map((question) => (
                  <SummaryListRow
                    key={question.id}
                    label={question.label}
                    value={formatAnswer(question)}
                    tag={
                      errorsByQuestionId.get(question.id) && (
                        <Tag colour="red">There is a problem</Tag>
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
              {groups.map((group) => (
                <Button
                  elementType={Link}
                  href={`${pageHref({ recordId }, group.pageId)}?returnTo=${checkAnswersReturnTo}&add=${addGroupItemToken}#${groupItemAnchor(group.id, group.items?.length ?? 0)}`}
                  key={group.id}
                >
                  Add another {group.itemLabel?.toLowerCase() ?? 'item'}
                </Button>
              ))}
            </SummaryCard>
          )
        }),
      )}

      <CheckAnswersActions
        recordId={recordId}
        formVersion={data.formVersion}
        revisionHash={data.revisionHash}
        organisationId={data.organisationId}
        canSubmit={data.errors.length === 0}
      />
    </>
  )
}

export default CheckAnswersPage
