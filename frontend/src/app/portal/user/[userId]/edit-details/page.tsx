import { isNumber } from 'payload/shared'

import { getUsersMe } from '@/client/generated'
import { BackLinkBrowser } from '@/components/BackLinkBrowser/BackLinkBrowser'
import { ErrorState } from '@/components/Paceholder/ErrorState'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import { EditDetailsForm } from './_components/EditDetailsForm'

export default async function EditDetails({ params }: { params: Promise<{ userId: string }> }) {
  const result = await getUsersMe()
  const me = await result.data
  const { userId } = await params

  const createPageContent = () => {
    if (userId !== 'me') {
      return <ErrorState>Editing another users values is not supported.</ErrorState>
    }

    if (!me) {
      return <ErrorState>Failed to load data for the current user.</ErrorState>
    }

    if (!isNumber(me.userId)) {
      return <ErrorState>Invalid user details.</ErrorState>
    }

    return <EditDetailsForm userId={me.userId} initialValues={{ ...me }} />
  }

  return (
    <>
      <PageHeader backLink={<BackLinkBrowser />} heading="Edit your details" />
      {createPageContent()}
    </>
  )
}
