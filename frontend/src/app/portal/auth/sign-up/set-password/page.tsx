import { PageHeader } from '@nice-digital/nds-page-header'

import { SignUpSetPasswordForm } from './_components/SignUpSetPasswordForm'

export default function SignUpSetPassword() {
  return (
    <>
      <PageHeader heading="Create a password"></PageHeader>
      <SignUpSetPasswordForm />
    </>
  )
}
