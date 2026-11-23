import { PageHeader } from '@/components/PageHeader/PageHeader'
import { getSafeReturnTo } from '@/lib/auth/routing'

import { SignInMfaForm } from './_components/SignInMfaForm'

export default async function SignInMfa({
  searchParams,
}: {
  searchParams: Promise<{
    username: string
    session: string
    returnTo?: string
  }>
}) {
  const { username, returnTo, session } = await searchParams

  return (
    <>
      <PageHeader heading="Enter the code from your chosen authenticator" />
      <SignInMfaForm username={username} session={session} returnTo={getSafeReturnTo(returnTo)} />
    </>
  )
}
