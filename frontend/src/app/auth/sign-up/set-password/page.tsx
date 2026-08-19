import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import { SignUpSetPasswordForm } from './_components/SignUpSetPasswordForm'

type SignUpSetPasswordProps = {
  searchParams: Promise<{
    setupToken?: string
  }>
}

export default async function SignUpSetPassword({ searchParams }: SignUpSetPasswordProps) {
  const setupToken = (await searchParams).setupToken?.trim()

  if (!setupToken) {
    return (
      <>
        <PageHeader heading="There is a problem with your sign-up link"></PageHeader>
        <p>This sign-up link is missing a setup token.</p>
      </>
    )
  }

  const termsHref = `/auth/sign-up/terms-and-conditions?${new URLSearchParams({
    setupToken,
  }).toString()}`

  return (
    <>
      <PageHeader
        heading="Create a password"
        backLink={<BackLink href={termsHref}>Back</BackLink>}
      ></PageHeader>

      <SignUpSetPasswordForm setupToken={setupToken} />
    </>
  )
}
