'use client'

import { type FormEvent, useState } from 'react'

import { Checkbox } from '@nice-digital/nds-checkbox'
import { FormGroup } from '@nice-digital/nds-form-group'

import { Example } from '../../_components/Example'
import styles from '../../page.module.scss'

const preventSubmit = (event: FormEvent<HTMLFormElement>) => event.preventDefault()

export function Examples() {
  const [acceptedTerms, setAcceptedTerms] = useState(false)
  const [contactMethods, setContactMethods] = useState<string[]>(['email'])

  const toggleContactMethod = (method: string, checked: boolean) => {
    setContactMethods((current) =>
      checked ? [...current, method] : current.filter((item) => item !== method),
    )
  }

  return (
    <>
      <Example title="Interactive group with hint">
        <form onSubmit={preventSubmit}>
          <FormGroup
            hint="Choose every method that the service may use."
            legend="How may we contact you?"
            name="showcase-contact-method"
          >
            <Checkbox
              checked={contactMethods.includes('email')}
              label="Email"
              name="showcase-contact-method"
              onChange={(event: FormEvent<HTMLInputElement>) =>
                toggleContactMethod('email', event.currentTarget.checked)
              }
              value="email"
            />
            <Checkbox
              checked={contactMethods.includes('post')}
              hint="Letters may take up to 5 working days."
              label="Post"
              name="showcase-contact-method"
              onChange={(event: FormEvent<HTMLInputElement>) =>
                toggleContactMethod('post', event.currentTarget.checked)
              }
              value="post"
            />
            <Checkbox
              disabled
              label="Telephone (not available)"
              name="showcase-contact-method"
              value="telephone"
            />
          </FormGroup>
        </form>
      </Example>
      <Example title="Inline and error states">
        <form onSubmit={preventSubmit}>
          <FormGroup inline legend="Accept the declaration" name="showcase-declaration">
            <Checkbox
              aria-describedby={!acceptedTerms ? 'showcase-declaration-error' : undefined}
              aria-invalid={!acceptedTerms}
              checked={acceptedTerms}
              error={!acceptedTerms && 'Select yes to continue'}
              label="Yes, I accept"
              name="showcase-declaration"
              onChange={(event: FormEvent<HTMLInputElement>) =>
                setAcceptedTerms(event.currentTarget.checked)
              }
              value="yes"
            />
            <Checkbox disabled label="No" name="showcase-declaration" value="no" />
          </FormGroup>
          {!acceptedTerms && (
            <span className={styles.visuallyHidden} id="showcase-declaration-error">
              Select yes to continue.
            </span>
          )}
        </form>
      </Example>
    </>
  )
}
