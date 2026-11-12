import { notFound } from 'next/navigation'

import { getUsersMe } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { errorMessages } from '@/lib/form/errorMessages'

import { findTypedPageProps, PageProps, UserDetail } from './navigation'

const PageWrapper = (props: React.PropsWithChildren & { label: string }) => {
  return (
    <>
      <PageHeader
        backLink={<BackLink href={'/portal/user/me'} />}
        heading={`Edit ${props.label}`}
      />
      {props.children}
    </>
  )
}

export const userDetailLabels: Record<UserDetail, string> = {
  fullName: 'Full name',
  contactNumber: 'Contact number',
  email: 'Email',
}

const Page = async (props: PageProps) => {
  const { params } = await findTypedPageProps(props)
  const { sku } = params

  if (!sku) {
    notFound()
  }

  const client = await createServerApiClient()
  const { data: me, error } = await getUsersMe({ client })
  const label = userDetailLabels[sku]

  if (!me || error) {
    return (
      <PageWrapper label={label}>
        <ErrorState>{errorMessages.failedToRetrieveCurrentUser}</ErrorState>
      </PageWrapper>
    )
  }

  const getForm = () => {
    switch (sku) {
      case 'email':
        return <></>
      case 'fullName':
        return <></>
      case 'contactNumber':
        return <></>
    }
  }

  return (
    <>
      <PageHeader heading="Update details" />
      {getForm()}
    </>
  )
}

export default Page
