import { renderToStaticMarkup } from 'react-dom/server'
import { describe, expect, it } from 'vitest'

import { UserOrganisationContext } from '@/components/Layout/Header/Header'

import { createLinks } from './linkCreation'

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

// Labels contain <br /> elements and trailing whitespace; flatten them for comparison.
const labelText = (label: React.ReactNode) =>
  renderToStaticMarkup(<>{label}</>)
    .replace(/<br\s*\/?>/g, ' ')
    .replace(/\s+/g, ' ')
    .trim()

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
