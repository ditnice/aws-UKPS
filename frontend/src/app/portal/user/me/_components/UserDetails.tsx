import Link from 'next/link'
import { ReactNode } from 'react'

import { UserInformationDto } from '@/client/generated'
import { Button, ButtonGroup } from '@/components/Button/Button'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { SummaryList, SummaryListRow } from '@/components/SummaryList/SummaryList'
import { errorMessages } from '@/lib/form/errorMessages'

import { buildUpdateUserDetailsHref, UserDetail } from '../edit-details/[sku]/navigation'

export const UserDetails = ({ currentUser }: { currentUser: UserInformationDto | undefined }) => {
  if (!currentUser)
    return (
      <ErrorState data-testid="failed-user-retrieval">
        {errorMessages.failedToRetrieveCurrentUser}
      </ErrorState>
    )

  const details: { label: string; value: ReactNode; changeLinkSku?: UserDetail }[] = [
    { label: 'Organisation', value: currentUser.organisationName },
    { label: 'Full name', value: currentUser.fullName, changeLinkSku: 'fullName' },
    { label: 'Email address', value: currentUser.workEmail, changeLinkSku: 'email' },
    { label: 'Contact Number', value: currentUser.workTelephone, changeLinkSku: 'contactNumber' },
  ]

  return (
    <div>
      <h2>Personal details</h2>
      <SummaryList>
        <>
          {details.map((d) => (
            <SummaryListRow key={d.label} label={d.label} value={d.value}>
              {d.changeLinkSku && (
                <Link href={buildUpdateUserDetailsHref({ sku: d.changeLinkSku })}>Change</Link>
              )}
            </SummaryListRow>
          ))}
        </>
      </SummaryList>
      {/* <ButtonGroup>
        <Button elementType={Link} href={`/portal/user/me/edit-details`} variant="cta">
          Edit Details
        </Button>
      </ButtonGroup> */}
    </div>
  )
}
