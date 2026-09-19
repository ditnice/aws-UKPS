import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { useRouter } from 'next/navigation'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { updateCurrentOrganisation } from '@/client/generated'

import ManageOrganisationLink from './ManageOrganisationLink'

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

const getLink = () => {
  return screen.getByTestId('action-link')
}

beforeEach(() => {
  vi.clearAllMocks()
})

afterEach(cleanup)

describe('ManageOrganisationLink', () => {
  it('renders a link to the organisation records', () => {
    render(<ManageOrganisationLink organisationId={123} />)

    expect(getLink().getAttribute('href')).toBe('/portal/organisations/123/records')
  })

  it('updates the current organisation and navigates to the records when successful', async () => {
    vi.mocked(updateCurrentOrganisation).mockResolvedValue({
      data: undefined,
      error: undefined,
    })

    render(<ManageOrganisationLink organisationId={123} />)

    fireEvent.click(getLink())

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

    render(<ManageOrganisationLink organisationId={123} />)

    fireEvent.click(getLink())

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
