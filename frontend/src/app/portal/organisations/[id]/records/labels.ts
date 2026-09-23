import { GetRecordsQuerySortValue, RecordStatus } from '@/client/generated'

export const recordStatusLabels: Record<RecordStatus, string> = {
  Unpublished: 'Unpublished',
  Active: 'Active',
  OnHold: 'On Hold',
  Archived: 'Archived',
}

const recordsTableHeaderKeys = [
  'id',
  'development-name',
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
  { key: 'development-name', label: 'Development name', sortColumn: 'DevelopmentName' },
  { key: 'records-title', label: 'Records title', sortColumn: null },
  { key: 'record-status', label: 'Record status', sortColumn: 'RecordStatus' },
  { key: 'next-update', label: 'Next update', sortColumn: 'NextUpdateDue' },
  { key: 'actions', label: 'Action', sortColumn: null },
] as const
