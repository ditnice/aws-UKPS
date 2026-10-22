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
import { Button } from '@/components/Button/Button'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { buildPayload, fieldName, toFormValues } from '@/lib/dynamic-form/answers'
import { applyServerErrors } from '@/lib/dynamic-form/applyServerErrors'
import { buildZodSchema } from '@/lib/dynamic-form/buildZodSchema'
import type { FormValue } from '@/lib/dynamic-form/types'
import { getFieldErrorMessage } from '@/lib/form/getFieldErrorMessage'
import { isValidationProblemDetails } from '@/lib/responses/typeGuards'

import styles from './DynamicForm.module.scss'
import { questionRegistry } from './questionRegistry'

export const pageHref = (page: Pick<RecordPageDto, 'recordId' | 'revisionId'>, pageId: string) =>
  `/portal/records/${page.recordId}/revisions/${page.revisionId}/${pageId}`

export const conflictCodes = ['revision_not_draft', 'revision_changed', 'form_version_changed']

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
  const recordsHref = `/portal/organisations/${page.organisationId}/records`
  const singleQuestion = questions.length === 1

  const form = useForm({
    defaultValues: toFormValues(questions, page.answers),
    onSubmit: async ({ value, formApi }) => {
      setSaveState({ kind: 'idle' })
      setOtherErrors([])
      const { data, error, response } = await saveRecordFormPage({
        path: { recordId: page.recordId, revisionId: page.revisionId, pageId: page.page.id },
        body: {
          formVersion: page.formVersion,
          revisionVersion: page.revisionVersion,
          answers: buildPayload(questions, value),
        },
      })

      if (response?.ok && data) {
        router.push(data.nextPageId ? pageHref(page, data.nextPageId) : recordsHref)
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

  const heading = singleQuestion ? questions[0].label : page.page.title
  const backLink = page.previousPageId ? (
    <BackLink href={pageHref(page, page.previousPageId)}>Back</BackLink>
  ) : undefined

  return (
    <>
      <PageHeader backLink={backLink} heading={heading} preheading={page.section.title} />

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
        {questions.map((question, index) => {
          const Component = questionRegistry[question.type]
          return (
            <form.Field key={question.id} name={fieldName(index)}>
              {(field) => (
                <div
                  className={clsx(styles.question, singleQuestion && styles.labelAsHeading)}
                  data-question-id={question.id}
                >
                  <Component
                    disabled={page.readOnly}
                    error={getFieldErrorMessage(field.state.meta.errors)}
                    labelAsHeading={singleQuestion}
                    name={field.name}
                    onBlur={field.handleBlur}
                    onChange={(next: FormValue) => field.handleChange(next)}
                    question={question}
                    value={field.state.value}
                  />
                </div>
              )}
            </form.Field>
          )
        })}

        <div className={styles.actions}>
          {!page.readOnly && (
            <form.Subscribe selector={(state) => state.isSubmitting}>
              {(isSubmitting) => (
                <Button disabled={isSubmitting} buttonType="submit" variant="cta">
                  Save and continue
                </Button>
              )}
            </form.Subscribe>
          )}
          {/* TODO: link to the tasklist once it exists. */}
          <Link href="#">Return to tasklist</Link>
        </div>
      </form>
    </>
  )
}
