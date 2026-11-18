import { notFound, redirect } from 'next/navigation'

import { getUsersMe, verifyPendingEmailAddress } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { buildSignInHref } from '@/lib/auth/routing'

type PageProps = {
  searchParams: Promise<{ verificationToken?: string }>
}

const Page = async ({ searchParams }: PageProps) => {
  const { verificationToken } = await searchParams

  if (!verificationToken) {
    notFound()
  }

  const client = await createServerApiClient()
  const { data: me } = await getUsersMe({ client })

  if (!me) {
    notFound()
  }

  const { error } = await verifyPendingEmailAddress({
    client,
    path: { userId: me.userId },
    body: { verificationToken },
  })

  if (error) {
    return (
      <>
        <PageHeader heading={error.title} />
        <ErrorState>{error.detail}</ErrorState>
      </>
    )
  }

  redirect(buildSignInHref(undefined, { emailVerified: true }))
}

export default Page
