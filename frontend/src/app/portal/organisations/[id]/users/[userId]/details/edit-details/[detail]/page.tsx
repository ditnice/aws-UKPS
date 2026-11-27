import { notFound } from 'next/navigation'
import { ReactNode } from 'react'

import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { errorMessages } from '@/lib/form/errorMessages'

import { getUser } from '../../dataRetrieval'

import EditUserDetailForm from './editUserDetailValue/EditUserDetailForm'
import EditUserEmailForm from './editUserEmail/EditUserEmailForm'
import { findTypedPageProps, PageProps, UserDetail } from './navigation'
import PendingEmailUpdateAlert from './PendingEmailUpdateAlert'

const PageWrapper = (
  props: React.PropsWithChildren & { label: string; returnLink: string; header?: ReactNode },
) => {
  return (
    <>
      {props.header}
      <PageHeader
        backLink={<BackLink href={props.returnLink} />}
        heading={`Edit ${props.label.toLocaleLowerCase()}`}
      />
      {props.children}
    </>
  )
}

const setUrlFlag = (relativeLink: string, flag: string): string => {
  const url = new URL(relativeLink, 'http://placeholder.local')
  url.searchParams.set(flag, 'true')
  return url.pathname + url.search + url.hash
}

const userDetailLabels: Record<UserDetail, string> = {
  fullName: 'Full name',
  contactNumber: 'Contact number',
  email: 'Work email address',
}

const Page = async (props: PageProps) => {
  const params = await findTypedPageProps(props)
  const { detail, userId, organisationId } = params

  if (!detail) {
    notFound()
  }

  const onSuccessRedirect = setUrlFlag(params.returnLink, params.onSuccessFlag)

  const { user, error, viewingCurrentUser } = await getUser(organisationId, userId)
  const label = userDetailLabels[detail]

  if (!user || error) {
    return (
      <PageWrapper label={label} returnLink={params.returnLink}>
        <ErrorState>{errorMessages.failedToRetrieveCurrentUser}</ErrorState>
      </PageWrapper>
    )
  }

  const getHeader = () => {
    if (params.detail === 'email' && user.pendingWorkEmail) {
      return (
        <>
          <PendingEmailUpdateAlert
            {...params}
            {...user.pendingWorkEmail}
            viewingCurrentUser={viewingCurrentUser}
          />
        </>
      )
    }
    return undefined
  }

  const getForm = () => {
    switch (detail) {
      case 'fullName':
        return (
          <EditUserDetailForm
            label={label}
            userId={user.userId}
            property="FullName"
            initialValue={user.fullName}
            onSuccessRedirect={onSuccessRedirect}
          />
        )
      case 'email':
        return (
          <EditUserEmailForm
            userId={user.userId}
            initialValue={user.workEmail}
            onSuccessRedirect={onSuccessRedirect}
          />
        )
      case 'contactNumber':
        return (
          <EditUserDetailForm
            label={label}
            userId={user.userId}
            property="WorkPhone"
            initialValue={user.workTelephone}
            onSuccessRedirect={onSuccessRedirect}
          />
        )
    }
  }

  return (
    <>
      <PageWrapper label={label} header={getHeader()} returnLink={params.returnLink}>
        {getForm()}
      </PageWrapper>
    </>
  )
}

export default Page
