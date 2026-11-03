import { expect, test } from '../fixtures/test'
import { forRoles } from '../helpers/roles'
import { requireEnvironmentVariable } from '../helpers/test-environment'

test(
  'organisation management has no automatically detectable violations',
  { tag: forRoles(['Super']) },
  async ({ checkAccessibility, page }) => {
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
    await page.goto(`/portal/organisations/${organisationId}`)
    await expect(page.getByRole('table', { name: 'Organisation Users' })).toBeVisible()

    await checkAccessibility()
  },
)
