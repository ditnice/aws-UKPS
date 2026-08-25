import { getUsersMe } from '@/client/generated/sdk.gen'
import { BackLinkBrowser } from '@/components/BackLinkBrowser/BackLinkBrowser'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import { UserDetails } from './_components/UserDetails'

export default async function Me() {
  const result = await getUsersMe()
  const me = await result.data

  return (
    <>
      <PageHeader backLink={<BackLinkBrowser />} heading="Your details" />
      <UserDetails currentUser={me}></UserDetails>
    </>
  )
}
