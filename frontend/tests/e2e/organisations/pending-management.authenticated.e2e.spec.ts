import { type Page } from '@playwright/test'

import { expect, test } from '../fixtures/test'
import { mutatesDataTag, requireEnvironmentVariable } from '../helpers/test-environment'
import { OrganisationUsersPage } from '../pages/organisation-users.page'

// Nothing seeds registration requests, so each test submits its own through the public form.
async function requestAccess(page: Page, organisationId: string): Promise<string> {
  const workEmail = `playwright-request-${Date.now()}-${test.info().workerIndex}@example.com`
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
