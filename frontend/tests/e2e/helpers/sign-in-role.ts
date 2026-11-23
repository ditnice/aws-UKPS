import { expect, type Page } from '@playwright/test'

import { SignInPage } from '../pages/sign-in.page'

import { type UserRole } from './roles'
import { type RoleCredentials } from './test-environment'
import { generateTotp } from './totp'

import type {
  OrganisationListDto,
  UserInformationDto,
} from '../../../src/client/generated/types.gen'

const selectOrganisationPath = '/portal/organisations/select'
const mfaPath = '/auth/sign-in/mfa'

function isSignedInDestination(url: URL): boolean {
  return (
    url.pathname === selectOrganisationPath ||
    /^\/portal\/organisations\/\d+\/records$/.test(url.pathname)
  )
}

/** Complete the real sign-in flow, including deliberate organisation selection, before the
 * setup project saves cookies. DEV-auth projects do not use this helper. */
export async function signInRole(
  page: Page,
  role: UserRole,
  credentials: RoleCredentials,
  organisationId: string,
) {
  const expectedOrganisationId = Number(organisationId)
  if (!Number.isSafeInteger(expectedOrganisationId) || expectedOrganisationId <= 0) {
    throw new Error(`The ${role} user's E2E organisation ID must be a positive integer.`)
  }

  const signInPage = new SignInPage(page)
  await signInPage.goto()
  await signInPage.signIn(credentials.email, credentials.password)
  await page.waitForURL((url) => url.pathname === mfaPath || isSignedInDestination(url))

  if (new URL(page.url()).pathname === mfaPath) {
    await page.getByLabel('Security code').fill(generateTotp(credentials.totpSecret))
    await page.getByRole('button', { name: 'Continue', exact: true }).click()
    await page.waitForURL(isSignedInDestination)
  }

  const organisationsResponse = await page.request.get('/backend-api/users/me/organisations')
  expect(organisationsResponse.ok(), 'Loading the signed-in user’s organisations').toBe(true)
  const organisations = (await organisationsResponse.json()) as OrganisationListDto[]
  const organisation = organisations.find(({ id }) => id === expectedOrganisationId)
  if (!organisation) {
    throw new Error(`The ${role} user cannot select E2E organisation ${organisationId}.`)
  }

  // A single membership redirects automatically. Multiple memberships require the same explicit
  // selection as a real user (ADR-002/003), not a cookie or a direct mutation from the test.
  if (organisations.length > 1) {
    await expect(
      page.getByRole('heading', { name: 'Select the organisation to manage' }),
    ).toBeVisible()
    await page
      .getByRole('button', {
        name: `Manage Organisation - ${organisation.organisationName}`,
        exact: true,
      })
      .click()
  }

  await expect(page).toHaveURL(`/portal/organisations/${expectedOrganisationId}/records`)

  const currentUserResponse = await page.request.get('/backend-api/users/me')
  expect(currentUserResponse.ok(), 'Loading the signed-in user').toBe(true)
  const currentUser = (await currentUserResponse.json()) as UserInformationDto
  expect(currentUser, 'The saved session must match the role project').toMatchObject({
    workEmail: credentials.email,
    userRole: role,
    organisationId: expectedOrganisationId,
  })
}
