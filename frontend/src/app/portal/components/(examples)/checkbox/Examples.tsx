'use client'

import { Checkbox, Checkboxes } from '@/components/Checkboxes/Checkboxes'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <>
      <Example title="Overview">
        <Checkboxes
          legend="How would you like us to contact you?"
          name="contact-example"
          hint="Please select all that apply."
          defaultValue={['email']}
        >
          <Checkbox id="preferred-email" value="email" label="Email" />
          <Checkbox value="telephone" label="Telephone" />
          <Checkbox value="sms" label="Text message" />
          <Checkbox value="post" label="Post" disabled />
        </Checkboxes>
      </Example>
      <Example title="Single checkboxes">
        <Checkbox
          id="accept-terms"
          name="terms-and-conditions"
          value="agree"
          label="I agree to the terms and conditions"
        />
      </Example>
      <Example title="An ordinary none option (not exclusive)">
        <Checkboxes
          legend="Which previous contact methods apply?"
          name="previous-contact"
          hint="None is an ordinary option, not an exclusive selection."
        >
          <Checkbox value="email" label="Email" />
          <Checkbox value="telephone" label="Telephone" />
          <Checkbox value="none" label="None" />
        </Checkboxes>
      </Example>
      <Example title="Inline checkboxes">
        <Checkboxes inline legend="Which updates interest you?" name="updates">
          <Checkbox value="news" label="News" />
          <Checkbox value="events" label="Events" />
        </Checkboxes>
      </Example>
      <Example title="Individual checkbox hint text">
        <Checkbox
          name="newsletter-subscribe"
          value="agree"
          label="Yes, sign me up to the newsletter"
          hint="You can unsubscribe at any time."
        />
      </Example>
      <Example title="Errors">
        <Checkbox
          name="terms-and-conditions-error-demo"
          value="agree"
          label="I accept the terms"
          error
          errorMessage="You must agree to the terms and conditions."
        />
        <Checkboxes
          legend="Choose contact methods"
          name="contact-error"
          error="Please choose at least one contact method."
        >
          <Checkbox value="email" label="Email" />
          <Checkbox value="telephone" label="Telephone" />
        </Checkboxes>
      </Example>
    </>
  )
}
