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
    case 'submitted':
      return (
        <Alert type="success">
          <h3>Record submitted for QA review</h3>
          <p>
            You have submitted the record for review by the QA team. You cannot make any edits to
            the record whilst it is in review.
          </p>
        </Alert>
      )
  }
}
