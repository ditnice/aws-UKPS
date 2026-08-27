import { getUsersMe } from '@/client/generated'
import { BackLinkBrowser } from '@/components/BackLinkBrowser/BackLinkBrowser'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { errorMessages } from '@/lib/form/errorMessages'

import { EditDetailsForm } from './_components/EditDetailsForm'

const PageWrapper = (props: React.PropsWithChildren) => {
  return (
    <>
      <PageHeader backLink={<BackLinkBrowser />} heading="Edit your details" />
      {props.children}
    </>
  )
}

export default async function EditDetails({ params }: { params: Promise<{ userId: string }> }) {
  const { userId } = await params

  if (userId !== 'me') {
    return (
      <PageWrapper>
        <ErrorState>{errorMessages.editingAnotherUserIsNotCurrentSupported}</ErrorState>
      </PageWrapper>
    )
  }

  const result = await getUsersMe()

  if (!result.data || result.error) {
    return (
      <PageWrapper>
        <ErrorState>{errorMessages.failedToRetrieveCurrentUser}</ErrorState>
      </PageWrapper>
    )
  }
  const me = result.data
  if (!Number.isInteger(me.userId)) {
    throw Error(`Unexpected user details [UserId:${me.userId}].`)
  }

  return (
    <PageWrapper>
      <EditDetailsForm
        userId={Number(me.userId)}
        initialValues={{
          fullName: me.fullName,
          workEmail: me.workEmail,
          workTelephone: me.workTelephone,
        }}
      />
    </PageWrapper>
  )
}
