import { PageHeader } from '@/components/PageHeader/PageHeader'

import { SignInForm } from './_components/SignInForm'

export default function SignIn() {
  return (
    <>
      <PageHeader heading="Sign-in" verticalPadding="top-only"></PageHeader>

      <SignInForm />
    </>
  )
}
