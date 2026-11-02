import { renderToStaticMarkup } from 'react-dom/server'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import PortalDashboard, { dynamic } from './page'

const { mockGetUsersMe, mockCreateServerApiClient, mockRedirect } = vi.hoisted(() => ({
  mockGetUsersMe: vi.fn(),
  mockCreateServerApiClient: vi.fn(),
  mockRedirect: vi.fn(),
}))

vi.mock('@/client/generated', () => ({
  getUsersMe: mockGetUsersMe,
}))

vi.mock('@/client/server-api', () => ({
  createServerApiClient: mockCreateServerApiClient,
}))

vi.mock('next/navigation', () => ({
  redirect: mockRedirect,
}))

vi.mock('@/components/Placeholder/ErrorState', () => ({
  ErrorState: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="error-state">{children}</div>
  ),
}))

const fakeClient = { name: 'server-api-client' }
const errorMessage = 'Unable to load the data for the current user.'

describe('PortalDashboard', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockCreateServerApiClient.mockResolvedValue(fakeClient)
  })

  const render = async () => renderToStaticMarkup(await PortalDashboard())

  it('is rendered dynamically on every request', () => {
    expect(dynamic).toBe('force-dynamic')
  })

  describe('when the current user loads successfully', () => {
    it("redirects to the records page of the user's organisation", async () => {
      mockGetUsersMe.mockResolvedValue({ data: { organisationId: 123 }, error: undefined })

      await render()

      expect(mockRedirect).toHaveBeenCalledTimes(1)
      expect(mockRedirect).toHaveBeenCalledWith('/portal/organisations/123/records')
    })
  })

  describe('when the current user cannot be loaded', () => {
    it('renders the error state when there is no data', async () => {
      mockGetUsersMe.mockResolvedValue({ data: undefined, error: undefined })

      const html = await render()

      expect(html).toContain('data-testid="error-state"')
      expect(html).toContain(errorMessage)
      expect(mockRedirect).not.toHaveBeenCalled()
    })

    it('renders the error state when the API returns an error', async () => {
      mockGetUsersMe.mockResolvedValue({ data: undefined, error: { status: 500 } })

      const html = await render()

      expect(html).toContain(errorMessage)
      expect(mockRedirect).not.toHaveBeenCalled()
    })

    it('renders the error state when both data and an error are present', async () => {
      mockGetUsersMe.mockResolvedValue({
        data: { organisationId: 123 },
        error: { status: 500 },
      })

      const html = await render()

      expect(html).toContain(errorMessage)
      expect(mockRedirect).not.toHaveBeenCalled()
    })

    it('renders the error state when the API returns a null body', async () => {
      mockGetUsersMe.mockResolvedValue({ data: null, error: undefined })

      const html = await render()

      expect(html).toContain(errorMessage)
      expect(mockRedirect).not.toHaveBeenCalled()
    })
  })
})
