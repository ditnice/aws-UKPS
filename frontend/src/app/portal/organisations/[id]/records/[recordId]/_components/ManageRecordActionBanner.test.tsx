import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { ManageRecordActionBanner } from './ManageRecordActionBanner'

type Record = React.ComponentProps<typeof ManageRecordActionBanner>['record']

const createRecord = (overrides: Partial<Record> = {}): Record => ({
  organisationId: 7,
  recordId: 42,
  recordType: 'Medicine',
  recordStatus: 'Published',
  displayStatus: 'Published',
  actions: [],
  ...overrides,
})

const updateRecordHref = '/portal/records/42/indication'

const renderBanner = (
  overrides: Partial<Record> = {},
  href: string | undefined = updateRecordHref,
) => render(<ManageRecordActionBanner record={createRecord(overrides)} updateRecordHref={href} />)

const buttonNames = [
  'Update record',
  'Change record status',
  'Confirm information as current',
  'Confirm status as current',
  'View record timeline',
]

const expectVisibleButtons = (expected: string[]) => {
  buttonNames.forEach((name) => {
    const button = screen.queryByRole('button', {
      name,
    })

    const link = screen.queryByRole('link', {
      name,
    })

    if (expected.includes(name)) {
      expect(button ?? link).toBeVisible()
    } else {
      expect(button ?? link).not.toBeInTheDocument()
    }
  })
}

describe('ManageRecordActionBanner', () => {
  describe('buttons shown for each record status', () => {
    it.each([
      {
        status: 'Published',
        actions: ['Update', 'ChangeStatus', 'ConfirmCurrent', 'ViewTimeline'],
        expected: [
          'Update record',
          'Change record status',
          'Confirm information as current',
          'View record timeline',
        ],
      },
      {
        status: 'OnHold',
        actions: ['Update', 'ChangeStatus', 'ConfirmOnHold', 'ViewTimeline'],
        expected: ['Change record status', 'Confirm status as current', 'View record timeline'],
      },
      {
        status: 'Archived',
        actions: ['Update', 'ChangeStatus', 'ConfirmCurrent', 'ViewTimeline'],
        expected: ['View record timeline'],
      },
      {
        status: 'QAReview',
        actions: ['Update', 'ChangeStatus', 'ConfirmCurrent', 'ViewTimeline'],
        expected: ['View record timeline'],
      },
    ] as const)(
      'shows the correct buttons when status is $status',
      ({ status, actions, expected }) => {
        renderBanner({
          displayStatus: status,
          actions: [...actions],
        })

        expectVisibleButtons([...expected])
      },
    )
  })
})
