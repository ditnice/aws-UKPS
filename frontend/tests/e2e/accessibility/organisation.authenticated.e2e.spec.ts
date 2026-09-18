import { expect, test } from '../fixtures/test'
import { requireEnvironmentVariable } from '../helpers/test-environment'

test('organisation management has no automatically detectable violations', async ({
  checkAccessibility,
  page,
}) => {
  const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
  await page.goto(`/portal/organisations/${organisationId}`)
  await expect(page.getByRole('table', { name: 'Organisation Users' })).toBeVisible()

  await checkAccessibility()
})
