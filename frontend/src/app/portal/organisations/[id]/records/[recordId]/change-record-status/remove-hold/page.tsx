import type { RecordStatusChangeReason } from '@/client/generated'

import OrganisationPageWrapper from '../../../../_components/OrganisationPageWrapper'
import { buildRecordHref } from '../../_lib/recordLinks'
import { StatusChangePage } from '../_components/StatusChangePage'
import {
  StatusChangeReasonForm,
  type StatusChangeReasonOption,
} from '../_components/StatusChangeReasonForm'
import { parseStatusChangeRoute, type StatusChangeRouteProps } from '../_lib/statusChangeRoute'

type RemoveHoldReason = Extract<RecordStatusChangeReason, 'TrialResumed' | 'PlansRefiled' | 'Other'>

const reasonOptions: StatusChangeReasonOption<RemoveHoldReason>[] = [
  {
    reason: 'TrialResumed',
    label: 'Trial has resumed',
    hint: 'One or more clinical trials have restarted',
  },
  {
    reason: 'PlansRefiled',
    label: 'Regulatory plans have been re-filed to MHRA',
    hint: 'A submission to MHRA has been re-filed and development will be continuing',
  },
  {
    reason: 'Other',
    label: 'Other',
    hint: 'Development has restarted for another reason, for example, manufacturing, funding, or strategic considerations have changed',
  },
]

const RemoveHoldPage = async (props: StatusChangeRouteProps) => {
  const { organisationId, recordId, recordType } = await parseStatusChangeRoute(props)

  return (
    <OrganisationPageWrapper organisationId={organisationId}>
      {(organisation) => (
        <StatusChangePage
          organisationId={organisation.id}
          recordId={recordId}
          recordType={recordType}
          action="RemoveHold"
          heading="Reason for removing hold"
          backHref={buildRecordHref(organisation.id, recordId, recordType, 'change-record-status')}
        >
          <p>
            Removing a record from hold means development has restarted and the information in this
            record may now need updating.
          </p>

          <StatusChangeReasonForm
            organisationId={organisation.id}
            recordId={recordId}
            recordType={recordType}
            action="RemoveHold"
            legend="What best describes why development has restarted?"
            missingReasonError="Select why development has restarted"
            options={reasonOptions}
          />
        </StatusChangePage>
      )}
    </OrganisationPageWrapper>
  )
}

export default RemoveHoldPage
