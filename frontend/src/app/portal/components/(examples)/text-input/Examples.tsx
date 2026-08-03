'use client'

import { FormGroup } from '@nice-digital/nds-form-group'

import { Input } from '@/components/Input/Input'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <>
      <Example title="Overview">
        <Input label="First name" name="firstname-example" />
      </Example>
      <Example title="Example: standard input">
        <Input label="First name" name="firstname" />
      </Example>
      <Example title="Example: grouped text inputs">
        <FormGroup legend="What is your address">
          <Input label="Address line 1" name="address-line-1" />
          <Input label="Address line 2" name="address-line-2" />
          <Input label="Town or city" name="address-town" />
          <Input label="County" name="address-county" />
          <Input label="Postcode" name="address-postcode" />
        </FormGroup>
      </Example>
      <Example title="Example: input with a hint">
        <Input label="Age" name="age" hint="Please enter in years" />
      </Example>
      <Example title="Example: Grouped inputs with a hint">
        <FormGroup legend="What is your address" hint="This should be a UK address">
          <Input label="Address line 1" name="address-line-1-hint" />
          <Input label="Address line 2" name="address-line-2-hint" />
          <Input label="Town or city" name="address-town-hint" />
          <Input label="County" name="address-county-hint" />
          <Input label="Postcode" name="address-postcode-hint" />
        </FormGroup>
      </Example>
      <Example title="Example: input with error">
        <Input
          label="Surname"
          name="surname"
          error
          required
          errorMessage="Please enter your surname"
        />
      </Example>
      <Example title="Example: fixed width">
        <Input label="20 characters" name="width-20" width={20} />
        <Input label="10 characters" name="width-10" width={10} />
        <Input label="5 characters" name="width-5" width={5} />
        <Input label="4 characters" name="width-4" width={4} />
        <Input label="3 characters" name="width-3" width={3} />
        <Input label="2 characters" name="width-2" width={2} />
      </Example>
      <Example title="Example: fluid width">
        <Input label="Full width" name="width-full" width="full" />
        <Input label="Three-quarters width" name="width-three-quarters" width="three-quarters" />
        <Input label="Two-thirds width" name="width-two-thirds" width="two-thirds" />
        <Input label="One-half width" name="width-one-half" width="one-half" />
        <Input label="One-third width" name="width-one-third" width="one-third" />
        <Input label="One-quarter width" name="width-one-quarter" width="one-quarter" />
      </Example>
    </>
  )
}
