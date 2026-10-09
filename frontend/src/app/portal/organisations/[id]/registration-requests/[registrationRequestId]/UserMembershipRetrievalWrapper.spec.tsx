import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { vi } from 'vitest'

import { RegisterUserConfirmationDto } from '@/client/generated'

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
  getUserRegistrationById: mockGetMembership,
}))
vi.mock('@/client/server-api', () => ({
  createServerApiClient: vi.fn(),
}))

const testData: RegisterUserConfirmationDto = {
  requestGuid: 'e52c7f89-e182-41b2-bbdc-69a0fa9f034d',
  organisationName: 'Test organisation',
  fullName: 'Test user',
  phoneNumber: '07845796823',
  workEmail: 'example@email.com',
}

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
    requestGuid: testData.requestGuid,
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
    const args = { organisationId: 4, requestGuid: testData.requestGuid }
    await renderComponent({
      ...args,
    })
    expect(mockGetMembership).toHaveBeenCalledExactlyOnceWith({
      path: args,
    })
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
