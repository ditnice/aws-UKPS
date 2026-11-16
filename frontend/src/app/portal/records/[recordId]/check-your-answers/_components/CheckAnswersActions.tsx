'use client'

import { useState } from 'react'

import { Button, ButtonGroup } from '@/components/Button/Button'

export type CheckAnswersActionsProps = {
  /* Submits the record. Resolves when the attempt has finished. */
  onSubmit: () => Promise<void>
  /* Where "Return to my records" goes. */
  returnHref: string
}

export function CheckAnswersActions({ onSubmit, returnHref }: CheckAnswersActionsProps) {
  const [isSubmitting, setIsSubmitting] = useState(false)

  const submit = async () => {
    setIsSubmitting(true)
    try {
      await onSubmit()
    } finally {
      setIsSubmitting(false)
    }
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
      <ButtonGroup>
        <Button disabled={isSubmitting} onClick={() => void submit()} variant="cta">
          Submit record for review
        </Button>
        <Button to={returnHref} variant="secondary">
          Return to my records
        </Button>
      </ButtonGroup>
    </section>
  )
}
