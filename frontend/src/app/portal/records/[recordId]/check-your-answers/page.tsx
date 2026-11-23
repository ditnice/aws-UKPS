import Link from 'next/link'
import { notFound } from 'next/navigation'

import { getRecordCheckAnswers, ProblemDetails } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { Alert } from '@/components/Alert/Alert'
import { BackLink } from '@/components/BackLink/BackLink'
import { Button } from '@/components/Button/Button'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { TaskAnswers } from '@/components/RecordAnswers/TaskAnswers'
import { SummaryCard } from '@/components/SummaryCard/SummaryCard'
import { SummaryListAction } from '@/components/SummaryList/SummaryList'
import { Tag } from '@/components/Tag/Tag'
import { checkAnswersPageHref, groupItemAnchor, taskListHref } from '@/lib/dynamic-form/links'
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
                  href={checkAnswersPageHref(recordId, error.pageId, { anchor: error.questionId })}
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

          return (
            <SummaryCard key={task.id} title={task.title}>
              <TaskAnswers
                questions={task.questions}
                groupItemHeadingLevel={3}
                rowTag={(question) =>
                  errorsByQuestionId.has(question.id) && <Tag colour="red">There is a problem</Tag>
                }
                groupTag={(question) =>
                  errorsByQuestionId.has(question.id) && (
                    <Tag colour="red">
                      There is a problem
                      <span className="visually-hidden"> with {question.label}</span>
                    </Tag>
                  )
                }
                rowAction={(question) => (
                  <SummaryListAction
                    href={checkAnswersPageHref(recordId, question.pageId)}
                    visuallyHiddenText={question.label}
                  >
                    Change
                  </SummaryListAction>
                )}
                groupItemAction={(question, index, title) => (
                  <SummaryListAction
                    href={checkAnswersPageHref(recordId, question.pageId, {
                      anchor: groupItemAnchor(question.id, index),
                    })}
                    visuallyHiddenText={title}
                  >
                    Edit
                  </SummaryListAction>
                )}
              />
              {groups.map((question) => (
                <Button
                  elementType={Link}
                  href={checkAnswersPageHref(recordId, question.pageId, {
                    addGroupItem: true,
                    anchor: groupItemAnchor(question.id, question.items?.length ?? 0),
                  })}
                  key={question.id}
                >
                  {question.addAnotherLabel ?? 'Add another'}
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
