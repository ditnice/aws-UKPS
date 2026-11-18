'use client'

import { useRouter } from 'next/navigation'
import { useState } from 'react'

import { buildRecordActionHref } from '@/app/portal/organisations/[id]/records/_lib/recordActionAlert'
import { submitRecord, SubmissionConflictProblemDetails } from '@/client/generated'
import { Alert } from '@/components/Alert/Alert'
import { Button, ButtonGroup } from '@/components/Button/Button'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { checkAnswersHref } from '@/lib/dynamic-form/links'

type SubmitState =
  | { kind: 'idle' }
  | { kind: 'conflict'; message: string }
  | { kind: 'failed' }
  | { kind: 'invalid' }

export type CheckAnswersActionsProps = {
  recordId: number
  formVersion: string
  revisionHash: string
  organisationId: number
}

export function CheckAnswersActions({
  recordId,
  formVersion,
  revisionHash,
  organisationId,
}: CheckAnswersActionsProps) {
  const [isSubmitting, setIsSubmitting] = useState(false)
  const router = useRouter()
  const [submitState, setSubmitState] = useState<SubmitState>({ kind: 'idle' })

  const submit = async () => {
    setSubmitState({ kind: 'idle' })
    setIsSubmitting(true)

    try {
      const { error, response } = await submitRecord({
        path: { recordId },
        body: { formVersion, revisionHash },
      })

      if (response?.ok) {
        router.push(buildRecordActionHref(organisationId, 'submitted'))
        // Stay disabled while navigating, so a second click cannot submit twice.
        return
      }

      if (response?.status === 422) {
        // The refreshed page shows the problems in its error summary and on the rows.
        router.refresh()
        setSubmitState({ kind: 'invalid' })
      } else if (response?.status === 409) {
        const { detail } = (error ?? {}) as SubmissionConflictProblemDetails
        setSubmitState({
          kind: 'conflict',
          message: detail ?? 'This record has changed. Reload the page and try again.',
        })
      } else {
        setSubmitState({ kind: 'failed' })
      }
    } catch {
      setSubmitState({ kind: 'failed' })
    }

    setIsSubmitting(false)
  }

  return (
    <section>
      <h2>Now submit your record for QA review</h2>
      <p>If your record passes QA review, horizon scanning users will be able to see it.</p>
      <p>
        If your record does not pass QA review, the QA team will let you know what information needs
        to be updated before horizon scanning users can see it. We will email you the outcome either
        way.
      </p>
      {submitState.kind === 'conflict' && (
        <Alert type="caution">
          <p>{submitState.message}</p>
          <p>
            <a href={checkAnswersHref(recordId)}>Reload this page</a> to see the latest answers.
          </p>
        </Alert>
      )}
      {submitState.kind === 'invalid' && (
        <ErrorState>
          There are problems with your answers. Fix the errors listed at the top of this page, then
          submit again.
        </ErrorState>
      )}
      {submitState.kind === 'failed' && (
        <ErrorState>There was a problem submitting your record. Please try again later.</ErrorState>
      )}
      <ButtonGroup>
        <Button disabled={isSubmitting} onClick={() => void submit()} variant="cta">
          Submit record for review
        </Button>
        <Button to={buildRecordActionHref(organisationId, 'draft-saved')} variant="secondary">
          Return to my records
        </Button>
      </ButtonGroup>
    </section>
  )
}
