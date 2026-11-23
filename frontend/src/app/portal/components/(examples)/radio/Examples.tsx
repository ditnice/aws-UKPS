'use client'

import { Input } from '@/components/Input/Input'
import { Radio, Radios } from '@/components/Radios/Radios'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <>
      <Example title="Overview">
        <Radios
          legend="Are you happy for us to contact you in the future?"
          name="contact-preference-default"
        >
          <Radio label="Yes" value="yes" />
          <Radio label="No" value="no" />
        </Radios>
      </Example>
      <Example title="Inline radios">
        <Radios
          inline
          legend="Are you happy for us to contact you in the future?"
          name="contact-preference-inline"
        >
          <Radio label="Yes" value="yes" />
          <Radio label="No" value="no" />
        </Radios>
      </Example>
      <Example title="Individual radio hint text">
        <Radios
          legend="Are you happy for us to contact you in the future?"
          name="contact-preference-hint"
        >
          <Radio hint="Some helpful hint text" label="Yes" value="yes" />
          <Radio label="No" value="no" />
        </Radios>
      </Example>
      <Example title="Group hint text">
        <Radios
          hint="Some helpful hint text"
          legend="Are you happy for us to contact you in the future?"
          name="contact-preference-grouphint"
        >
          <Radio label="Yes" value="yes" />
          <Radio label="No" value="no" />
        </Radios>
      </Example>
      <Example title="Conditionally revealing a related question">
        <Radios
          defaultValue="post"
          hint="Select one option"
          legend="How would you like to be contacted?"
          name="contact-method"
        >
          <Radio
            conditional={
              <Input
                label="Email address"
                name="contact-method-email"
                required
                type="email"
                width={20}
              />
            }
            id="contact-email-option"
            label="Email"
            value="email"
          />
          <Radio
            conditional={
              <Input label="Phone number" name="contact-method-phone" type="tel" width={20} />
            }
            label="Phone"
            value="phone"
          />
          <Radio
            conditional={
              <Input label="Mobile phone number" name="contact-method-text" type="tel" width={20} />
            }
            label="Text message"
            value="text"
          />
          <Radio label="Post" value="post" />
        </Radios>
      </Example>
      <Example title="Errors">
        <Radios
          error="Select whether you are happy for us to contact you"
          legend="Are you happy for us to contact you in the future?"
          name="contact-preference-error"
        >
          <Radio label="Yes" value="yes" />
          <Radio label="No" value="no" />
        </Radios>
      </Example>
    </>
  )
}
