import Link from 'next/link'
import { ReactNode } from 'react'

import { UserInformationDto } from '@/client/generated'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { SummaryList, SummaryListRow } from '@/components/SummaryList/SummaryList'
import { Tag } from '@/components/Tag/Tag'
import { errorMessages } from '@/lib/form/errorMessages'

import { buildUpdateUserDetailsHref, UserDetail } from '../edit-details/[detail]/navigation'

import styles from './UserDetails.module.scss'

export const UserDetails = ({ currentUser }: { currentUser: UserInformationDto | undefined }) => {
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
                <Link href={buildUpdateUserDetailsHref({ detail: d.changeLinkDetailValue })}>
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
