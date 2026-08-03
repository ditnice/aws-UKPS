'use client'

import { PasswordInput } from '@/components/PasswordInput/PasswordInput'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <>
      <Example title="Default">
        <PasswordInput label="Password" name="password" />
      </Example>
      <Example title="With a hint">
        <PasswordInput hint="At least 8 characters" label="Password" name="password-hint" />
      </Example>
      <Example title="With an error">
        <PasswordInput
          error
          errorMessage="Enter your password"
          label="Password"
          name="password-error"
          required
        />
      </Example>
      <Example title="New password (autocomplete override)">
        <PasswordInput autoComplete="new-password" label="New password" name="new-password" />
      </Example>
      <Example title="Fixed width">
        <PasswordInput label="20 characters" name="password-width-20" width={20} />
        <PasswordInput label="10 characters" name="password-width-10" width={10} />
        <PasswordInput label="5 characters" name="password-width-5" width={5} />
        <PasswordInput label="4 characters" name="password-width-4" width={4} />
        <PasswordInput label="3 characters" name="password-width-3" width={3} />
        <PasswordInput label="2 characters" name="password-width-2" width={2} />
      </Example>
      <Example title="Fluid width">
        <PasswordInput label="Full width" name="password-width-full" width="full" />
        <PasswordInput
          label="Three-quarters width"
          name="password-width-three-quarters"
          width="three-quarters"
        />
        <PasswordInput
          label="Two-thirds width"
          name="password-width-two-thirds"
          width="two-thirds"
        />
        <PasswordInput label="One-half width" name="password-width-one-half" width="one-half" />
        <PasswordInput label="One-third width" name="password-width-one-third" width="one-third" />
        <PasswordInput
          label="One-quarter width"
          name="password-width-one-quarter"
          width="one-quarter"
        />
      </Example>
    </>
  )
}
