import { expect, test } from '../fixtures/test'

test('portal dashboard has no automatically detectable violations', async ({
  checkAccessibility,
  page,
}) => {
  await page.goto('/portal')
  await expect(page).toHaveURL(/\/portal\/organisations\/1\/records$/)

  await checkAccessibility()
})
