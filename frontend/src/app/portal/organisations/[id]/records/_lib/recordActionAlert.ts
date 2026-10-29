const recordAlertActions = ['discarded'] as const
export type RecordAlertAction = (typeof recordAlertActions)[number]

export type RecordActionSearchParams = {
  action?: string
}

const isRecordAlertAction = (value: string | undefined): value is RecordAlertAction => {
  return recordAlertActions.includes(value as RecordAlertAction)
}

export function parseRecordAction({
  action,
}: RecordActionSearchParams): RecordAlertAction | undefined {
  return isRecordAlertAction(action) ? action : undefined
}

export function buildRecordActionHref(organisationId: number, action: RecordAlertAction): string {
  return `/portal/organisations/${organisationId}/records?action=${action}`
}
