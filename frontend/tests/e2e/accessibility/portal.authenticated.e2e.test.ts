import { expect, test } from '../fixtures/test'
import { allRoles, forRoles } from '../helpers/roles'

test(
  'portal dashboard has no automatically detectable violations',
  { tag: forRoles(allRoles) },
  async ({ checkAccessibility, page, organisationId }) => {
    await page.goto('/portal')
    await expect(page).toHaveURL(`/portal/organisations/${organisationId}/records`)

    await checkAccessibility()
  },
)
