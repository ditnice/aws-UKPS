import { Alert } from '@/components/Alert/Alert'

const alertContent = {
  archived: {
    heading: 'Record status changed to archived',
    body: 'You have updated the record status to archived. You can view the record at any time but cannot update this record.',
  },
  'placed-on-hold': {
    heading: 'Record placed on hold',
    body: 'You have placed this record on hold. You will receive update reminder emails every 6 months.',
  },
  'hold-removed': {
    heading: 'Record status changed to published',
    body: 'You have updated the record status to published. You will receive update reminder emails every 3 months.',
  },
} satisfies Record<string, { heading: string; body: string }>

/** An action just taken on the record, shown as an alert on the record page. */
export type RecordViewAlertAction = keyof typeof alertContent

export const parseRecordViewAlert = (
  action: string | string[] | undefined,
): RecordViewAlertAction | undefined =>
  typeof action === 'string' && Object.hasOwn(alertContent, action)
    ? (action as RecordViewAlertAction)
    : undefined

export type RecordViewAlertProps = {
  action: RecordViewAlertAction
}

export const RecordViewAlert = ({ action }: RecordViewAlertProps) => {
  const { heading, body } = alertContent[action]
  return (
    <Alert type="success">
      <h2>{heading}</h2>
      <p>{body}</p>
    </Alert>
  )
}
