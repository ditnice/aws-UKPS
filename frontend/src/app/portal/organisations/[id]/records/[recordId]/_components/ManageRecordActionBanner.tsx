import Link from 'next/link'

import { ActionBanner } from '@/components/ActionBanner/ActionBanner'
import { Button, ButtonGroup } from '@/components/Button/Button'

type ManageRecordActionBannerProps = {
  /** The record's content form, or undefined when its type has no form yet. */
  updateRecordHref?: string
}

export const ManageRecordActionBanner = ({ updateRecordHref }: ManageRecordActionBannerProps) => {
  return (
    <ActionBanner
      variant="subtle"
      title="Manage record"
      headingLevel={2}
      cta={
        <ButtonGroup>
          {updateRecordHref && (
            <Button elementType={Link} href={updateRecordHref} variant="cta">
              Update record
            </Button>
          )}
          <Button>Change record status</Button>
          <Button>Confirm information as current</Button>
          <Button variant="secondary">View record timeline</Button>
        </ButtonGroup>
      }
    >
      Update record information, change the status of a record, or confirm the record information is
      up to date. You can also review all changes made to this record since it was created.
    </ActionBanner>
  )
}
