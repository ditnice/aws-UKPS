import type { RecordStatusChangeReason } from '@/client/generated'

import OrganisationPageWrapper from '../../../../_components/OrganisationPageWrapper'
import { buildRecordHref } from '../../_lib/recordLinks'
import { StatusChangePage } from '../_components/StatusChangePage'
import {
  StatusChangeReasonForm,
  type StatusChangeReasonOption,
} from '../_components/StatusChangeReasonForm'
import { parseStatusChangeRoute, type StatusChangeRouteProps } from '../_lib/statusChangeRoute'

type PlaceOnHoldReason = Extract<
  RecordStatusChangeReason,
  'TrialSuspended' | 'FilingWithdrawn' | 'Other'
>

const reasonOptions: StatusChangeReasonOption<PlaceOnHoldReason>[] = [
  {
    reason: 'TrialSuspended',
    label: 'Trial has been suspended',
    hint: 'One or more clinical trials have been temporarily halted',
  },
  {
    reason: 'FilingWithdrawn',
    label: 'Filing has been withdrawn',
    hint: 'A submission to a regulator was withdrawn, but development may continue or be re-submitted',
  },
  {
    reason: 'Other',
    label: 'Other',
    hint: 'Development has paused for another reason, for example, manufacturing, funding, or strategic considerations',
  },
]

const PlaceOnHoldPage = async (props: StatusChangeRouteProps) => {
  const { organisationId, recordId, recordType } = await parseStatusChangeRoute(props)

  return (
    <OrganisationPageWrapper organisationId={organisationId}>
      {(organisation) => (
        <StatusChangePage
          organisationId={organisation.id}
          recordId={recordId}
          recordType={recordType}
          action="PlaceOnHold"
          heading="Reason for putting the record on hold"
          backHref={buildRecordHref(organisation.id, recordId, recordType, 'change-record-status')}
        >
          <p>
            Select the option that best describes the reason development has paused. This
            information helps horizon scanners understand the nature and likely duration of the
            pause.
          </p>

          <StatusChangeReasonForm
            organisationId={organisation.id}
            recordId={recordId}
            recordType={recordType}
            action="PlaceOnHold"
            legend={`Why is the development of this ${recordType === 'Vaccine' ? 'vaccine' : 'medicine'} pausing?`}
            missingReasonError="Select a reason why development has paused"
            options={reasonOptions}
          />
        </StatusChangePage>
      )}
    </OrganisationPageWrapper>
  )
}

export default PlaceOnHoldPage
