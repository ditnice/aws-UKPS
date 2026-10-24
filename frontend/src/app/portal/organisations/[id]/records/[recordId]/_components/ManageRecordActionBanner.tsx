import Link from 'next/link'

import type { RecordDto } from '@/client/generated'
import { ActionBanner } from '@/components/ActionBanner/ActionBanner'
import { Button, ButtonGroup } from '@/components/Button/Button'

import { getRecordActions } from '../_lib/recordActions'

type ManageRecordActionBannerProps = {
  record: Pick<
    RecordDto,
    'organisationId' | 'recordId' | 'recordType' | 'displayStatus' | 'recordStatus' | 'canDiscard'
  >
}

export const ManageRecordActionBanner = ({ record }: ManageRecordActionBannerProps) => {
  const actions = getRecordActions(record)
  const discardHref = `/portal/organisations/${record.organisationId}/records/${record.recordId}/discard?recordType=${record.recordType}`

  return (
    <ActionBanner
      variant="subtle"
      title="Manage record"
      headingLevel={2}
      cta={
        <ButtonGroup>
          {actions.includes('update') && (
            <Button variant="cta" buttonType="button">
              Update record
            </Button>
          )}
          {actions.includes('changeStatus') && <Button>Change record status</Button>}
          {actions.includes('confirmCurrent') && <Button>Confirm information as current</Button>}
          {actions.includes('discard') && (
            <Button elementType={Link} href={discardHref}>
              Discard record
            </Button>
          )}
          {actions.includes('confirmOnHold') && <Button>Confirm status as current</Button>}
          {actions.includes('viewTimeline') && (
            <Button variant="secondary">View record timeline</Button>
          )}
        </ButtonGroup>
      }
    >
      Update record information, change the status of a record, or confirm the record information is
      up to date. You can also review all changes made to this record since it was created.
    </ActionBanner>
  )
}
