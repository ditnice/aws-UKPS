'use client'

import { FormGroup } from '@nice-digital/nds-form-group'
import { Input } from '@nice-digital/nds-input'

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
    </>
  )
}
