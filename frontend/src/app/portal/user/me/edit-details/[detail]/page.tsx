import { notFound } from 'next/navigation'

import { getUsersMe } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { errorMessages } from '@/lib/form/errorMessages'

import EditUserDetailForm from './editUserDetailValue/EditUserDetailForm'
import EditUserEmailForm from './editUserEmail/EditUserEmailForm'
import { findTypedPageProps, PageProps, UserDetail } from './navigation'

const PageWrapper = (props: React.PropsWithChildren & { label: string }) => {
  return (
    <>
      <PageHeader
        backLink={<BackLink href={'/portal/user/me'} />}
        heading={`Update ${props.label}`}
      />
      {props.children}
    </>
  )
}

const userDetailLabels: Record<UserDetail, string> = {
  fullName: 'Full name',
  contactNumber: 'Contact number',
  email: 'Email address',
}

const Page = async (props: PageProps) => {
  const { params } = await findTypedPageProps(props)
  const { detail } = params

  if (!detail) {
    notFound()
  }

  const client = await createServerApiClient()
  const { data: me, error } = await getUsersMe({ client })
  const label = userDetailLabels[detail]

  if (!me || error) {
    return (
      <PageWrapper label={label}>
        <ErrorState>{errorMessages.failedToRetrieveCurrentUser}</ErrorState>
      </PageWrapper>
    )
  }

  const getForm = () => {
    switch (detail) {
      case 'fullName':
        return (
          <EditUserDetailForm
            label={label}
            userId={me.userId}
            property="FullName"
            initialValue={me.fullName}
          />
        )
      case 'email':
        return <EditUserEmailForm userId={me.userId} initialValue={me.workEmail} />
      case 'contactNumber':
        return (
          <EditUserDetailForm
            label={label}
            userId={me.userId}
            property="WorkPhone"
            initialValue={me.workTelephone}
          />
        )
    }
  }

  return (
    <>
      <PageWrapper label={label}>{getForm()}</PageWrapper>
    </>
  )
}

export default Page
