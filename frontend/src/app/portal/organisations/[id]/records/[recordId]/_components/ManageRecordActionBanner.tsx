import Link from 'next/link'

import type { RecordDto } from '@/client/generated'
import { ActionBanner } from '@/components/ActionBanner/ActionBanner'
import { Button, ButtonGroup } from '@/components/Button/Button'

type ManageRecordActionBannerProps = {
  record: Pick<RecordDto, 'organisationId' | 'recordId' | 'recordType' | 'actions'>
}

export const ManageRecordActionBanner = ({ record }: ManageRecordActionBannerProps) => {
  const { actions } = record
  const discardHref = `/portal/organisations/${record.organisationId}/records/${record.recordId}/discard?recordType=${record.recordType}`

  return (
    <ActionBanner
      variant="subtle"
      title="Manage record"
      headingLevel={2}
      cta={
        <ButtonGroup>
          {actions.includes('Update') && (
            <Button variant="cta" buttonType="button">
              Update record
            </Button>
          )}
          {actions.includes('ChangeStatus') && <Button>Change record status</Button>}
          {actions.includes('ConfirmCurrent') && <Button>Confirm information as current</Button>}
          {actions.includes('Discard') && (
            <Button elementType={Link} href={discardHref}>
              Discard record
            </Button>
          )}
          {actions.includes('ConfirmOnHold') && <Button>Confirm status as current</Button>}
          {actions.includes('ViewTimeline') && (
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
