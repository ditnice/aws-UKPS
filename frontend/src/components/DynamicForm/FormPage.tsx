'use client'

import { revalidateLogic, useForm } from '@tanstack/react-form'
import clsx from 'clsx'
import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { useMemo, useState } from 'react'

import { saveRecordFormPage } from '@/client/generated'
import type { RecordPageDto } from '@/client/generated'
import { Alert } from '@/components/Alert/Alert'
import { BackLink } from '@/components/BackLink/BackLink'
import { Badge } from '@/components/Badge/Badge'
import { Button } from '@/components/Button/Button'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { buildPayload, fieldName, toFormValues } from '@/lib/dynamic-form/answers'
import { applyServerErrors } from '@/lib/dynamic-form/applyServerErrors'
import { buildZodSchema } from '@/lib/dynamic-form/buildZodSchema'
import { pageLayout, shownQuestionIds } from '@/lib/dynamic-form/conditions'
import type { FormValue } from '@/lib/dynamic-form/types'
import { getFieldErrorMessage } from '@/lib/form/getFieldErrorMessage'
import { isValidationProblemDetails } from '@/lib/responses/typeGuards'

import styles from './DynamicForm.module.scss'
import { questionRegistry } from './questionRegistry'

import type { ReactNode } from 'react'

export const pageHref = (page: Pick<RecordPageDto, 'recordId'>, pageId: string) =>
  `/portal/records/${page.recordId}/${pageId}`

export const taskListHref = (recordId: number) => `/portal/records/${recordId}/task-list`

export const conflictCodes = [
  'record_not_editable',
  'concurrent_update',
  'form_version_changed',
  'page_answers_changed',
]

type SaveState = { kind: 'idle' } | { kind: 'conflict'; detail: string } | { kind: 'failed' }

export type FormPageProps = {
  page: RecordPageDto
}

/** Renders a record content form page from its description, and saves it. */
export function FormPage({ page }: FormPageProps) {
  const router = useRouter()
  const { questions } = page
  const [saveState, setSaveState] = useState<SaveState>({ kind: 'idle' })
  const [otherErrors, setOtherErrors] = useState<string[]>([])
  const schema = useMemo(() => buildZodSchema(questions), [questions])
  const tasksHref = taskListHref(page.recordId)
  const layout = useMemo(() => pageLayout(questions), [questions])
  // A page whose other questions are all revealed beneath one question reads as a
  // single-question page.
  const singleQuestion = layout.topLevel.length === 1

  const form = useForm({
    defaultValues: toFormValues(questions, page.answers),
    onSubmitMeta: { returnToTaskList: false },
    onSubmit: async ({ value, formApi, meta }) => {
      setSaveState({ kind: 'idle' })
      setOtherErrors([])
      const { data, error, response } = await saveRecordFormPage({
        path: { recordId: page.recordId, pageId: page.page.id },
        body: {
          formVersion: page.formVersion,
          pageHash: page.pageHash,
          answers: buildPayload(questions, value, shownQuestionIds(questions, value)),
        },
      })

      if (response?.ok && data) {
        // The task list is the root of the journey: each task ends back on it.
        const nextPageId = meta.returnToTaskList ? null : data.nextPageId
        router.push(nextPageId ? pageHref(page, nextPageId) : tasksHref)
        return
      }

      if (isValidationProblemDetails(error)) {
        setOtherErrors(applyServerErrors(formApi, questions, error))
        return
      }

      const code = (error as { code?: unknown } | undefined)?.code
      if (response?.status === 409 && typeof code === 'string' && conflictCodes.includes(code)) {
        setSaveState({ kind: 'conflict', detail: (error as { detail?: string }).detail ?? '' })
        return
      }

      setSaveState({ kind: 'failed' })
    },
    validationLogic: revalidateLogic({ mode: 'submit', modeAfterSubmission: 'blur' }),
    validators: { onDynamic: schema },
  })

  const heading = singleQuestion ? questions[layout.topLevel[0]].label : page.page.title
  const preheading = page.page.commercialInConfidence ? (
    <>
      {page.task.title}
      <Badge className={styles.commercialBadge} colour="purple">
        Commercial in confidence
      </Badge>
    </>
  ) : (
    page.task.title
  )
  const backLink = (
    <BackLink href={page.previousPageId ? pageHref(page, page.previousPageId) : tasksHref}>
      Back
    </BackLink>
  )

  return (
    <>
      <PageHeader backLink={backLink} heading={heading} preheading={preheading} />

      {saveState.kind === 'conflict' && (
        <Alert type="caution">
          <p>{saveState.detail}</p>
          <p>
            <a href={pageHref(page, page.page.id)}>Reload this page</a> to see the latest answers.
          </p>
        </Alert>
      )}
      {otherErrors.map((message) => (
        <ErrorState key={message}>{message}</ErrorState>
      ))}
      {saveState.kind === 'failed' && (
        <ErrorState>There was a problem saving your answers. Please try again later.</ErrorState>
      )}

      <form
        noValidate
        onSubmit={(event) => {
          event.preventDefault()
          event.stopPropagation()
          void form.handleSubmit()
        }}
      >
        {/* Re-render only when which questions are shown changes, not on every keystroke. */}
        <form.Subscribe
          selector={(state) => [...shownQuestionIds(questions, state.values)].join('\n')}
        >
          {(shownIds) => {
            const shown = new Set(shownIds.split('\n'))
            const renderQuestion = (index: number, labelAsHeading: boolean): ReactNode => {
              const question = questions[index]
              if (!shown.has(question.id)) {
                return null
              }
              const Component = questionRegistry[question.type]
              const revealed = layout.nested.get(index)
              return (
                <form.Field key={question.id} name={fieldName(index)}>
                  {(field) => (
                    <div
                      className={clsx(styles.question, labelAsHeading && styles.labelAsHeading)}
                      data-question-id={question.id}
                    >
                      <Component
                        clearError={() => field.setMeta((meta) => ({ ...meta, errorMap: {} }))}
                        error={getFieldErrorMessage(field.state.meta.errors)}
                        labelAsHeading={labelAsHeading}
                        name={field.name}
                        onBlur={field.handleBlur}
                        onChange={(next: FormValue) => field.handleChange(next)}
                        question={question}
                        revealed={
                          revealed &&
                          Object.fromEntries(
                            [...revealed].map(([value, indexes]) => [
                              value,
                              // Mounted only under the chosen option: a question revealed by
                              // several options is nested under each, but one copy keeps its
                              // id and field unique.
                              value === field.state.value
                                ? indexes.map((i) => renderQuestion(i, false))
                                : [],
                            ]),
                          )
                        }
                        value={field.state.value}
                      />
                    </div>
                  )}
                </form.Field>
              )
            }
            return layout.topLevel.map((index) => renderQuestion(index, singleQuestion))
          }}
        </form.Subscribe>

        <div className={styles.actions}>
          <form.Subscribe selector={(state) => state.isSubmitting}>
            {(isSubmitting) => (
              <>
                <Button disabled={isSubmitting} buttonType="submit" variant="cta">
                  {page.nextPageId ? 'Save and continue' : 'Save and return to task list'}
                </Button>
                {page.nextPageId && (
                  <Button
                    disabled={isSubmitting}
                    buttonType="button"
                    onClick={() => void form.handleSubmit({ returnToTaskList: true })}
                    variant="primary"
                  >
                    Save and return to task list
                  </Button>
                )}
              </>
            )}
          </form.Subscribe>
          <Link href={tasksHref}>Return to task list</Link>
        </div>
      </form>
    </>
  )
}
