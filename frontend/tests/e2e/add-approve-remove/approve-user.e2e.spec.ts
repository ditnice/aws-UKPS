import { expect, test } from '../fixtures/test'
import { requireEnvironmentVariable } from '../helpers/test-environment'

test('approves a user', async ({ page }) => {
  const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
  await page.route(
    '**backend-api/organisations/{organisationId:int}/registration-requests/approve',
    async (route) => {
      await route.fulfill({ status: 204 })
    },
  )
  await page.goto(`/portal/organisations/${organisationId}/approve`)

  await page.getByRole('button', { name: 'Approve' }).click()
  await expect(page).toHaveURL('/portal/organisations/${organisationId}')
  await expect(page.getByLabel('Organisation name')).toBeVisible()
})
