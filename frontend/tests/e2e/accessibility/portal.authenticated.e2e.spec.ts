import { expect, test } from '../fixtures/test'
import { forRoles } from '../helpers/roles'

test(
  'portal dashboard has no automatically detectable violations',
  { tag: forRoles(['Super']) },
  async ({ checkAccessibility, page }) => {
    await page.goto('/portal')
    await expect(page).toHaveURL(/\/portal\/organisations\/1\/records$/)

    await checkAccessibility()
  },
)
