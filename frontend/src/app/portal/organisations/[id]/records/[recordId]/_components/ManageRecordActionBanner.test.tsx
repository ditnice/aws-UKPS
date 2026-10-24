import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import type { RecordDisplayStatus, RecordStatus } from '@/client/generated'

import { ManageRecordActionBanner } from './ManageRecordActionBanner'

const renderBanner = (
  displayStatus: RecordDisplayStatus,
  recordStatus: RecordStatus,
  canDiscard = false,
) =>
  render(
    <ManageRecordActionBanner
      record={{
        organisationId: 7,
        recordId: 42,
        recordType: 'Medicine',
        displayStatus,
        recordStatus,
        canDiscard,
      }}
    />,
  )

const getActionLabels = () =>
  [...document.querySelectorAll('[data-component="button-group"] > *')].map(
    (element) => element.textContent,
  )

afterEach(cleanup)

describe('ManageRecordActionBanner', () => {
  it.each<[string, RecordDisplayStatus, RecordStatus, boolean, string[]]>([
    [
      'published',
      'Published',
      'Published',
      false,
      [
        'Update record',
        'Change record status',
        'Confirm information as current',
        'View record timeline',
      ],
    ],
    [
      'a discardable draft',
      'Draft',
      'Unpublished',
      true,
      ['Update record', 'Discard record', 'View record timeline'],
    ],
    [
      'a draft that has been submitted',
      'Draft',
      'Unpublished',
      false,
      ['Update record', 'View record timeline'],
    ],
    [
      'a draft of a published record',
      'Draft',
      'Published',
      false,
      [
        'Update record',
        'Change record status',
        'Confirm information as current',
        'View record timeline',
      ],
    ],
    ['in QA review', 'QAReview', 'Unpublished', false, ['View record timeline']],
    [
      'on hold',
      'OnHold',
      'OnHold',
      false,
      ['Change record status', 'Confirm status as current', 'View record timeline'],
    ],
    ['archived', 'Archived', 'Archived', false, ['View record timeline']],
  ])(
    'shows the permitted actions for a record that is %s',
    (_, displayStatus, recordStatus, canDiscard, expected) => {
      renderBanner(displayStatus, recordStatus, canDiscard)

      expect(getActionLabels()).toEqual(expected)
    },
  )

  it('links discard to the discard confirmation page', () => {
    renderBanner('Draft', 'Unpublished', true)

    expect(screen.getByRole('link', { name: 'Discard record' }).getAttribute('href')).toBe(
      '/portal/organisations/7/records/42/discard?recordType=Medicine',
    )
  })
})
