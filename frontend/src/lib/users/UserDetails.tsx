import Link from 'next/link'
import { ReactNode } from 'react'

import {
  buildUpdateUserDetailsHref,
  UserDetail,
} from '@/app/portal/organisations/[id]/users/[userId]/details/edit-details/[detail]/navigation'
import { UserInformationDto } from '@/client/generated'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { SummaryList, SummaryListRow } from '@/components/SummaryList/SummaryList'
import { Tag } from '@/components/Tag/Tag'
import { errorMessages } from '@/lib/form/errorMessages'

import styles from './UserDetails.module.scss'

type UserDetailsProps = {
  currentUser: UserInformationDto | undefined
  navigation: {
    returnLink: string
    onSuccessFlag: Record<UserDetail, string>
  }
}
export const UserDetails = ({ currentUser, navigation }: UserDetailsProps) => {
  if (!currentUser)
    return (
      <ErrorState data-testid="failed-user-retrieval">
        {errorMessages.failedToRetrieveCurrentUser}
      </ErrorState>
    )

  const workEmailValue = (
    <div className={styles.workEmailValue}>
      <div>{currentUser.workEmail}</div>
      {currentUser.pendingWorkEmail ? <Tag>Update Pending</Tag> : undefined}
    </div>
  )
  const details: { label: string; value: ReactNode; changeLinkDetailValue?: UserDetail }[] = [
    { label: 'Organisation', value: currentUser.organisationName },
    { label: 'Full name', value: currentUser.fullName, changeLinkDetailValue: 'fullName' },
    { label: 'Email address', value: workEmailValue, changeLinkDetailValue: 'email' },
    {
      label: 'Contact Number',
      value: currentUser.workTelephone,
      changeLinkDetailValue: 'contactNumber',
    },
  ]

  return (
    <div>
      <h2>Personal details</h2>
      <SummaryList>
        <>
          {details.map((d) => (
            <SummaryListRow key={d.label} label={d.label} value={d.value}>
              {d.changeLinkDetailValue && (
                <Link
                  href={buildUpdateUserDetailsHref({
                    userId: currentUser.userId,
                    organisationId: currentUser.organisationId,
                    detail: d.changeLinkDetailValue,
                    returnLink: navigation.returnLink,
                    onSuccessFlag: navigation.onSuccessFlag[d.changeLinkDetailValue],
                  })}
                >
                  Change
                </Link>
              )}
            </SummaryListRow>
          ))}
        </>
      </SummaryList>
    </div>
  )
}
