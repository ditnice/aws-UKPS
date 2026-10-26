import { type Browser, type Page } from '@playwright/test'

import { expect, test } from '../fixtures/test'
import { uniqueEmail } from '../helpers/test-data'
import {
  isEphemeralStack,
  mutatesDataTag,
  requireEnvironmentVariable,
} from '../helpers/test-environment'
import { OrganisationUsersPage } from '../pages/organisation-users.page'

// Nothing seeds registration requests, so each test submits its own through the public form.
async function requestAccess(page: Page, organisationId: string): Promise<string> {
  const workEmail = uniqueEmail('request')
  await page.goto('/register/provide-details')
  const organisation = page.getByLabel('Select the organisation you are requesting access for')
  // Selected by ID, as other tests rename the organisation.
  await expect(organisation.locator(`option[value="${organisationId}"]`)).toBeAttached()
  await organisation.selectOption(organisationId)
  await page.getByLabel('Full name').fill('Playwright Requester')
  await page.getByLabel('Work email address').fill(workEmail)
  await page.getByLabel('Phone number').fill('07123456789')
  await page.getByRole('button', { name: 'Submit request' }).click()
  await expect(page).toHaveURL('/register/request-submitted')
  return workEmail
}

async function openRequest(page: Page, organisationId: string, workEmail: string) {
  const usersPage = new OrganisationUsersPage(page)
  await usersPage.goto(organisationId)
  await usersPage.expectUser(workEmail, 'Requested')
  return usersPage
}

test('approves a registration request', { tag: mutatesDataTag }, async ({ page }) => {
  const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
  const workEmail = await requestAccess(page, organisationId)
  const usersPage = await openRequest(page, organisationId, workEmail)

  await usersPage.row(workEmail).getByRole('link', { name: 'Approve', exact: true }).click()
  await expect(page.getByRole('heading', { name: 'Approve user' })).toBeVisible()
  await expect(page.getByText(`You are about to approve ${workEmail}'s request`)).toBeVisible()
  await page.getByRole('button', { name: 'Approve user' }).click()

  await expect(page.getByRole('heading', { name: 'Approval Email Sent' })).toBeVisible()
  await expect(page.getByText(`We’ve sent an email to ${workEmail}`)).toBeVisible()
  // Approving invites the requester, so they become a pending user.
  await usersPage.expectUser(workEmail, 'Pending')
})

test('rejects a registration request', { tag: mutatesDataTag }, async ({ page }) => {
  const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
  const workEmail = await requestAccess(page, organisationId)
  const usersPage = await openRequest(page, organisationId, workEmail)

  await usersPage.row(workEmail).getByRole('link', { name: 'Reject', exact: true }).click()
  await expect(page.getByRole('heading', { name: 'Reject user' })).toBeVisible()
  await expect(page.getByText(`You are about to reject ${workEmail}'s request`)).toBeVisible()
  await page.getByRole('button', { name: 'Reject user' }).click()

  await expect(page.getByRole('heading', { name: 'Rejection Email Sent' })).toBeVisible()
  await expect(page.getByText(`We’ve sent an email to ${workEmail}`)).toBeVisible()
  // Rejected requests aren't listed.
  await usersPage.filterByEmail(workEmail)
  await expect(usersPage.noUsersMessage).toBeVisible()
})

/* Opens the `action` page for a registration request. A read-only test can't submit a request, so
 * on the ephemeral stack one is submitted from a separate page outside the read-only guard, and a
 * deployed environment must already have one. */
async function openAnyRequest(
  browser: Browser,
  baseURL: string | undefined,
  page: Page,
  organisationId: string,
  action: string,
): Promise<string | undefined> {
  const usersPage = new OrganisationUsersPage(page)
  if (!isEphemeralStack()) {
    return usersPage.openUser(
      organisationId,
      { status: 'RequestedAccess', sortBy: 'Email', sortDirection: 'Ascending' },
      action,
    )
  }

  // Only the ephemeral stack gets here, which is plain http, so the project's other context
  // options, such as ignoreHTTPSErrors, aren't needed.
  const requestContext = await browser.newContext({ baseURL })
  let workEmail: string
  try {
    workEmail = await requestAccess(await requestContext.newPage(), organisationId)
  } finally {
    await requestContext.close()
  }
  await openRequest(page, organisationId, workEmail)
  await usersPage.row(workEmail).getByRole('link', { name: action, exact: true }).click()
  return workEmail
}

test.describe('Reviewing a registration request without saving', () => {
  test.use({ readOnly: true })

  for (const { action, verb } of [
    { action: 'Approve', verb: 'approve' },
    { action: 'Reject', verb: 'reject' },
  ]) {
    test(`opens ${verb} and cancels`, async ({ baseURL, browser, page }) => {
      const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
      const workEmail = await openAnyRequest(browser, baseURL, page, organisationId, action)
      if (!workEmail) {
        test.skip(true, `Organisation ${organisationId} has no registration request to ${verb}`)
        return
      }

      const heading = page.getByRole('heading', { name: `${action} user` })
      await expect(heading).toBeVisible()
      await expect(page.getByText(`You are about to ${verb} ${workEmail}'s request`)).toBeVisible()
      const usersPage = new OrganisationUsersPage(page)
      await usersPage.cancel()
      await expect(heading).toBeHidden()

      await usersPage.goto(organisationId)
      await usersPage.expectUser(workEmail, 'Requested')
    })
  }
})
