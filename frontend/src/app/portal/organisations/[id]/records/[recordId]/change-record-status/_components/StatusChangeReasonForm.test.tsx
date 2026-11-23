import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { router } from '@/test-utils/nextNavigation'

import { loadReasonAnswers, saveReasonAnswers } from '../_lib/reasonAnswerStorage'
import { statusChangeErrorMessages } from '../_lib/statusChangeActions'

import { StatusChangeReasonForm } from './StatusChangeReasonForm'

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

let user: ReturnType<typeof userEvent.setup>

beforeEach(() => {
  user = userEvent.setup()
})

afterEach(() => {
  sessionStorage.clear()
  vi.restoreAllMocks()
})

const legend = 'Why?'
const missingReasonError = 'Select a reason'
const detailLabel = 'Provide additional information'
const confirmHref =
  '/portal/organisations/7/records/42/change-record-status/place-on-hold/confirm?recordType=Medicine'

const renderReasons = () =>
  render(
    <StatusChangeReasonForm
      organisationId={7}
      recordId={42}
      recordType="Medicine"
      action="PlaceOnHold"
      legend={legend}
      missingReasonError={missingReasonError}
      options={[
        { reason: 'TrialSuspended', label: 'Trial suspended', hint: 'Paused.' },
        { reason: 'Other', label: 'Other', hint: 'Something else.' },
      ]}
    />,
  )

const continueButton = () => screen.getByRole('button', { name: 'Continue' })
const queryDetail = () => screen.queryByRole('textbox', { name: detailLabel })

const chooseOther = async (detail: string) => {
  await user.click(screen.getByRole('radio', { name: 'Other' }))
  if (detail) {
    await user.type(screen.getByRole('textbox', { name: detailLabel }), detail)
  }
}

describe('StatusChangeReasonForm', () => {
  it('asks for the reason as a sub-heading, with each option unselected', () => {
    renderReasons()

    expect(screen.getByRole('group', { name: legend })).toBeInTheDocument()
    expect(screen.getByRole('heading', { level: 2, name: legend })).toBeInTheDocument()
    const radio = screen.getByRole('radio', { name: 'Trial suspended' })
    expect(radio).toHaveAttribute('value', 'TrialSuspended')
    expect(radio).toHaveAccessibleDescription('Paused.')
    expect(radio).not.toBeChecked()
  })

  it('shows an error without continuing when Continue is clicked without a reason', async () => {
    renderReasons()

    await user.click(continueButton())

    expect(screen.getByText(missingReasonError)).toBeInTheDocument()
    expect(router.push).not.toHaveBeenCalled()
  })

  it('clears the error when a reason is selected', async () => {
    renderReasons()
    await user.click(continueButton())

    await user.click(screen.getByRole('radio', { name: 'Other' }))

    expect(screen.getByRole('radio', { name: 'Other' })).toBeChecked()
    expect(screen.queryByText(missingReasonError)).not.toBeInTheDocument()
  })

  it('only reveals the detail box when Other is selected', async () => {
    renderReasons()
    expect(queryDetail()).not.toBeInTheDocument()

    await user.click(screen.getByRole('radio', { name: 'Other' }))
    expect(queryDetail()).toBeInTheDocument()

    await user.click(screen.getByRole('radio', { name: 'Trial suspended' }))
    expect(queryDetail()).not.toBeInTheDocument()
  })

  it('saves the selected reason and continues to the confirmation page', async () => {
    renderReasons()

    await user.click(screen.getByRole('radio', { name: 'Trial suspended' }))
    await user.click(continueButton())

    expect(loadReasonAnswers(42, 'PlaceOnHold')).toEqual({ reason: 'TrialSuspended' })
    expect(router.push).toHaveBeenCalledExactlyOnceWith(confirmHref)
  })

  it('saves the detail given for Other as the note, trimmed', async () => {
    renderReasons()

    await chooseOther('  Rights sold to another company  ')
    await user.click(continueButton())

    expect(loadReasonAnswers(42, 'PlaceOnHold')).toEqual({
      reason: 'Other',
      note: 'Rights sold to another company',
    })
    expect(router.push).toHaveBeenCalledExactlyOnceWith(confirmHref)
  })

  it.each(['', '   '])(
    'asks for the detail without continuing when Other is chosen with %j',
    async (detail) => {
      renderReasons()
      await chooseOther(detail)

      await user.click(continueButton())

      expect(screen.getByText(statusChangeErrorMessages.noteRequired)).toBeInTheDocument()
      expect(loadReasonAnswers(42, 'PlaceOnHold')).toBeUndefined()
      expect(router.push).not.toHaveBeenCalled()
    },
  )

  it('clears the detail error once the user types', async () => {
    renderReasons()
    await chooseOther('')
    await user.click(continueButton())

    await user.type(screen.getByRole('textbox', { name: detailLabel }), 'A')

    expect(screen.queryByText(statusChangeErrorMessages.noteRequired)).not.toBeInTheDocument()
  })

  it('does not save detail typed for Other when another reason is chosen', async () => {
    renderReasons()
    await chooseOther('Some detail')

    await user.click(screen.getByRole('radio', { name: 'Trial suspended' }))
    await user.click(continueButton())

    expect(loadReasonAnswers(42, 'PlaceOnHold')).toEqual({ reason: 'TrialSuspended' })
  })

  it('shows the answers saved earlier', () => {
    saveReasonAnswers(42, 'PlaceOnHold', { reason: 'Other', note: 'Funding paused' })

    renderReasons()

    expect(screen.getByRole('radio', { name: 'Other' })).toBeChecked()
    expect(screen.getByRole('textbox', { name: detailLabel })).toHaveValue('Funding paused')
  })

  it('ignores a saved reason this change does not offer', () => {
    saveReasonAnswers(42, 'PlaceOnHold', { reason: 'TrialResumed' })

    renderReasons()

    for (const radio of screen.getAllByRole('radio')) {
      expect(radio).not.toBeChecked()
    }
  })

  it('shows an error without continuing when the answer cannot be saved', async () => {
    vi.spyOn(Storage.prototype, 'setItem').mockImplementation(() => {
      throw new Error('blocked')
    })
    renderReasons()

    await user.click(screen.getByRole('radio', { name: 'Trial suspended' }))
    await user.click(continueButton())

    expect(screen.getByRole('alert')).toHaveTextContent('There was a problem saving your answer')
    expect(router.push).not.toHaveBeenCalled()
  })

  it('links Cancel back to the record and forgets the saved answers', async () => {
    saveReasonAnswers(42, 'PlaceOnHold', { reason: 'TrialSuspended' })
    renderReasons()

    const cancel = screen.getByRole('link', { name: 'Cancel' })
    expect(cancel).toHaveAttribute('href', '/portal/organisations/7/records/42?recordType=Medicine')
    await user.click(cancel)

    expect(loadReasonAnswers(42, 'PlaceOnHold')).toBeUndefined()
  })
})
