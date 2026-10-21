import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import type { RecordDisplayStatus, RecordStatus } from '@/client/generated'

import { ManageRecordActionBanner } from './ManageRecordActionBanner'

const renderBanner = (displayStatus: RecordDisplayStatus, recordStatus: RecordStatus) =>
  render(
    <ManageRecordActionBanner
      record={{
        organisationId: 7,
        recordId: 42,
        recordType: 'Medicine',
        displayStatus,
        recordStatus,
      }}
    />,
  )

const actionLabels = () =>
  [...document.querySelectorAll('[data-component="button-group"] > *')].map(
    (element) => element.textContent,
  )

afterEach(cleanup)

describe('ManageRecordActionBanner', () => {
  it.each<[string, RecordDisplayStatus, RecordStatus, string[]]>([
    [
      'published',
      'Published',
      'Published',
      [
        'Update record',
        'Change record status',
        'Confirm information as current',
        'View record timeline',
      ],
    ],
    [
      'an unpublished draft',
      'Draft',
      'Unpublished',
      ['Update record', 'Discard record', 'View record timeline'],
    ],
    [
      'a draft of a published record',
      'Draft',
      'Published',
      ['Update record', 'Change record status', 'View record timeline'],
    ],
    ['in QA review', 'QAReview', 'Unpublished', ['View record timeline']],
    ['on hold', 'OnHold', 'OnHold', ['Change record status', 'View record timeline']],
    ['archived', 'Archived', 'Archived', ['View record timeline']],
  ])(
    'shows the permitted actions for a record that is %s',
    (_, displayStatus, recordStatus, expected) => {
      renderBanner(displayStatus, recordStatus)

      expect(actionLabels()).toEqual(expected)
    },
  )

  it('links discard to the discard confirmation page', () => {
    renderBanner('Draft', 'Unpublished')

    expect(screen.getByRole('link', { name: 'Discard record' }).getAttribute('href')).toBe(
      '/portal/organisations/7/records/42/discard?recordType=Medicine',
    )
  })
})
