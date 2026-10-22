import Link from 'next/link'

import { UserInformationDto } from '@/client/generated'
import { Button, ButtonGroup } from '@/components/Button/Button'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { SummaryList, SummaryListRow } from '@/components/SummaryList/SummaryList'
import { errorMessages } from '@/lib/form/errorMessages'

export const UserDetails = ({ currentUser }: { currentUser: UserInformationDto | undefined }) => {
  if (!currentUser)
    return (
      <ErrorState data-testid="failed-user-retrieval">
        {errorMessages.failedToRetrieveCurrentUser}
      </ErrorState>
    )
  return (
    <div>
      <h2>Personal details</h2>
      <SummaryList>
        <SummaryListRow label="Organisation" value={currentUser.organisationName} />
        <SummaryListRow label="Full name" value={currentUser.fullName} />
        <SummaryListRow label="Email address" value={currentUser.workEmail} />
        <SummaryListRow label="Contact Number" value={currentUser.workTelephone} />
      </SummaryList>
      <ButtonGroup>
        <Button elementType={Link} href={`/portal/user/me/edit-details`} variant="cta">
          Edit Details
        </Button>
      </ButtonGroup>
    </div>
  )
}
