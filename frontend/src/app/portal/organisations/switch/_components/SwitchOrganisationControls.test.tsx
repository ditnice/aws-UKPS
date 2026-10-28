import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { updateCurrentOrganisation } from '@/client/generated'

import { SwitchOrganisationControls } from './SwitchOrganisationControls'

vi.mock('next/link', async () => ({
  default: (await import('@/test-utils/nextMocks')).NextLinkMock,
}))

vi.mock('@/client/generated', () => ({
  updateCurrentOrganisation: vi.fn(),
}))

type UpdateResult = Awaited<ReturnType<typeof updateCurrentOrganisation>>

const renderControls = () =>
  render(
    <SwitchOrganisationControls
      organisationId={2}
      confirmHref="/portal/organisations/2/records/5"
      cancelHref="/portal/organisations/1/records"
    />,
  )

let user: ReturnType<typeof userEvent.setup>

const assign = vi.fn()

beforeEach(() => {
  user = userEvent.setup()
  vi.stubGlobal('location', { ...globalThis.location, assign })
})

describe('SwitchOrganisationControls', () => {
  it('switches organisation and continues to the requested page', async () => {
    vi.mocked(updateCurrentOrganisation).mockResolvedValue({
      data: undefined,
      error: undefined,
    } as UpdateResult)
    renderControls()

    await user.click(screen.getByRole('button', { name: 'Confirm' }))

    expect(updateCurrentOrganisation).toHaveBeenCalledWith({ body: { organisationId: 2 } })
    await waitFor(() => {
      expect(assign).toHaveBeenCalledWith('/portal/organisations/2/records/5')
    })
  })

  it('links cancel back to the page the user came from', () => {
    renderControls()

    expect(screen.getByRole('link', { name: 'Cancel' })).toHaveAttribute(
      'href',
      '/portal/organisations/1/records',
    )
  })

  it('shows an error and does not navigate when the backend returns an error', async () => {
    vi.mocked(updateCurrentOrganisation).mockResolvedValue({
      data: undefined,
      error: { title: 'error' },
    } as UpdateResult)
    renderControls()

    await user.click(screen.getByRole('button', { name: 'Confirm' }))

    expect(await screen.findByTestId('switch-organisation-error')).toBeInTheDocument()
    expect(assign).not.toHaveBeenCalled()
  })

  it('shows an error and does not navigate when the request fails', async () => {
    vi.mocked(updateCurrentOrganisation).mockRejectedValue(new Error('Network error'))
    renderControls()

    await user.click(screen.getByRole('button', { name: 'Confirm' }))

    expect(await screen.findByTestId('switch-organisation-error')).toBeInTheDocument()
    expect(assign).not.toHaveBeenCalled()
  })
})
