import { getUsersMe } from '@/client/generated'
import { BackLinkBrowser } from '@/components/BackLinkBrowser/BackLinkBrowser'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { errorMessages } from '@/lib/form/errorMessages'

import { EditDetailsForm } from './_components/EditDetailsForm'

export default async function EditDetails({ params }: { params: Promise<{ userId: string }> }) {
  const result = await getUsersMe()
  const me = await result.data
  const { userId } = await params

  const createPageContent = () => {
    if (userId !== 'me') {
      return <ErrorState>{errorMessages.editingAnotherUserIsNotCurrentSupported}</ErrorState>
    }

    if (!me) {
      return <ErrorState>{errorMessages.failedToRetrieveCurrentUser}</ErrorState>
    }

    if (!Number.isInteger(me.userId)) {
      throw Error(`Unexpected user details [UserId:${me.userId}].`)
    }

    return (
      <EditDetailsForm
        userId={Number(me.userId)}
        initialValues={{
          fullName: me.fullName,
          workEmail: me.workEmail,
          workTelephone: me.workTelephone,
        }}
      />
    )
  }

  return (
    <>
      <PageHeader backLink={<BackLinkBrowser />} heading="Edit your details" />
      {createPageContent()}
    </>
  )
}
