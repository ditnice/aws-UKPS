import { renderToStaticMarkup } from 'react-dom/server'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import type { UserOrganisationContext } from '@/components/Layout/Header/Header'

import PortalLayout, { createLinks, metadata } from './layout'

const { mockGetUserOrganisationContext, applicationLayoutSpy } = vi.hoisted(() => ({
  mockGetUserOrganisationContext: vi.fn(),
  applicationLayoutSpy: vi.fn(),
}))

vi.mock('./_lib/userOrganisationContext', () => ({
  getUserOrganisationContext: mockGetUserOrganisationContext,
}))

vi.mock('@/styles/global.scss', () => ({}))

vi.mock('@/styles/fonts', () => ({
  inter: { variable: 'inter-variable' },
  lora: { variable: 'lora-variable' },
}))

vi.mock('@/components/Layout/ApplicationLayout', () => ({
  ApplicationLayout: (props: { children: React.ReactNode; headerExtension: React.ReactNode }) => {
    applicationLayoutSpy(props)
    return (
      <div data-testid="application-layout">
        <div data-testid="header-extension">{props.headerExtension}</div>
        {props.children}
      </div>
    )
  },
}))

vi.mock('@/components/Layout/Header/OrganisationSwitcher/OrganisationSwitcher', () => ({
  OrganisationSwitcher: ({ organisationName }: { organisationName: string }) => (
    <div data-testid="organisation-switcher">{organisationName}</div>
  ),
}))

// Labels contain <br /> elements and trailing whitespace; flatten them for comparison.
const labelText = (label: React.ReactNode) =>
  renderToStaticMarkup(<>{label}</>)
    .replace(/<br\s*\/?>/g, ' ')
    .replace(/\s+/g, ' ')
    .trim()

const defaultOrganisationId = 123

const buildContext = (
  overrides: Partial<UserOrganisationContext> = {},
): UserOrganisationContext => ({
  organisationId: defaultOrganisationId,
  organisationName: 'Acme Pharma',
  userRole: 'Super',
  hasMultipleOrganisations: false,
  ...overrides,
})

describe('metadata', () => {
  it('exposes the portal title and description', () => {
    expect(metadata).toEqual({
      description: 'An essential first step to market access for medicines.',
      title: 'UK PharmaScan Portal',
    })
  })
})

describe('createLinks', () => {
  it('returns no links when there is no user context', () => {
    expect(createLinks(undefined)).toEqual([])
  })

  it('returns all three links for non-Standard users', () => {
    const links = createLinks(buildContext({ userRole: 'Super' }))

    expect(links.map((l) => l.href)).toEqual([
      `/portal/organisations/${defaultOrganisationId}/records`,
      `/portal/organisations/${defaultOrganisationId}`,
      '/portal/user/me',
    ])
    expect(links.map((l) => labelText(l.label))).toEqual([
      'View and manage records',
      'Manage my organisation',
      'Manage my details',
    ])
  })

  it('omits the "Manage my organisation" link for Standard users', () => {
    const links = createLinks(buildContext({ userRole: 'Standard' }))

    expect(links.map((l) => l.href)).toEqual([
      `/portal/organisations/${defaultOrganisationId}/records`,
      '/portal/user/me',
    ])
    expect(links.map((l) => labelText(l.label))).not.toContain('Manage my organisation')
  })

  it('builds hrefs from the organisation id', () => {
    const links = createLinks(buildContext({ organisationId: 999 }))

    expect(links[0].href).toBe('/portal/organisations/999/records')
    expect(links[1].href).toBe('/portal/organisations/999')
  })
})

describe('PortalLayout', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  const render = async (children: React.ReactNode = <p>page content</p>) =>
    renderToStaticMarkup(await PortalLayout({ children }))

  it('renders html with lang="en" and both font variable classes', async () => {
    mockGetUserOrganisationContext.mockResolvedValue(buildContext())

    const html = await render()

    expect(html).toMatch(/^<html/)
    expect(html).toContain('lang="en"')
    expect(html).toContain('class="inter-variable lora-variable"')
  })

  it('renders children inside the ApplicationLayout within the body', async () => {
    mockGetUserOrganisationContext.mockResolvedValue(buildContext())

    const html = await render(<p>page content</p>)

    expect(html).toContain('<body><div data-testid="application-layout">')
    expect(html).toContain('<p>page content</p>')
  })

  it('passes links derived from the user context to ApplicationLayout', async () => {
    mockGetUserOrganisationContext.mockResolvedValue(buildContext({ userRole: 'Super' }))

    await render()

    const { links } = applicationLayoutSpy.mock.calls[0][0]
    expect(links.map((l: { href: string }) => l.href)).toEqual([
      `/portal/organisations/${defaultOrganisationId}/records`,
      `/portal/organisations/${defaultOrganisationId}`,
      '/portal/user/me',
    ])
  })

  it('shows the OrganisationSwitcher when the user has multiple organisations', async () => {
    mockGetUserOrganisationContext.mockResolvedValue(
      buildContext({ hasMultipleOrganisations: true, organisationName: 'Acme Pharma' }),
    )

    const html = await render()

    expect(html).toContain('data-testid="organisation-switcher"')
    expect(html).toContain('Acme Pharma')
  })

  it('does not show the OrganisationSwitcher for a single organisation', async () => {
    mockGetUserOrganisationContext.mockResolvedValue(
      buildContext({ hasMultipleOrganisations: false }),
    )

    const html = await render()

    expect(html).not.toContain('organisation-switcher')
  })

  it('renders without links or a switcher when there is no user context', async () => {
    mockGetUserOrganisationContext.mockResolvedValue(undefined)

    const html = await render()

    expect(applicationLayoutSpy.mock.calls[0][0].links).toEqual([])
    expect(html).not.toContain('organisation-switcher')
    expect(html).toContain('<p>page content</p>')
  })

  it('fetches the user organisation context once per render', async () => {
    mockGetUserOrganisationContext.mockResolvedValue(buildContext())

    await render()

    expect(mockGetUserOrganisationContext).toHaveBeenCalledTimes(1)
  })
})
