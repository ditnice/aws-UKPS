import { describe, expect, it } from 'vitest'

import {
  buildSelectOrganisationHref,
  buildSignInHref,
  getOrganisationLandingHref,
  getOrganisationSelectionHref,
  getSafeReturnTo,
  selectOrganisationPath,
  signInPath,
} from './routing'

describe('auth routing', () => {
  it('returns safe relative returnTo paths', () => {
    expect(getSafeReturnTo('/portal/organisations/1?tab=users')).toBe(
      '/portal/organisations/1?tab=users',
    )
  })

  it('rejects unsafe returnTo values', () => {
    expect(getSafeReturnTo('https://example.com/portal')).toBeUndefined()
    expect(getSafeReturnTo('//example.com/portal')).toBeUndefined()
    expect(getSafeReturnTo('')).toBeUndefined()
  })

  it('builds a sign-in href with an encoded returnTo', () => {
    expect(buildSignInHref('/portal/organisations/1?tab=users')).toBe(
      '/auth/sign-in?returnTo=%2Fportal%2Forganisations%2F1%3Ftab%3Dusers',
    )
  })

  it('omits unsafe returnTo values from sign-in hrefs', () => {
    expect(buildSignInHref('https://example.com/portal')).toBe(signInPath)
  })

  it('builds an organisation selection href with an encoded returnTo', () => {
    expect(buildSelectOrganisationHref('/portal/organisations/2/records?page=2')).toBe(
      '/portal/organisations/select?returnTo=%2Fportal%2Forganisations%2F2%2Frecords%3Fpage%3D2',
    )
  })

  it('omits missing or unsafe returnTo values from organisation selection hrefs', () => {
    expect(buildSelectOrganisationHref(undefined)).toBe(selectOrganisationPath)
    expect(buildSelectOrganisationHref('https://example.com/portal')).toBe(selectOrganisationPath)
  })

  it('does not nest organisation selection hrefs', () => {
    const selectHref = '/portal/organisations/select?returnTo=%2Fportal%2Fuser%2Fme'

    expect(buildSelectOrganisationHref(selectHref)).toBe(selectHref)
  })

  it.each([
    [
      '/portal/organisations/2/records/7?tab=history',
      '/portal/organisations/2/records/7?tab=history',
    ],
    ['/portal/organisations/2', '/portal/organisations/2'],
    ['/portal/user/me', '/portal/user/me'],
  ])('lands on returnTo %s when it is not for another organisation', (returnTo, expected) => {
    expect(getOrganisationLandingHref(2, returnTo)).toBe(expected)
  })

  it.each([
    undefined,
    'https://example.com/portal',
    '/portal/organisations/3/records',
    '/portal/organisations/select?returnTo=%2Fportal',
    '/portal/organisations/switch?organisationId=2',
  ])("lands on the organisation's records when returnTo is %s", (returnTo) => {
    expect(getOrganisationLandingHref(2, returnTo)).toBe('/portal/organisations/2/records')
  })

  it.each(['SelectedOrganisationRequired', 'SelectedOrganisationIsNotValid'])(
    'sends %s to the organisation selection page',
    (code) => {
      expect(getOrganisationSelectionHref({ code }, '/portal/user/me')).toBe(
        '/portal/organisations/select?returnTo=%2Fportal%2Fuser%2Fme',
      )
    },
  )

  it('sends a selected organisation mismatch to the switch page for the requested organisation', () => {
    expect(
      getOrganisationSelectionHref(
        { code: 'SelectedOrganisationMismatch', organisationId: 2 },
        '/portal/organisations/2/records?page=2',
      ),
    ).toBe(
      '/portal/organisations/switch?organisationId=2&returnTo=%2Fportal%2Forganisations%2F2%2Frecords%3Fpage%3D2',
    )
    expect(
      getOrganisationSelectionHref(
        { code: 'SelectedOrganisationMismatch', organisationId: 2 },
        'https://example.com/portal',
      ),
    ).toBe('/portal/organisations/switch?organisationId=2')
  })

  it.each([null, { code: 'MembershipDeactivated' }, { code: 'SelectedOrganisationMismatch' }])(
    'does not require organisation selection for %o',
    (problem) => {
      expect(getOrganisationSelectionHref(problem, '/portal')).toBeUndefined()
    },
  )
})
