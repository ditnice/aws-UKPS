import { expect, test } from '../fixtures/test'

test.describe('public page accessibility', () => {
  test('homepage has no automatically detectable violations', async ({
    checkAccessibility,
    page,
  }) => {
    await page.goto('/')
    await expect(page.getByRole('main')).toBeVisible()

    await checkAccessibility()
  })

  test('skip link moves focus to the main content', async ({ page }) => {
    await page.goto('/')

    await page.keyboard.press('Tab')
    const skipLink = page.getByRole('link', { name: 'Skip to content' })
    await expect(skipLink).toBeFocused()
    await skipLink.press('Enter')

    await expect(page.locator('#content-start')).toBeFocused()
  })
})
