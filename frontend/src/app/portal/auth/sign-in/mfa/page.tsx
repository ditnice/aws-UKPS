import { PageHeader } from '@/components/PageHeader/PageHeader'

import { SignInMfaForm } from './_components/SignInMfaForm'

export default function SignInMfa() {
  return (
    <>
      <PageHeader heading="Enter the code from your authenticator app"></PageHeader>

      <SignInMfaForm />
    </>
  )
}
