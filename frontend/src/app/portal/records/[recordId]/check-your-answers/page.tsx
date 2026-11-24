import Link from 'next/link'
import { notFound } from 'next/navigation'

import { getRecordCheckAnswers, ProblemDetails } from '@/client/generated'
import type { RecordAnswersQuestionDto } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { Alert } from '@/components/Alert/Alert'
import { BackLink } from '@/components/BackLink/BackLink'
import { Button } from '@/components/Button/Button'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { TaskAnswers } from '@/components/RecordAnswers/TaskAnswers'
import { SummaryCard, SummaryCardHint } from '@/components/SummaryCard/SummaryCard'
import {
  SummaryList,
  SummaryListAction,
  SummaryListRow,
} from '@/components/SummaryList/SummaryList'
import { Tag } from '@/components/Tag/Tag'
import { checkAnswersPageHref, groupItemAnchor, taskListHref } from '@/lib/dynamic-form/links'
import { parsePositiveInteger } from '@/lib/valueParsing'

import { CheckAnswersActions } from './_components/CheckAnswersActions'

type CheckAnswersPageProps = {
  params: Promise<{ recordId: string }>
}

/**
 * Questions not visible to horizon scanners, tagged "Hidden". The endpoint has no per-question
 * flag yet, so the one row is named here.
 */
const hiddenQuestionIds = new Set(['record_product_detail.record_title'])

const isUnanswered = (question: RecordAnswersQuestionDto) =>
  question.values.length === 0 && !question.items?.length

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
          // An optional task with nothing answered is one "Not yet provided" row, not a row each.
          const untouched =
            section.optional && !task.notApplicable && task.questions.every(isUnanswered)
          const groups = untouched ? [] : task.questions.filter((question) => question.items)

          return (
            <SummaryCard
              key={task.id}
              title={section.optional ? `${task.title} (optional)` : task.title}
            >
              {/* TODO: once the 12-month rule exists the backend decides per record whether the
                  section is still optional. A mandatory task drops "(optional)" and this line, and
                  shows its errors. */}
              {section.optional && (
                <p>
                  This task becomes mandatory within 12 months of the market authorisation date.
                </p>
              )}
              {/* The copy assumes PIM and EAMS on a biosimilar, the only task that can be wholly
                  conditional today. A second one needs the backend to send the reason. */}
              {task.notApplicable && (
                <SummaryCardHint>
                  Your product is a biosimilar, you do not need to complete this task. You can
                  change technology status in indication details.
                </SummaryCardHint>
              )}
              {untouched ? (
                <SummaryList variant="wide-value">
                  <SummaryListRow label="Not yet provided" plainLabel value="">
                    <SummaryListAction
                      href={checkAnswersPageHref(recordId, task.questions[0].pageId)}
                      visuallyHiddenText={task.title}
                    >
                      Edit section
                    </SummaryListAction>
                  </SummaryListRow>
                </SummaryList>
              ) : (
                <TaskAnswers
                  questions={task.questions}
                  groupItemHeadingLevel={3}
                  showEmptyGroupCard
                  rowTag={(question) => {
                    const hidden = hiddenQuestionIds.has(question.id)
                    const problem = errorsByQuestionId.has(question.id)
                    return (
                      (hidden || problem) && (
                        <>
                          {hidden && <Tag colour="purple">Hidden</Tag>}
                          {problem && <Tag colour="red">There is a problem</Tag>}
                        </>
                      )
                    )
                  }}
                  // A group with no items shows an empty card, so there is always a card to tag.
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
              )}
              {groups.map((question) => (
                <Button
                  elementType={Link}
                  href={checkAnswersPageHref(recordId, question.pageId, {
                    addGroupItem: true,
                    anchor: groupItemAnchor(question.id, question.items?.length ?? 0),
                  })}
                  key={question.id}
                >
                  {question.checkAnswersAddLabel ?? 'Add'}
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
