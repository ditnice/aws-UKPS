'use client'

import { type FormEvent, useState } from 'react'

import { Input } from '@nice-digital/nds-input'

import { Example } from '../../_components/Example'
import styles from '../../page.module.scss'

const preventSubmit = (event: FormEvent<HTMLFormElement>) => event.preventDefault()

export function Examples() {
  const [reference, setReference] = useState('NG123')
  const [email, setEmail] = useState('')

  return (
    <>
      <Example title="Controlled text and email inputs">
        <form onSubmit={preventSubmit}>
          <Input
            hint="For example, NG123"
            label="Guidance reference"
            name="showcase-reference-input"
            onChange={(event: FormEvent<HTMLInputElement>) =>
              setReference(event.currentTarget.value)
            }
            value={reference}
          />
          <Input
            autoComplete="email"
            label="Email address"
            name="showcase-email-input"
            onChange={(event: FormEvent<HTMLInputElement>) => setEmail(event.currentTarget.value)}
            type="email"
            value={email}
          />
        </form>
      </Example>
      <Example title="Error and disabled states">
        <Input
          aria-describedby="showcase-organisation-error-message"
          aria-invalid="true"
          error
          errorMessage="Enter an organisation name."
          label="Organisation name"
          name="showcase-organisation-error"
        />
        <span className={styles.visuallyHidden} id="showcase-organisation-error-message">
          Enter an organisation name.
        </span>
        <Input disabled label="Account number" name="showcase-disabled-input" value="UKPS-2048" />
      </Example>
    </>
  )
}
