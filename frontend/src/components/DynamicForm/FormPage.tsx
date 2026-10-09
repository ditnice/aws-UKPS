'use client'

import { revalidateLogic, useForm } from '@tanstack/react-form'
import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { useMemo, useState } from 'react'

import { saveRecordPage } from '@/client/generated'
import type { RecordPageDto } from '@/client/generated'
import { Alert } from '@/components/Alert/Alert'
import { BackLink } from '@/components/BackLink/BackLink'
import { Button } from '@/components/Button/Button'
import { ErrorSummary } from '@/components/ErrorSummary/ErrorSummary'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { buildPayload, toFormValues } from '@/lib/dynamic-form/answers'
import type { FormValues } from '@/lib/dynamic-form/answers'
import { applyServerErrors } from '@/lib/dynamic-form/applyServerErrors'
import { buildZodSchema } from '@/lib/dynamic-form/buildZodSchema'
import { getFieldErrorMessage } from '@/lib/form/getFieldErrorMessage'
import { isValidationProblemDetails } from '@/lib/responses/typeGuards'

import styles from './DynamicForm.module.scss'
import { getQuestionComponent, getQuestionTargetId } from './questionRegistry'

type FormPageProps = {
  recordId: number
  revisionId: number
  page: RecordPageDto
}

type SaveFailure = 'conflict' | 'error' | undefined

export const recordPageHref = (recordId: number, revisionId: number, pageId: string) =>
  `/portal/records/${recordId}/revisions/${revisionId}/${pageId}`

/**
 * Renders one page of a record's content form from its server definition, validates it with
 * the server's rules, and saves every answer on the page.
 */
export function FormPage({ recordId, revisionId, page }: FormPageProps) {
  const router = useRouter()
  const [saveFailure, setSaveFailure] = useState<SaveFailure>()
  const [otherErrors, setOtherErrors] = useState<string[]>([])
  const { questions } = page
  const questionIds = questions.map((question) => question.id)
  const recordsHref = `/portal/organisations/${page.organisationId}/records`
  const backHref = page.previousPageId
    ? recordPageHref(recordId, revisionId, page.previousPageId)
    : recordsHref
  // A single-question page uses the question as its heading.
  const singleQuestion = questions.length === 1 ? questions[0] : undefined

  const schema = useMemo(() => buildZodSchema(questions), [questions])
  const form = useForm({
    defaultValues: toFormValues(questions, page.answers) as FormValues,
    validationLogic: revalidateLogic({ mode: 'submit', modeAfterSubmission: 'blur' }),
    validators: { onDynamic: schema },
    onSubmit: async ({ value, formApi }) => {
      setSaveFailure(undefined)
      setOtherErrors([])

      const { data, error, response } = await saveRecordPage({
        path: { recordId, revisionId, pageId: page.page.id },
        body: {
          formVersion: page.formVersion,
          revisionVersion: page.revisionVersion,
          answers: buildPayload(questions, value),
        },
      })

      if (response?.ok && data) {
        router.push(
          data.nextPageId ? recordPageHref(recordId, revisionId, data.nextPageId) : recordsHref,
        )
        return
      }

      if (response?.status === 400 && isValidationProblemDetails(error)) {
        setOtherErrors(applyServerErrors(formApi, error, questionIds))
        return
      }

      setSaveFailure(response?.status === 409 ? 'conflict' : 'error')
    },
  })

  return (
    <>
      <PageHeader
        backLink={<BackLink href={backHref}>Back</BackLink>}
        heading={singleQuestion?.label ?? page.page.title}
        preheading={page.section.title}
      />

      {page.readOnly && (
        <Alert type="info">You can view this record but you cannot change it.</Alert>
      )}

      {saveFailure === 'conflict' && (
        <Alert type="caution">
          This page has changed since you opened it.{' '}
          <a href={recordPageHref(recordId, revisionId, page.page.id)}>Reload the page</a> to see
          the latest answers.
        </Alert>
      )}
      {saveFailure === 'error' && (
        <ErrorState>There was a problem saving your answers. Please try again.</ErrorState>
      )}

      <form.Subscribe
        selector={(state) => ({
          submissionAttempts: state.submissionAttempts,
          fieldMeta: state.fieldMeta,
        })}
      >
        {({ submissionAttempts, fieldMeta }) => {
          const meta = fieldMeta as Record<string, { errors?: unknown[] } | undefined>
          const fieldErrors = questions.flatMap((question) => {
            const message = getFieldErrorMessage(meta[question.id]?.errors ?? [])
            return message ? [{ targetId: getQuestionTargetId(question), message }] : []
          })
          const errors = [...fieldErrors, ...otherErrors.map((message) => ({ message }))]
          return submissionAttempts > 0 ? (
            <ErrorSummary errors={errors} focusKey={submissionAttempts} />
          ) : null
        }}
      </form.Subscribe>

      <form
        noValidate
        onSubmit={(event) => {
          event.preventDefault()
          event.stopPropagation()
          void form.handleSubmit()
        }}
      >
        {questions.map((question) => {
          const Question = getQuestionComponent(question)
          return (
            <form.Field key={question.id} name={question.id}>
              {(field) => (
                <Question
                  disabled={page.readOnly}
                  error={getFieldErrorMessage(field.state.meta.errors)}
                  labelAsHeading={question === singleQuestion}
                  onBlur={field.handleBlur}
                  onChange={(value) => field.handleChange(value)}
                  question={question}
                  value={field.state.value}
                />
              )}
            </form.Field>
          )
        })}

        <div className={styles.actions}>
          {!page.readOnly && (
            <form.Subscribe selector={(state) => state.isSubmitting}>
              {(isSubmitting) => (
                <Button disabled={isSubmitting} type="submit" variant="cta">
                  Save and continue
                </Button>
              )}
            </form.Subscribe>
          )}
          <Link href={recordsHref}>Return to tasklist</Link>
        </div>
      </form>
    </>
  )
}
