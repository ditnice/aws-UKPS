import type { RecordDisplayStatus, RecordStatus } from '@/client/generated'

export type RecordAction = 'update' | 'changeStatus' | 'confirmCurrent' | 'discard' | 'viewTimeline'

type RecordStatuses = {
  displayStatus: RecordDisplayStatus
  recordStatus: RecordStatus
}

/**
 * Returns the actions a user can take on a record, in display order. A draft of a record that
 * has never been published can be discarded, whereas a draft of a published record can have its
 * status changed instead.
 */
export const getRecordActions = ({
  displayStatus,
  recordStatus,
}: RecordStatuses): RecordAction[] => {
  switch (displayStatus) {
    case 'Published':
      return ['update', 'changeStatus', 'confirmCurrent', 'viewTimeline']
    case 'Draft':
      return recordStatus === 'Unpublished'
        ? ['update', 'discard', 'viewTimeline']
        : ['update', 'changeStatus', 'viewTimeline']
    case 'OnHold':
      return ['changeStatus', 'viewTimeline']
    case 'QAReview':
    case 'Archived':
    default:
      return ['viewTimeline']
  }
}

export const canDiscardRecord = (record: RecordStatuses) =>
  getRecordActions(record).includes('discard')
