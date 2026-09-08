import { expect, test } from '../fixtures/test'
import { requireEnvironmentVariable } from '../helpers/test-environment'

test('invites a user without mutating authenticated-dev identity data', async ({ page }) => {
  const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
  const email = `playwright-${Date.now()}@example.com`
  await page.route('**/backend-api/users/onboard', async (route) => {
    await route.fulfill({ status: 204 })
  })
  await page.goto(`/portal/organisations/${organisationId}/onboard-user`)

  await page.getByLabel('Full name').fill('Playwright User')
  await page.getByLabel('Work email address').fill(email)
  await page.getByLabel('Phone number').fill('07700 900 982')
  await page.getByRole('button', { name: 'Send invite' }).click()

  await expect(page).toHaveURL(
    `/portal/organisations/${organisationId}?invited=${encodeURIComponent(email)}`,
  )
  await expect(page.getByRole('heading', { name: 'Invitation sent' })).toBeVisible()
  await expect(page.getByText(email)).toBeVisible()
})
