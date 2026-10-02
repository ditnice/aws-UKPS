import { redirect } from 'next/navigation'

import { getAuthValidateSetupToken } from '@/client/generated/sdk.gen'
import type { ProblemDetails } from '@/client/generated/types.gen'

import { RequestNewLink } from './_components/RequestNewLink'
import { SignUpInitiateError } from './_components/SignUpInitiateError'

type SignUpInitiateProps = {
  searchParams: Promise<{
    setupToken?: string
  }>
}

type ErrorContent = {
  detail: string
  title: string
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

  return <SignUpInitiateError {...getErrorContent(result.error, result.response?.status)} />
}

function getErrorContent(error: ProblemDetails, status?: number): ErrorContent {
  if (status === 409 || status === 404) {
    return {
      title: error.title ?? 'There is a problem with your sign-up link',
      detail:
        error.detail ??
        (status === 404
          ? 'This sign-up link could not be found.'
          : 'This sign-up link has already been used.'),
    }
  }

  if (status === 400) {
    return {
      title: error.title ?? 'There is a problem with your sign-up link',
      detail: error.detail ?? 'This sign-up link is not valid.',
    }
  }

  return {
    title: error.title ?? 'There is a problem with your sign-up link',
    detail: error.detail ?? 'We could not check your sign-up link. Try again later.',
  }
}
