import OrganisationPageWrapper from '../../../../../_components/OrganisationPageWrapper'
import { ConfirmStatusChangeControls } from '../../_components/ConfirmStatusChangeControls'
import { StatusChangePage } from '../../_components/StatusChangePage'
import { buildStatusChangeHref } from '../../_lib/statusChangeActions'
import { parseStatusChangeRoute, type StatusChangeRouteProps } from '../../_lib/statusChangeRoute'

const RemoveHoldConfirmPage = async (props: StatusChangeRouteProps) => {
  const { organisationId, recordId, recordType } = await parseStatusChangeRoute(props)

  return (
    <OrganisationPageWrapper organisationId={organisationId}>
      {(organisation) => (
        <StatusChangePage
          organisationId={organisation.id}
          recordId={recordId}
          recordType={recordType}
          action="RemoveHold"
          heading="Confirm decision to remove hold"
          backHref={buildStatusChangeHref(
            organisation.id,
            recordId,
            recordType,
            'RemoveHold',
            'reason',
          )}
        >
          <p>
            By removing this record from hold, you confirm that development has restarted and that
            the information you have provided is correct to the best of your knowledge.
          </p>
          <p>
            This record will be visible to horizon scanners as an active development. You may be
            prompted to review and update sections of the record to reflect the current status.
          </p>
          <p>
            You will resume receiving automated update reminders to help keep this record up to
            date.
          </p>

          <ConfirmStatusChangeControls
            organisationId={organisation.id}
            recordId={recordId}
            recordType={recordType}
            action="RemoveHold"
          />
        </StatusChangePage>
      )}
    </OrganisationPageWrapper>
  )
}

export default RemoveHoldConfirmPage
