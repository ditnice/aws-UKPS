import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import { SignUpSetPasswordForm } from './_components/SignUpSetPasswordForm'

export default function SignUpSetPassword() {
  return (
    <>
      {/* TODO - we need to decide where this is linking to */}
      <PageHeader
        heading="Create a password"
        backLink={<BackLink href="#">Back</BackLink>}
        verticalPadding="top-only"
      ></PageHeader>
      <SignUpSetPasswordForm />
    </>
  )
}
