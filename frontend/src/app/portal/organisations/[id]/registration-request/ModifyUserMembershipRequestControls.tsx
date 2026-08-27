'use client'

import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { useState } from 'react'

import {
  patchOrganisationsByOrganisationIdUsersByUserIdMembershipRequestsApprove,
  patchOrganisationsByOrganisationIdUsersByUserIdMembershipRequestsReject,
} from '@/client/generated'
import { Button, ButtonGroup } from '@/components/Button/Button'

type ModificationAction = 'Approve' | 'Reject'
export type ModifyUserMembershipRequestControlsProps = {
  organisationId: number
  userId: number
  backLink: string
  successLink: string
  action: ModificationAction
}
const ModifyUserMembershipRequestControls = ({
  action,
  organisationId,
  userId,
  backLink,
  successLink,
}: ModifyUserMembershipRequestControlsProps) => {
  const router = useRouter()
  const [error, setError] = useState<boolean>(false)

  const sendRequest = async () => {
    const path = { organisationId, userId }
    switch (action) {
      case 'Approve':
        const approveResponse =
          await patchOrganisationsByOrganisationIdUsersByUserIdMembershipRequestsApprove({
            path,
          })
        return approveResponse.response
      case 'Reject':
        const rejectResponse =
          await patchOrganisationsByOrganisationIdUsersByUserIdMembershipRequestsReject({
            path,
          })
        return rejectResponse.response
    }
  }

  const initiateRequest = async () => {
    setError(false)
    const response = await sendRequest()
    if (response?.ok) {
      router.push(successLink)
      return
    }
    setError(true)
  }

  return (
    <>
      {error && (
        <p data-testid="action-error">An error occurred when trying to approved the user.</p>
      )}
      <ButtonGroup>
        <Button data-testid="action-button" variant="cta" onClick={initiateRequest}>
          {action} user
        </Button>
        <Button data-testid="cancel-button" elementType={Link} href={backLink} variant="secondary">
          Cancel
        </Button>
      </ButtonGroup>
    </>
  )
}
export default ModifyUserMembershipRequestControls
