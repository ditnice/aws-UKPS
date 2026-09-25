import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { useRouter } from 'next/navigation'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { updateCurrentOrganisation } from '@/client/generated'

import ManageOrganisationLink from './ManageOrganisationLink'

const organisationName = 'organisation-name'

const mocks = vi.hoisted(() => ({
  push: vi.fn(),
}))

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    push: mocks.push,
  }),
}))

vi.mock('@/client/generated', () => ({
  updateCurrentOrganisation: vi.fn(),
}))

const getManageButton = () => {
  return screen.getByTestId('action-link')
}

beforeEach(() => {
  vi.clearAllMocks()
})

afterEach(cleanup)

describe('ManageOrganisationLink', () => {
  it('updates the current organisation and navigates to the records when successful', async () => {
    vi.mocked(updateCurrentOrganisation).mockResolvedValue({
      data: undefined,
      error: undefined,
    })

    render(<ManageOrganisationLink organisationId={123} organisationName={organisationName} />)

    fireEvent.click(getManageButton())

    expect(updateCurrentOrganisation).toHaveBeenCalledWith({
      body: {
        organisationId: 123,
      },
    })

    await waitFor(() => {
      expect(mocks.push).toHaveBeenCalledWith('/portal/organisations/123/records')
    })
  })

  it('navigates to the error page when updating the current organisation is unsuccessful', async () => {
    vi.mocked(updateCurrentOrganisation).mockResolvedValue({
      data: undefined,
      error: { title: 'error' },
    })

    render(<ManageOrganisationLink organisationId={123} organisationName={organisationName} />)

    fireEvent.click(getManageButton())

    expect(updateCurrentOrganisation).toHaveBeenCalledWith({
      body: {
        organisationId: 123,
      },
    })

    await waitFor(() => {
      expect(mocks.push).toHaveBeenCalledWith('?error=organisation-selection')
    })
  })
})
