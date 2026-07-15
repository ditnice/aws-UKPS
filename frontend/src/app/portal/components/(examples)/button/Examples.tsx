'use client'

import { useState } from 'react'

import { Button } from '@nice-digital/nds-button'

import { Example } from '../../_components/Example'
import styles from '../../page.module.scss'

import type { FormEvent } from 'react'

export function Examples() {
  const [variantStatus, setVariantStatus] = useState('Choose an enabled button.')
  const [inverseStatus, setInverseStatus] = useState('Choose the inverse button.')
  const [elementStatus, setElementStatus] = useState('Choose a button type or follow the link.')

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setElementStatus('The form was submitted.')
  }

  return (
    <>
      <Example title="Variants">
        <div className={styles.inlineExamples}>
          <Button onClick={() => setVariantStatus('Primary button selected.')}>Primary</Button>
          <Button onClick={() => setVariantStatus('Call to action button selected.')} variant="cta">
            Call to action
          </Button>
          <Button
            onClick={() => setVariantStatus('Secondary button selected.')}
            variant="secondary"
          >
            Secondary
          </Button>
          <Button disabled>Disabled</Button>
        </div>
        <p aria-live="polite">
          <small>{variantStatus}</small>
        </p>
      </Example>
      <Example dark title="Inverse">
        <Button onClick={() => setInverseStatus('Inverse button selected.')} variant="inverse">
          Inverse button
        </Button>
        <p aria-live="polite">
          <small>{inverseStatus}</small>
        </p>
      </Example>
      <Example title="Button, submit, reset and link elements">
        <form
          className={styles.inlineExamples}
          onReset={() => setElementStatus('The form was reset.')}
          onSubmit={handleSubmit}
        >
          <Button
            buttonType="button"
            onClick={() => setElementStatus('The button element was selected.')}
          >
            Button type
          </Button>
          <Button buttonType="submit" variant="cta">
            Submit type
          </Button>
          <Button buttonType="reset" variant="secondary">
            Reset type
          </Button>
          <Button to="/portal/components/tag">View tag examples</Button>
        </form>
        <p aria-live="polite">
          <small>{elementStatus}</small>
        </p>
      </Example>
    </>
  )
}
