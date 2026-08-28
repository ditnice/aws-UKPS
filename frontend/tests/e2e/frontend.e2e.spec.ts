import { test, expect, Page } from '@playwright/test'

test.describe('Frontend', () => {
  let _page: Page

  test.beforeAll(async ({ browser }, _testInfo) => {
    const context = await browser.newContext()
    _page = await context.newPage()
  })

  test('can go on homepage', async ({ page }) => {
    await page.goto('https://localhost:3000')

    await expect(page).toHaveTitle('UK PharmaScan')
  })
})
