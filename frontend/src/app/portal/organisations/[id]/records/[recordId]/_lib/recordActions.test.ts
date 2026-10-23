import { describe, expect, it } from 'vitest'

import type { RecordDisplayStatus, RecordStatus } from '@/client/generated'

import { canDiscardRecord, getRecordActions } from './recordActions'

describe('getRecordActions', () => {
  it.each<[string, RecordDisplayStatus, RecordStatus, string[]]>([
    [
      'published',
      'Published',
      'Published',
      ['update', 'changeStatus', 'confirmCurrent', 'viewTimeline'],
    ],
    ['an unpublished draft', 'Draft', 'Unpublished', ['update', 'discard', 'viewTimeline']],
    [
      'a draft of a published record',
      'Draft',
      'Published',
      ['update', 'changeStatus', 'confirmCurrent', 'viewTimeline'],
    ],
    ['in QA review', 'QAReview', 'Unpublished', ['viewTimeline']],
    ['on hold', 'OnHold', 'OnHold', ['changeStatus', 'confirmOnHold', 'viewTimeline']],
    ['archived', 'Archived', 'Archived', ['viewTimeline']],
  ])('returns the actions for a record that is %s', (_, displayStatus, recordStatus, expected) => {
    expect(getRecordActions({ displayStatus, recordStatus })).toEqual(expected)
  })
})

describe('canDiscardRecord', () => {
  it('allows an unpublished draft to be discarded', () => {
    expect(canDiscardRecord({ displayStatus: 'Draft', recordStatus: 'Unpublished' })).toBe(true)
  })

  it.each<[RecordDisplayStatus, RecordStatus]>([
    ['Draft', 'Published'],
    ['Published', 'Published'],
    ['QAReview', 'Unpublished'],
    ['OnHold', 'OnHold'],
    ['Archived', 'Archived'],
  ])(
    'does not allow a %s record with record status %s to be discarded',
    (displayStatus, recordStatus) => {
      expect(canDiscardRecord({ displayStatus, recordStatus })).toBe(false)
    },
  )
})
