import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { router } from '@/test-utils/nextNavigation'

import { RemoveHoldReasons } from './RemoveHoldReasons'

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

let user: ReturnType<typeof userEvent.setup>

beforeEach(() => {
  user = userEvent.setup()
})

const legend = 'What is the reason for removing hold?'

const renderReasons = () =>
  render(<RemoveHoldReasons organisationId={7} recordId={42} recordType="Medicine" />)

describe('RemoveHoldReasons', () => {
  it('asks for the reason as a sub-heading', () => {
    renderReasons()

    expect(screen.getByRole('group', { name: legend })).toBeInTheDocument()
    expect(screen.getByRole('heading', { level: 2, name: legend })).toBeInTheDocument()
  })

  it.each([
    ['TrialResumed', 'Trial has resumed', 'One or more clinical trials have restarted.'],
    [
      'PlansRefiled',
      'Regulatory plans have been re-filed to MHRA',
      'A submission to MHRA has been re-filed and development will be continuing.',
    ],
    [
      'Other',
      'Other',
      'Development has restarted for another reason, for example, manufacturing, funding, or strategic considerations have changed.',
    ],
  ])('offers the %s reason with its hint, unselected', (value, name, hint) => {
    renderReasons()

    const radio = screen.getByRole('radio', { name })
    expect(radio).toHaveAttribute('value', value)
    expect(radio).toHaveAccessibleDescription(hint)
    expect(radio).not.toBeChecked()
  })

  it('shows an error without navigating when Continue is clicked without a reason', async () => {
    renderReasons()

    await user.click(screen.getByRole('button', { name: 'Continue' }))

    expect(screen.getByText('Select a reason for removing the hold')).toBeInTheDocument()
    expect(screen.getByRole('group', { name: legend })).not.toHaveAttribute('aria-invalid')
    expect(router.push).not.toHaveBeenCalled()
  })

  it('clears the error when a reason is selected', async () => {
    renderReasons()
    await user.click(screen.getByRole('button', { name: 'Continue' }))

    await user.click(screen.getByRole('radio', { name: 'Trial has resumed' }))

    expect(screen.getByRole('radio', { name: 'Trial has resumed' })).toBeChecked()
    expect(screen.queryByText('Select a reason for removing the hold')).not.toBeInTheDocument()
    expect(screen.getByRole('group', { name: legend })).not.toHaveAttribute('aria-invalid')
  })

  it('does not navigate or show an error when Continue is clicked with a reason', async () => {
    renderReasons()

    await user.click(screen.getByRole('radio', { name: 'Other' }))
    await user.click(screen.getByRole('button', { name: 'Continue' }))

    expect(screen.queryByText('Select a reason for removing the hold')).not.toBeInTheDocument()
    expect(router.push).not.toHaveBeenCalled()
  })

  it('links Cancel back to the record', () => {
    renderReasons()

    expect(screen.getByRole('link', { name: 'Cancel' })).toHaveAttribute(
      'href',
      '/portal/organisations/7/records/42?recordType=Medicine',
    )
  })
})
