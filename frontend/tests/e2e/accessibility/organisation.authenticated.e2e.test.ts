import { expect, test } from '../fixtures/test'
import { forRoles } from '../helpers/roles'

test(
  'organisation management has no automatically detectable violations',
  { tag: forRoles(['Super', 'Champion']) },
  async ({ checkAccessibility, page, organisationId }) => {
    await page.goto(`/portal/organisations/${organisationId}`)
    await expect(page.getByRole('table', { name: 'Organisation Users' })).toBeVisible()

    await checkAccessibility()
  },
)
