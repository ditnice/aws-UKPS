import { Alert } from '@/components/Alert/Alert'

import OrganisationPageWrapper from '../../../../../_components/OrganisationPageWrapper'
import { ConfirmStatusChangeControls } from '../../_components/ConfirmStatusChangeControls'
import { StatusChangePage } from '../../_components/StatusChangePage'
import { buildStatusChangeHref } from '../../_lib/statusChangeActions'
import { parseStatusChangeRoute, type StatusChangeRouteProps } from '../../_lib/statusChangeRoute'

const PlaceOnHoldConfirmPage = async (props: StatusChangeRouteProps) => {
  const { organisationId, recordId, recordType } = await parseStatusChangeRoute(props)

  return (
    <OrganisationPageWrapper organisationId={organisationId}>
      {(organisation) => (
        <StatusChangePage
          organisationId={organisation.id}
          recordId={recordId}
          recordType={recordType}
          action="PlaceOnHold"
          heading="Confirm decision to place the record on hold"
          backHref={buildStatusChangeHref(
            organisation.id,
            recordId,
            recordType,
            'PlaceOnHold',
            'reason',
          )}
          banner={
            <Alert type="caution">
              <h2>If you continue, this record will be placed on hold</h2>
              <p>
                The record status will change as development has paused and may restart in the
                future. Update requests for this record will be reduced to every 6 months. You can
                update this status at any time.
              </p>
            </Alert>
          }
        >
          <div>
            <p>What happens when this product is on hold:</p>
            <ul>
              <li>the product will remain accessible to view by you and horizon scanners</li>
              <li>
                you will no longer receive automated update notifications every 3 months requesting
                updates or confirmation that the information is still correct
              </li>
              <li>
                you will receive notifications every 6 months asking you to confirm whether the
                record is still on hold
              </li>
            </ul>
          </div>

          <ConfirmStatusChangeControls
            organisationId={organisation.id}
            recordId={recordId}
            recordType={recordType}
            action="PlaceOnHold"
          />
        </StatusChangePage>
      )}
    </OrganisationPageWrapper>
  )
}

export default PlaceOnHoldConfirmPage
