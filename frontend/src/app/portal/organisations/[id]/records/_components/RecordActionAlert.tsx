import { Alert } from '@/components/Alert/Alert'

import type { RecordAlertAction } from '../_lib/recordActionAlert'

export type RecordActionAlertProps = {
  action: RecordAlertAction
}

export const RecordActionAlert = ({ action }: RecordActionAlertProps) => {
  switch (action) {
    case 'discarded':
      return (
        <Alert type="success">
          <h3>Record successfully discarded</h3>
          <p>
            You have discarded the record. This record will no longer be viewable under my records.
          </p>
        </Alert>
      )
  }
}
