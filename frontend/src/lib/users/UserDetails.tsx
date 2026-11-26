import Link from 'next/link'
import { ReactNode } from 'react'

import {
  buildUpdateUserDetailsHref,
  Owner,
  UserDetail,
} from '@/app/portal/organisations/[id]/users/[userId]/details/edit-details/[detail]/navigation'
import { UserInformationDto } from '@/client/generated'
import { SummaryList, SummaryListRow } from '@/components/SummaryList/SummaryList'
import { Tag } from '@/components/Tag/Tag'

import styles from './UserDetails.module.scss'

type UserDetailsProps = {
  owner: Owner
  user: UserInformationDto
  navigation: {
    returnLink: string
    onSuccessFlag: Record<UserDetail, string>
  }
}
export const UserDetails = ({ owner, user, navigation }: UserDetailsProps) => {
  const workEmailValue = (
    <div className={styles.workEmailValue}>
      <div>{user.workEmail}</div>
      {user.pendingWorkEmail ? <Tag>Update Pending</Tag> : undefined}
    </div>
  )
  const details: { label: string; value: ReactNode; changeLinkDetailValue?: UserDetail }[] = [
    { label: 'Organisation', value: user.organisationName },
    { label: 'Full name', value: user.fullName, changeLinkDetailValue: 'fullName' },
    { label: 'Email address', value: workEmailValue, changeLinkDetailValue: 'email' },
    {
      label: 'Contact Number',
      value: user.workTelephone,
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
                    userId: user.userId,
                    organisationId: user.organisationId,
                    detail: d.changeLinkDetailValue,
                    returnLink: navigation.returnLink,
                    onSuccessFlag: navigation.onSuccessFlag[d.changeLinkDetailValue],
                    owner,
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
