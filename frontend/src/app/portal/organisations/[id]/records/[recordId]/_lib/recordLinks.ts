import type { RecordType } from '@/client/generated'

export const buildRecordHref = (organisationId: number, recordId: number, recordType: RecordType) =>
  `/portal/organisations/${organisationId}/records/${recordId}?recordType=${recordType}`
