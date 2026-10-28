import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import type { RecordStatus } from '@/client/generated'
import { router } from '@/test-utils/nextNavigation'

import { ChangeRecordStatusActions } from './ChangeRecordStatusActions'

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

let user: ReturnType<typeof userEvent.setup>

beforeEach(() => {
  user = userEvent.setup()
})

const renderActions = (recordStatus: RecordStatus = 'Published') =>
  render(
    <ChangeRecordStatusActions
      organisationId={7}
      recordId={42}
      recordType="Medicine"
      recordStatus={recordStatus}
    />,
  )

const getRadioLabels = () =>
  screen.getAllByRole('radio').map((radio) => radio.getAttribute('value'))

describe('ChangeRecordStatusActions', () => {
  it.each<[RecordStatus, string[]]>([
    ['Published', ['Archive', 'PlaceOnHold']],
    ['OnHold', ['Archive', 'RemoveHold']],
  ])('offers the actions for a %s record', (recordStatus, expected) => {
    renderActions(recordStatus)

    expect(getRadioLabels()).toEqual(expected)
  })

  it('does not select an action by default', () => {
    renderActions()

    for (const radio of screen.getAllByRole('radio')) {
      expect(radio).not.toBeChecked()
    }
  })

  it('shows a selection error without navigating when Continue is clicked without an action', async () => {
    renderActions()

    await user.click(screen.getByRole('button', { name: 'Continue' }))

    expect(screen.getByText('Select an option')).toBeInTheDocument()
    expect(router.push).not.toHaveBeenCalled()
  })

  it('clears the selection error when an action is selected', async () => {
    renderActions()
    await user.click(screen.getByRole('button', { name: 'Continue' }))

    await user.click(screen.getByRole('radio', { name: 'Archive the record' }))

    expect(screen.queryByText('Select an option')).not.toBeInTheDocument()
    expect(router.push).not.toHaveBeenCalled()
  })

  it.each<[RecordStatus, string, string]>([
    ['Published', 'Archive the record', 'archive'],
    ['Published', 'Place the record on hold', 'place-on-hold'],
    ['OnHold', 'Remove the record hold', 'remove-hold'],
  ])('navigates a %s record to the "%s" page', async (recordStatus, label, path) => {
    renderActions(recordStatus)

    await user.click(screen.getByRole('radio', { name: label }))
    await user.click(screen.getByRole('button', { name: 'Continue' }))

    expect(router.push).toHaveBeenCalledExactlyOnceWith(
      `/portal/organisations/7/records/42/change-record-status/${path}?recordType=Medicine`,
    )
  })

  it('links Cancel to the record view page', () => {
    renderActions()

    expect(screen.getByRole('link', { name: 'Cancel' }).getAttribute('href')).toBe(
      '/portal/organisations/7/records/42?recordType=Medicine',
    )
  })
})
