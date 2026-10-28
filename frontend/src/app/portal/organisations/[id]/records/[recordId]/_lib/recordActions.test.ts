import { describe, expect, it } from 'vitest'

import type { RecordDisplayStatus, RecordStatus } from '@/client/generated'

import { canChangeRecordStatus, getRecordActions } from './recordActions'

describe('getRecordActions', () => {
  it.each<[string, RecordDisplayStatus, RecordStatus, boolean, string[]]>([
    [
      'published',
      'Published',
      'Published',
      false,
      ['update', 'changeStatus', 'confirmCurrent', 'viewTimeline'],
    ],
    ['a discardable draft', 'Draft', 'Unpublished', true, ['update', 'discard', 'viewTimeline']],
    ['a draft that has been submitted', 'Draft', 'Unpublished', false, ['update', 'viewTimeline']],
    [
      'a draft of a published record',
      'Draft',
      'Published',
      false,
      ['update', 'changeStatus', 'confirmCurrent', 'viewTimeline'],
    ],
    ['in QA review', 'QAReview', 'Unpublished', false, ['viewTimeline']],
    ['on hold', 'OnHold', 'OnHold', false, ['changeStatus', 'confirmOnHold', 'viewTimeline']],
    ['archived', 'Archived', 'Archived', false, ['viewTimeline']],
  ])(
    'returns the actions for a record that is %s',
    (_, displayStatus, recordStatus, canDiscard, expected) => {
      expect(getRecordActions({ displayStatus, recordStatus, canDiscard })).toEqual(expected)
    },
  )

  it.each<[RecordDisplayStatus, RecordStatus]>([
    ['Draft', 'Published'],
    ['Published', 'Published'],
    ['QAReview', 'Unpublished'],
    ['OnHold', 'OnHold'],
    ['Archived', 'Archived'],
  ])(
    'does not offer discard for a %s record with record status %s',
    (displayStatus, recordStatus) => {
      expect(getRecordActions({ displayStatus, recordStatus, canDiscard: true })).not.toContain(
        'discard',
      )
    },
  )
})

describe('canChangeRecordStatus', () => {
  it.each<[RecordDisplayStatus, RecordStatus, boolean]>([
    ['Published', 'Published', true],
    ['Draft', 'Published', true],
    ['OnHold', 'OnHold', true],
    ['Draft', 'Unpublished', false],
    ['QAReview', 'Unpublished', false],
    ['Archived', 'Archived', false],
  ])(
    'for a %s record with record status %s returns %s',
    (displayStatus, recordStatus, expected) => {
      expect(canChangeRecordStatus({ displayStatus, recordStatus, canDiscard: false })).toBe(
        expected,
      )
    },
  )
})
