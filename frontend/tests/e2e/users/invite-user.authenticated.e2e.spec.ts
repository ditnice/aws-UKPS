import { buildUserActionHref } from '../../../src/app/portal/organisations/[id]/_lib/userActionAlert'
import { expect, test } from '../fixtures/test'
import { requireEnvironmentVariable } from '../helpers/test-environment'

test('invites a user without mutating authenticated-dev identity data', async ({ page }) => {
  const organisationId = Number(requireEnvironmentVariable('E2E_ORGANISATION_ID'))
  const email = `playwright-${Date.now()}@example.com`
  // Not a real user, so the alert falls back to generic copy rather than the email.
  const userId = 999_999_999
  await page.route('**/backend-api/users/onboard', async (route) => {
    await route.fulfill({ status: 201, json: { userId } })
  })
  await page.goto(`/portal/organisations/${organisationId}/onboard-user`)

  await page.getByLabel('Full name').fill('Playwright User')
  await page.getByLabel('Work email address').fill(email)
  await page.getByLabel('Phone number').fill('020 7946 0000')
  await page.getByRole('button', { name: 'Send invite' }).click()

  await expect(page).toHaveURL(buildUserActionHref(organisationId, { action: 'invited', userId }))
  await expect(page.getByRole('heading', { name: 'Invitation sent' })).toBeVisible()
})
