import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { updateCurrentOrganisation } from '@/client/generated'

import { SelectOrganisationTable } from './SelectOrganisationTable'

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

const organisations = [
  { id: 123, organisationName: 'First organisation' },
  { id: 456, organisationName: 'Second organisation' },
]

const getManageButton = (organisationName: string) =>
  screen.getByRole('button', { name: `Manage Organisation - ${organisationName}` })

beforeEach(() => {
  vi.clearAllMocks()
})

afterEach(cleanup)

describe('SelectOrganisationTable', () => {
  it('renders a row for each organisation', () => {
    render(<SelectOrganisationTable organisations={organisations} />)

    expect(screen.getByText('First organisation')).toBeTruthy()
    expect(screen.getByText('Second organisation')).toBeTruthy()
    expect(screen.getAllByRole('button', { name: /Manage Organisation/ })).toHaveLength(2)
  })

  it('updates the current organisation and navigates to its records when successful', async () => {
    vi.mocked(updateCurrentOrganisation).mockResolvedValue({
      data: undefined,
      error: undefined,
    } as Awaited<ReturnType<typeof updateCurrentOrganisation>>)

    render(<SelectOrganisationTable organisations={organisations} />)

    fireEvent.click(getManageButton('Second organisation'))

    expect(updateCurrentOrganisation).toHaveBeenCalledWith({ body: { organisationId: 456 } })
    await waitFor(() => {
      expect(mocks.push).toHaveBeenCalledWith('/portal/organisations/456/records')
    })
    expect(screen.queryByTestId('organisation-selection-error')).toBeNull()
  })

  it('shows an error and does not navigate when the backend returns an error', async () => {
    vi.mocked(updateCurrentOrganisation).mockResolvedValue({
      data: undefined,
      error: { title: 'error' },
    } as Awaited<ReturnType<typeof updateCurrentOrganisation>>)

    render(<SelectOrganisationTable organisations={organisations} />)

    fireEvent.click(getManageButton('First organisation'))

    expect(await screen.findByTestId('organisation-selection-error')).toBeTruthy()
    expect(mocks.push).not.toHaveBeenCalled()
  })

  it('shows an error when the request fails', async () => {
    vi.mocked(updateCurrentOrganisation).mockRejectedValue(new Error('network error'))

    render(<SelectOrganisationTable organisations={organisations} />)

    fireEvent.click(getManageButton('First organisation'))

    expect(await screen.findByTestId('organisation-selection-error')).toBeTruthy()
    expect(mocks.push).not.toHaveBeenCalled()
  })

  it('disables the manage buttons while a selection is in progress', async () => {
    let resolveRequest: (
      value: Awaited<ReturnType<typeof updateCurrentOrganisation>>,
    ) => void = () => {}
    vi.mocked(updateCurrentOrganisation).mockReturnValue(
      new Promise((resolve) => {
        resolveRequest = resolve
      }) as ReturnType<typeof updateCurrentOrganisation>,
    )

    render(<SelectOrganisationTable organisations={organisations} />)

    fireEvent.click(getManageButton('First organisation'))

    await waitFor(() => {
      expect(getManageButton('First organisation').hasAttribute('disabled')).toBe(true)
      expect(getManageButton('Second organisation').hasAttribute('disabled')).toBe(true)
    })

    resolveRequest({ data: undefined, error: undefined } as Awaited<
      ReturnType<typeof updateCurrentOrganisation>
    >)

    await waitFor(() => {
      expect(getManageButton('First organisation').hasAttribute('disabled')).toBe(false)
    })
  })
})
