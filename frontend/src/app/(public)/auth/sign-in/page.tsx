import { Alert } from '@/components/Alert/Alert'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { getSafeReturnTo } from '@/lib/auth/routing'

import { SignInForm } from './_components/SignInForm'

type SignInProps = {
  searchParams: Promise<{
    returnTo?: string
    emailVerified?: string
  }>
}

export default async function SignIn({ searchParams }: SignInProps) {
  const { returnTo: returnToSearchParams, emailVerified } = await searchParams
  const returnTo = getSafeReturnTo(returnToSearchParams)

  return (
    <>
      <PageHeader heading="Sign-in"></PageHeader>
      {emailVerified === String(true) ? (
        <Alert type="success" title="New email address verified">
          <h3>New email address verified</h3>
          <p>
            Your new email address has been verified. To complete the change, sign in using your new
            email address and complete multi-factor authentication.
          </p>
        </Alert>
      ) : undefined}
      <SignInForm returnTo={returnTo} />
    </>
  )
}
