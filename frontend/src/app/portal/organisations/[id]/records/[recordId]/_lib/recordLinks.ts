import type { RecordType } from '@/client/generated'

export const buildRecordHref = (
  organisationId: number,
  recordId: number,
  recordType: RecordType,
  subPath?: string,
) =>
  `/portal/organisations/${organisationId}/records/${recordId}${subPath ? `/${subPath}` : ''}?recordType=${recordType}`
