import Link from 'next/link'
import { notFound } from 'next/navigation'

import {
  getRecordCheckAnswers,
  ProblemDetails,
  type RecordCheckAnswersQuestionDto,
} from '@/client/generated'
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
import { checkAnswersPageHref, groupItemAnchor, taskListHref } from '@/lib/dynamic-form/links'
import { parsePositiveInteger } from '@/lib/valueParsing'

import { CheckAnswersActions } from './_components/CheckAnswersActions'
import { GroupItemCards } from './_components/GroupItemCards'

type CheckAnswersPageProps = {
  params: Promise<{ recordId: string }>
}

type TaskBlock =
  | { kind: 'rows'; questions: RecordCheckAnswersQuestionDto[] }
  | { kind: 'group'; question: RecordCheckAnswersQuestionDto }

/** Splits a task's questions, in form order, into runs of plain rows and the group questions between them. */
const splitTaskQuestions = (questions: RecordCheckAnswersQuestionDto[]): TaskBlock[] => {
  const blocks: TaskBlock[] = []
  for (const question of questions) {
    const last = blocks.at(-1)
    if (question.items) {
      blocks.push({ kind: 'group', question })
    } else if (last?.kind === 'rows') {
      last.questions.push(question)
    } else {
      blocks.push({ kind: 'rows', questions: [question] })
    }
  }
  return blocks
}

/**
 * Questions not visible to horizon scanners, tagged "Hidden". The endpoint has no per-question
 * flag yet, so the one row is named here.
 */
const hiddenQuestionIds = new Set(['record_product_detail.record_title'])

const isUnanswered = (question: RecordCheckAnswersQuestionDto) =>
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
          const blocks = untouched ? [] : splitTaskQuestions(task.questions)

          return (
            <SummaryCard
              key={task.id}
              title={section.optional ? `${task.title} (optional)` : task.title}
            >
              {/* TODO: once the 12-month rule exists the backend decides per record whether the section
                  is still optional. A mandatory task drops "(optional)" and this line, and shows its
                  errors (design: "serv readiness is mandatory"). Same TODO in the task list page. */}
              {section.optional && (
                <p>
                  This task becomes mandatory within 12 months of the market authorisation date.
                </p>
              )}
              {/* The copy assumes PIM and EAMS on a biosimilar, the only task that can be wholly
                  conditional today. A second one needs the backend to send the reason. */}
              {task.notApplicable && (
                <p>
                  Your product is a biosimilar, you do not need to complete this task. You can
                  change technology status in indication details.
                </p>
              )}
              {untouched && (
                <SummaryList variant="wide-value">
                  <SummaryListRow label="Not yet provided" value="">
                    <SummaryListAction
                      href={checkAnswersPageHref(recordId, task.questions[0].pageId)}
                      visuallyHiddenText={task.title}
                    >
                      Edit section
                    </SummaryListAction>
                  </SummaryListRow>
                </SummaryList>
              )}
              {blocks.map((block, index) =>
                block.kind === 'group' ? (
                  <GroupItemCards
                    hasError={errorsByQuestionId.has(block.question.id)}
                    key={block.question.id}
                    question={block.question}
                    recordId={recordId}
                  />
                ) : (
                  <SummaryList key={`rows-${index}`} variant="wide-value">
                    {block.questions.map((question) => (
                      <SummaryListRow
                        key={question.id}
                        label={question.label}
                        value={formatAnswer(question)}
                        tag={
                          (hiddenQuestionIds.has(question.id) ||
                            errorsByQuestionId.has(question.id)) && (
                            <>
                              {hiddenQuestionIds.has(question.id) && (
                                <Tag colour="purple">Hidden</Tag>
                              )}
                              {errorsByQuestionId.has(question.id) && (
                                <Tag colour="red">There is a problem</Tag>
                              )}
                            </>
                          )
                        }
                      >
                        <SummaryListAction
                          href={checkAnswersPageHref(recordId, question.pageId)}
                          visuallyHiddenText={question.label}
                        >
                          Change
                        </SummaryListAction>
                      </SummaryListRow>
                    ))}
                  </SummaryList>
                ),
              )}
              {blocks.map(
                (block) =>
                  block.kind === 'group' && (
                    <Button
                      elementType={Link}
                      href={checkAnswersPageHref(recordId, block.question.pageId, {
                        addGroupItem: true,
                        anchor: groupItemAnchor(
                          block.question.id,
                          block.question.items?.length ?? 0,
                        ),
                      })}
                      key={block.question.id}
                    >
                      {block.question.addAnotherLabel ?? 'Add another'}
                    </Button>
                  ),
              )}
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
