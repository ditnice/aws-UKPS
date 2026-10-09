import { GetRecordsQuerySortValue, RecordStatus, UpdateStatus } from '@/client/generated'
import type { RecordListItemDto } from '@/client/generated/types.gen'
import type { TagColour } from '@/components/Tag/Tag'

export const recordStatusLabels: Record<RecordStatus, string> = {
  Unpublished: 'Unpublished',
  Active: 'Active',
  OnHold: 'On Hold',
  Archived: 'Archived',
}

export const recordStatusTagColours: Record<RecordStatus, TagColour> = {
  Unpublished: 'blue',
  Active: 'green',
  OnHold: 'orange',
  Archived: 'grey',
}

export const workflowStatusLabels: Record<RecordListItemDto['workflowStatus'], string> = {
  Draft: 'Draft',
  InReview: 'QA review',
  Published: 'Published',
  Rejected: 'Draft',
}

export const workflowStatusTagColours: Record<RecordListItemDto['workflowStatus'], TagColour> = {
  Draft: 'blue',
  InReview: 'yellow',
  Published: 'green',
  Rejected: 'blue',
}

export const updateStatusLabels: Record<UpdateStatus, string> = {
  Overdue: 'Overdue',
  NotOverdue: 'Not overdue',
}

const recordsTableHeaderKeys = [
  'id',
  'development-name',
  'record-title',
  'record-status',
  'next-update',
  'actions',
] as const
export type RecordsTableHeaderKey = (typeof recordsTableHeaderKeys)[number]
export type RecordsTableHeader = {
  key: RecordsTableHeaderKey
  label: string
  sortColumn: GetRecordsQuerySortValue | null
}
export const organisationRecordsTableHeaders: RecordsTableHeader[] = [
  { key: 'id', label: 'ID', sortColumn: 'Id' },
  { key: 'development-name', label: 'Development name', sortColumn: 'DevelopmentName' },
  { key: 'record-title', label: 'Record title', sortColumn: 'RecordTitle' },
  { key: 'record-status', label: 'Record status', sortColumn: 'RecordStatus' },
  { key: 'next-update', label: 'Next update', sortColumn: 'NextUpdateDue' },
  { key: 'actions', label: 'Action', sortColumn: null },
] as const
