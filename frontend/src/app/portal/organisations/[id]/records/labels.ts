import { RecordStatus } from '@/client/generated'

export const recordStatusLabels: Record<RecordStatus, string> = {
  Unpublished: 'Unpublished',
  Active: 'Active',
  OnHold: 'On Hold',
  Archived: 'Archived',
}
