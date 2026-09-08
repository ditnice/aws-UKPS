import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { vi } from 'vitest'

import { UserMembershipRequestDto } from '@/client/generated'

import UserMembershipRetrievalWrapper, {
  UserMembershipRetrievalWrapperProps,
} from './UserMembershipRetrievalWrapper'

const { mockGetMembership, notFound } = vi.hoisted(() => ({
  mockGetMembership: vi.fn(),
  notFound: vi.fn(),
}))

vi.mock('next/navigation', () => ({
  notFound,
}))
vi.mock('@/client/generated', () => ({
  getUserMembershipRequest: mockGetMembership,
}))
vi.mock('@/client/server-api', () => ({
  createServerApiClient: vi.fn(),
}))

const testData: UserMembershipRequestDto = { id: 3, workEmail: 'example@email.com' }

afterEach(cleanup)

beforeEach(() => {
  vi.clearAllMocks()
  mockGetMembership.mockResolvedValue({
    data: testData,
  })
})

const renderComponent = async (overrides?: Partial<UserMembershipRetrievalWrapperProps>) => {
  const children = () => <div data-testid="children"></div>
  const defaults: UserMembershipRetrievalWrapperProps = {
    organisationId: 1,
    userId: 2,
    children,
  }
  const props = { ...defaults, ...overrides }
  render(await UserMembershipRetrievalWrapper(props))
}

const assertErrorMessageShown = () => {
  const element = screen.queryByTestId('failure-message')
  expect(element).toBeTruthy()
}

const confirmChildrenNotRendered = () => {
  const element = screen.queryByTestId('children')
  expect(element).toBeFalsy()
}

describe('UserMembershipRetrievalWrapper', () => {
  it('renders child content on success', async () => {
    await renderComponent({
      children: (request) => <div data-testid="data">{JSON.stringify(request)}</div>,
    })
    const content = screen.getByTestId('data')
    expect(content.textContent).toBe(JSON.stringify(testData))
  })
  it('calls the request with the expected arguments', async () => {
    const expectedPath = { userId: 2, organisationId: 4 }
    await renderComponent({
      ...expectedPath,
    })
    expect(mockGetMembership).toHaveBeenCalledExactlyOnceWith({ path: expectedPath })
  })
  it('calls notfound when the response is not found', async () => {
    mockGetMembership.mockResolvedValue({
      error: { status: 404 },
    })
    await renderComponent()
    expect(notFound).toHaveBeenCalledOnce()
    confirmChildrenNotRendered()
  })
  it('shows an error message when return data is undefined', async () => {
    mockGetMembership.mockResolvedValue({
      data: undefined,
    })
    await renderComponent()
    assertErrorMessageShown()
    confirmChildrenNotRendered()
  })
  it('shows an error messages when unrecognised error is returned', async () => {
    mockGetMembership.mockResolvedValue({
      error: { status: 400 },
    })
    await renderComponent()
    assertErrorMessageShown()
    confirmChildrenNotRendered()
  })
})
