import { Button } from '@nice-digital/nds-button'
import { PageHeader } from '@nice-digital/nds-page-header'

import { Details } from '@/components/Details/Details'
import { Input } from '@/components/Input/Input'
import { PasswordInput } from '@/components/PasswordInput/PasswordInput'

export default function SignIn() {
  return (
    <>
      <PageHeader heading="Sign-in"></PageHeader>

      <Input width="one-third" label="Email address" name="email-address" />

      <PasswordInput width="one-third" label="Password" name="password" />

      <Details summary="Forgotten your password?">
        If you have forgotten your password visit the{' '}
        <a href="#">account revovery (opens in a new tab)</a> page.
      </Details>

      <Button variant="cta">Continue</Button>
    </>
  )
}
