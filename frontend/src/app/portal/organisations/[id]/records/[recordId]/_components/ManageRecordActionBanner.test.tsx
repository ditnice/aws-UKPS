import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import type { RecordAction } from '@/client/generated'

import { ManageRecordActionBanner } from './ManageRecordActionBanner'

const renderBanner = (actions: RecordAction[]) =>
  render(
    <ManageRecordActionBanner
      record={{
        organisationId: 7,
        recordId: 42,
        recordType: 'Medicine',
        actions,
      }}
    />,
  )

const getActionLabels = () =>
  [...document.querySelectorAll('[data-component="button-group"] > *')].map(
    (element) => element.textContent,
  )

afterEach(cleanup)

describe('ManageRecordActionBanner', () => {
  it.each<[string, RecordAction[], string[]]>([
    [
      'published',
      ['Update', 'ChangeStatus', 'ConfirmCurrent', 'ViewTimeline'],
      [
        'Update record',
        'Change record status',
        'Confirm information as current',
        'View record timeline',
      ],
    ],
    [
      'a discardable draft',
      ['Update', 'Discard', 'ViewTimeline'],
      ['Update record', 'Discard record', 'View record timeline'],
    ],
    [
      'on hold',
      ['ChangeStatus', 'ConfirmOnHold', 'ViewTimeline'],
      ['Change record status', 'Confirm status as current', 'View record timeline'],
    ],
    ['in QA review', ['ViewTimeline'], ['View record timeline']],
  ])('shows the permitted actions for a record that is %s', (_, actions, expected) => {
    renderBanner(actions)

    expect(getActionLabels()).toEqual(expected)
  })

  it('links discard to the discard confirmation page', () => {
    renderBanner(['Update', 'Discard', 'ViewTimeline'])

    expect(screen.getByRole('link', { name: 'Discard record' }).getAttribute('href')).toBe(
      '/portal/organisations/7/records/42/discard?recordType=Medicine',
    )
  })
})
