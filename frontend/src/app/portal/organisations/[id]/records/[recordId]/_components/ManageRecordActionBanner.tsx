import { ActionBanner } from '@/components/ActionBanner/ActionBanner'
import { Button, ButtonGroup } from '@/components/Button/Button'

export const ManageRecordActionBanner = () => {
  return (
    <ActionBanner
      variant="subtle"
      title="Manage record"
      headingLevel={2}
      cta={
        <ButtonGroup>
          <Button variant="cta" buttonType="button">
            Update record
          </Button>
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
