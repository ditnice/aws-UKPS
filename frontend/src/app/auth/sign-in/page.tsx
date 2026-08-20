import { PageHeader } from '@/components/PageHeader/PageHeader'

import { getSafeReturnTo } from '../constants'

import { SignInForm } from './_components/SignInForm'

type SignInProps = {
  searchParams: Promise<{
    returnTo?: string
  }>
}

export default async function SignIn({ searchParams }: SignInProps) {
  const returnTo = getSafeReturnTo((await searchParams).returnTo)

  return (
    <>
      <PageHeader heading="Sign-in"></PageHeader>

      <SignInForm returnTo={returnTo} />
    </>
  )
}
