import { Alert } from '@/components/Alert/Alert'

import { OrganisationAction } from '../_lib/organisationActionsAlert'

export type OrganisationActionAlertProps = {
  organisationAction: OrganisationAction
}
export const OrganisationActionAlert = async (props: OrganisationActionAlertProps) => {
  if (props.organisationAction === 'updated-details')
    return (
      <Alert type="success">
        <h3>Organisation Details Updated</h3>
        <p>The details for the organisations have been successfully updated.</p>
      </Alert>
    )
  return <></>
}
