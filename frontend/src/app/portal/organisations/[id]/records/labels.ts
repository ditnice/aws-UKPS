import { GetRecordsQuerySortValue, RecordStatus, UpdateStatus } from '@/client/generated'

export const recordStatusLabels: Record<RecordStatus, string> = {
  Unpublished: 'Unpublished',
  Active: 'Active',
  OnHold: 'On Hold',
  Archived: 'Archived',
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
  { key: 'record-status', label: 'Record status', sortColumn: 'RecordStatus' },
  { key: 'next-update', label: 'Next update', sortColumn: 'NextUpdateDue' },
  { key: 'actions', label: 'Action', sortColumn: null },
] as const
