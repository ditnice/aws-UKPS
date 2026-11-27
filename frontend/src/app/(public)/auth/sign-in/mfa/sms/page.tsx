import { PageHeader } from '@/components/PageHeader/PageHeader'
import { getSafeReturnTo } from '@/lib/auth/routing'

import { SignInMfaForm } from '../_components/SignInMfaForm'

export default async function SignInSmsMfa({
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
      <PageHeader heading="Check your phone" />
      <p>We have sent you a text message with a security code.</p>
      <SignInMfaForm
        username={username}
        session={session}
        returnTo={getSafeReturnTo(returnTo)}
        method="sms"
      />
    </>
  )
}
