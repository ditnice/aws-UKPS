import type { RecordStatusChangeReason } from '@/client/generated'

import OrganisationPageWrapper from '../../../../_components/OrganisationPageWrapper'
import { buildRecordHref } from '../../_lib/recordLinks'
import { StatusChangePage } from '../_components/StatusChangePage'
import {
  StatusChangeReasonForm,
  type StatusChangeReasonOption,
} from '../_components/StatusChangeReasonForm'
import { parseStatusChangeRoute, type StatusChangeRouteProps } from '../_lib/statusChangeRoute'

type ArchiveReason = Extract<RecordStatusChangeReason, 'DevelopmentDiscontinued' | 'Other'>

const reasonOptions: StatusChangeReasonOption<ArchiveReason>[] = [
  {
    reason: 'DevelopmentDiscontinued',
    label: 'Development discontinued',
    hint: 'Development has permanently stopped and is not expected to resume',
  },
  {
    reason: 'Other',
    label: 'Other',
    hint: 'The record is being archived for another reason. For example, transfer of rights or organisational changes',
  },
]

const ArchivePage = async (props: StatusChangeRouteProps) => {
  const { organisationId, recordId, recordType } = await parseStatusChangeRoute(props)

  return (
    <OrganisationPageWrapper organisationId={organisationId}>
      {(organisation) => (
        <StatusChangePage
          organisationId={organisation.id}
          recordId={recordId}
          recordType={recordType}
          action="Archive"
          heading="Reason for archiving"
          backHref={buildRecordHref(organisation.id, recordId, recordType, 'change-record-status')}
        >
          <p>
            Archiving a record means development has ended and will not restart, or you no longer
            own the rights to the product. The record cannot be updated once it has been archived.
          </p>

          <StatusChangeReasonForm
            organisationId={organisation.id}
            recordId={recordId}
            recordType={recordType}
            action="Archive"
            legend="What is the reason for archiving?"
            missingReasonError="Select a reason for archiving this record"
            options={reasonOptions}
          />
        </StatusChangePage>
      )}
    </OrganisationPageWrapper>
  )
}

export default ArchivePage
