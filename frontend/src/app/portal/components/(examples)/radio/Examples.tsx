'use client'

import { useState } from 'react'

import { FormGroup } from '@nice-digital/nds-form-group'
import { Radio } from '@nice-digital/nds-radio'

import { Example } from '../../_components/Example'
import styles from '../../page.module.scss'

export function Examples() {
  const [contactPreference, setContactPreference] = useState('email')

  return (
    <>
      <Example title="Interactive inline group">
        <FormGroup
          hint="This changes immediately in the showcase."
          inline
          legend="Preferred contact method"
          name="showcase-contact-preference"
        >
          {['email', 'post', 'telephone'].map((value) => (
            <Radio
              checked={contactPreference === value}
              key={value}
              label={value[0].toUpperCase() + value.slice(1)}
              onChange={() => setContactPreference(value)}
              value={value}
            />
          ))}
        </FormGroup>
        <p aria-live="polite">Selected: {contactPreference}</p>
      </Example>
      <Example title="Hint, disabled and error states">
        <Radio
          hint="Use the address registered to your account."
          label="Post"
          name="showcase-radio-states"
          value="post"
        />
        <Radio disabled label="Fax (not available)" name="showcase-radio-states" value="fax" />
        <Radio
          aria-describedby="showcase-radio-error-message"
          aria-invalid="true"
          error="This option is unavailable."
          label="Other"
          name="showcase-radio-error"
          value="other"
        />
        <span className={styles.visuallyHidden} id="showcase-radio-error-message">
          This option is unavailable.
        </span>
      </Example>
    </>
  )
}
