import { redirect } from 'next/navigation'

import { getAuthValidateSetupToken } from '@/client/generated/sdk.gen'

import { RequestNewLink } from './_components/RequestNewLink'
import { SignUpInitiateError } from './_components/SignUpInitiateError'
import { getSetupTokenErrorContent } from './_lib/setupTokenErrorContent'

type SignUpInitiateProps = {
  searchParams: Promise<{
    setupToken?: string
  }>
}

export default async function SignUpInitiate({ searchParams }: SignUpInitiateProps) {
  const setupToken = (await searchParams).setupToken?.trim()

  if (!setupToken) {
    return <SignUpInitiateError detail="This sign-up link is missing a setup token." />
  }

  let result: Awaited<ReturnType<typeof getAuthValidateSetupToken>>

  try {
    result = await getAuthValidateSetupToken({
      query: { setupToken },
    })
  } catch {
    return <SignUpInitiateError detail="We could not check your sign-up link. Try again later." />
  }

  if (!result.error) {
    redirect(`/auth/sign-up/terms-and-conditions?${new URLSearchParams({ setupToken }).toString()}`)
  }

  if (result.response?.status === 410) {
    return <RequestNewLink setupToken={setupToken} />
  }

  return (
    <SignUpInitiateError {...getSetupTokenErrorContent(result.error, result.response?.status)} />
  )
}
