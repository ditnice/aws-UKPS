import { GetRecordsQuerySortValue, RecordDisplayStatus, UpdateStatus } from '@/client/generated'
import { TagColour } from '@/components/Tag/Tag'

export const recordStatusLabels: Record<RecordDisplayStatus, string> = {
  Draft: 'Draft',
  QAReview: 'QA review',
  Published: 'Published',
  OnHold: 'On Hold',
  Archived: 'Archived',
}

export const recordStatusTagColours: Record<RecordDisplayStatus, TagColour> = {
  Draft: 'blue',
  QAReview: 'yellow',
  Published: 'green',
  OnHold: 'orange',
  Archived: 'grey',
}

export const updateStatusLabels: Record<UpdateStatus, string> = {
  Overdue: 'Overdue',
  NotOverdue: 'Not overdue',
}

const recordsTableHeaderKeys = [
  'id',
  'company-code',
  'records-title',
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
  { key: 'company-code', label: 'Company code', sortColumn: 'CompanyCode' },
  { key: 'records-title', label: 'Records title', sortColumn: null },
  { key: 'record-status', label: 'Record status', sortColumn: 'DisplayStatus' },
  { key: 'next-update', label: 'Next update', sortColumn: 'NextUpdateDue' },
  { key: 'actions', label: 'Action', sortColumn: null },
] as const
