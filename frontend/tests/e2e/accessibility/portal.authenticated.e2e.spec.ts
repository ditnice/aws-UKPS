import { expect, test } from '../fixtures/test'

test('portal dashboard has no automatically detectable violations', async ({
  checkAccessibility,
  page,
}) => {
  await page.goto('/portal')
  await expect(page.getByRole('heading', { name: 'Your dashboard' })).toBeVisible()

  await checkAccessibility()
})
