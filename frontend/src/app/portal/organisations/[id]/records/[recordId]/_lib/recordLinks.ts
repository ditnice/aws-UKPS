import type { RecordType } from '@/client/generated'

import type { RecordViewAlertAction } from '../_components/RecordViewAlert'

/**
 * Links to the record page, or a page beneath it at `subPath`. `action` shows its alert on the
 * record page.
 */
export const buildRecordHref = (
  organisationId: number,
  recordId: number,
  recordType: RecordType,
  subPath?: string,
  action?: RecordViewAlertAction,
) =>
  `/portal/organisations/${organisationId}/records/${recordId}${subPath ? `/${subPath}` : ''}?recordType=${recordType}${action ? `&action=${action}` : ''}`
