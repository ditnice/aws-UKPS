import { PageHeader } from '@/components/PageHeader/PageHeader'

import { SignInMfaForm } from './_components/SignInMfaForm'

export default async function SignInMfa({
  searchParams,
}: {
  searchParams: Promise<{
    username: string
    session: string
  }>
}) {
  const { username, session } = await searchParams

  return (
    <>
      <PageHeader heading="Enter the code from your authenticator app" />
      <SignInMfaForm username={username} session={session} />
    </>
  )
}
