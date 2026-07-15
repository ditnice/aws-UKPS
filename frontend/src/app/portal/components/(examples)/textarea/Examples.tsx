'use client'

import { type FormEvent, useState } from 'react'

import { Textarea } from '@nice-digital/nds-textarea'

import { Example } from '../../_components/Example'
import styles from '../../page.module.scss'

export function Examples() {
  const [comments, setComments] = useState('')

  return (
    <>
      <Example title="Controlled textarea with hint">
        <Textarea
          hint="Do not include personal or confidential information."
          label="Comments"
          name="showcase-comments"
          onChange={(event: FormEvent<HTMLTextAreaElement>) =>
            setComments(event.currentTarget.value)
          }
          rows={5}
          value={comments}
        />
        <p aria-live="polite">{comments.length} characters entered</p>
      </Example>
      <Example title="Populated, error and disabled states">
        <Textarea
          defaultValue="The implementation date needs to be reviewed."
          label="Review note"
          name="showcase-review-note"
          rows={4}
        />
        <Textarea
          aria-describedby="showcase-reason-error-message"
          aria-invalid="true"
          error
          errorMessage="Explain why the implementation date should change."
          label="Reason for change"
          name="showcase-reason-error"
          rows={4}
        />
        <span className={styles.visuallyHidden} id="showcase-reason-error-message">
          Explain why the implementation date should change.
        </span>
        <Textarea
          disabled
          label="Archived note"
          name="showcase-disabled-textarea"
          value="This note is read only."
        />
      </Example>
    </>
  )
}
