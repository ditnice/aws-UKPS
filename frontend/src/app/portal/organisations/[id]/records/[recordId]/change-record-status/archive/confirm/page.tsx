import { Alert } from '@/components/Alert/Alert'

import OrganisationPageWrapper from '../../../../../_components/OrganisationPageWrapper'
import { ConfirmStatusChangeControls } from '../../_components/ConfirmStatusChangeControls'
import { StatusChangePage } from '../../_components/StatusChangePage'
import { buildStatusChangeHref } from '../../_lib/statusChangeActions'
import { parseStatusChangeRoute, type StatusChangeRouteProps } from '../../_lib/statusChangeRoute'

const ArchiveConfirmPage = async (props: StatusChangeRouteProps) => {
  const { organisationId, recordId, recordType } = await parseStatusChangeRoute(props)

  return (
    <OrganisationPageWrapper organisationId={organisationId}>
      {(organisation) => (
        <StatusChangePage
          organisationId={organisation.id}
          recordId={recordId}
          recordType={recordType}
          action="Archive"
          heading="Confirm decision to archive record"
          backHref={buildStatusChangeHref(
            organisation.id,
            recordId,
            recordType,
            'Archive',
            'reason',
          )}
          banner={
            <Alert type="caution">
              <h2>If you continue, this record will be archived</h2>
              <p>
                The record status will change as development has ended and is not expected to
                resume. You cannot undo this and this record cannot be updated in the future.
              </p>
            </Alert>
          }
        >
          <div>
            <p>What happens when this product is archived:</p>
            <ul>
              <li>the product will remain accessible to view by you and horizon scanners</li>
              <li>
                it will be clearly marked as archived and will not be treated as in active
                development
              </li>
              <li>archived records cannot be updated</li>
            </ul>
          </div>

          <ConfirmStatusChangeControls
            organisationId={organisation.id}
            recordId={recordId}
            recordType={recordType}
            action="Archive"
          />
        </StatusChangePage>
      )}
    </OrganisationPageWrapper>
  )
}

export default ArchiveConfirmPage
