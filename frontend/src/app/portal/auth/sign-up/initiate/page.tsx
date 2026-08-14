import { redirect } from 'next/navigation'

import { getAuthValidateSetupToken } from '@/client/generated/sdk.gen'
import type { ProblemDetails } from '@/client/generated/types.gen'
import { PageHeader } from '@/components/PageHeader/PageHeader'

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
    redirect(
      `/portal/auth/sign-up/terms-and-conditions?${new URLSearchParams({ setupToken }).toString()}`,
    )
  }

  return <SignUpInitiateError {...getErrorContent(result.error, result.response?.status)} />
}

function SignUpInitiateError({
  detail,
  title = 'There is a problem with your sign-up link',
}: Partial<ErrorContent> & Pick<ErrorContent, 'detail'>) {
  return (
    <>
      <PageHeader heading={title}></PageHeader>
      <p>{detail}</p>
    </>
  )
}

function getErrorContent(error: ProblemDetails, status?: number): ErrorContent {
  if (status === 401 || status === 404) {
    return {
      title: error.title ?? 'There is a problem with your sign-up link',
      detail:
        error.detail ??
        (status === 404
          ? 'This sign-up link could not be found.'
          : 'This sign-up link has expired or has already been used.'),
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
