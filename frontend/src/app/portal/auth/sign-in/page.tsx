import { PageHeader } from '@nice-digital/nds-page-header'

import { SignInForm } from './_components/SignInForm'

export default function SignIn() {
  return (
    <>
      <PageHeader heading="Sign-in"></PageHeader>

      <SignInForm />
    </>
  )
}
