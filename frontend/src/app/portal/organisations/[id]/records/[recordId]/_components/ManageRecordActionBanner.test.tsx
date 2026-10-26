import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import type { RecordDisplayStatus, RecordStatus } from '@/client/generated'

import { ManageRecordActionBanner } from './ManageRecordActionBanner'

const updateRecordHref = '/portal/records/42/revisions/3/indication'

const renderBanner = (
  displayStatus: RecordDisplayStatus,
  recordStatus: RecordStatus,
  href: string | null = updateRecordHref,
) =>
  render(
    <ManageRecordActionBanner
      record={{
        organisationId: 7,
        recordId: 42,
        recordType: 'Medicine',
        displayStatus,
        recordStatus,
      }}
      updateRecordHref={href ?? undefined}
    />,
  )

const getActionLabels = () =>
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
      [
        'Update record',
        'Change record status',
        'Confirm information as current',
        'View record timeline',
      ],
    ],
    ['in QA review', 'QAReview', 'Unpublished', ['View record timeline']],
    [
      'on hold',
      'OnHold',
      'OnHold',
      ['Change record status', 'Confirm status as current', 'View record timeline'],
    ],
    ['archived', 'Archived', 'Archived', ['View record timeline']],
  ])(
    'shows the permitted actions for a record that is %s',
    (_, displayStatus, recordStatus, expected) => {
      renderBanner(displayStatus, recordStatus)

      expect(getActionLabels()).toEqual(expected)
    },
  )

  it('links Update record to the record content form', () => {
    renderBanner('Draft', 'Unpublished')

    expect(screen.getByRole('link', { name: 'Update record' }).getAttribute('href')).toBe(
      updateRecordHref,
    )
  })

  it('hides Update record when there is no form the user can edit', () => {
    renderBanner('Draft', 'Unpublished', null)

    expect(screen.queryByText('Update record')).toBeNull()
    expect(getActionLabels()).toEqual(['Discard record', 'View record timeline'])
  })

  it('links discard to the discard confirmation page', () => {
    renderBanner('Draft', 'Unpublished')

    expect(screen.getByRole('link', { name: 'Discard record' }).getAttribute('href')).toBe(
      '/portal/organisations/7/records/42/discard?recordType=Medicine',
    )
  })
})
