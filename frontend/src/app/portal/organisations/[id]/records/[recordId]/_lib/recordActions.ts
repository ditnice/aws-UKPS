import type { RecordDto } from '@/client/generated'

export type RecordAction =
  'update' | 'changeStatus' | 'confirmCurrent' | 'confirmOnHold' | 'discard' | 'viewTimeline'

type RecordActionState = Pick<RecordDto, 'displayStatus' | 'recordStatus' | 'canDiscard'>

/**
 * Returns the actions a user can take on a record, in display order. A draft of a published
 * record can have its status changed. A draft of a record that has never been published can be
 * discarded if the API allows it, which it does only until the record is first submitted.
 */
export const getRecordActions = ({
  displayStatus,
  recordStatus,
  canDiscard,
}: RecordActionState): RecordAction[] => {
  switch (displayStatus) {
    case 'Published':
      return ['update', 'changeStatus', 'confirmCurrent', 'viewTimeline']
    case 'Draft':
      if (recordStatus !== 'Unpublished') {
        return ['update', 'changeStatus', 'confirmCurrent', 'viewTimeline']
      }
      return canDiscard ? ['update', 'discard', 'viewTimeline'] : ['update', 'viewTimeline']
    case 'OnHold':
      return ['changeStatus', 'confirmOnHold', 'viewTimeline']
    case 'QAReview':
    case 'Archived':
    default:
      return ['viewTimeline']
  }
}

export const canChangeRecordStatus = (record: RecordActionState) =>
  getRecordActions(record).includes('changeStatus')
