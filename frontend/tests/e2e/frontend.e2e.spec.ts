import { expect, test } from '@playwright/test'

test.describe('Frontend', () => {
  test('loads the homepage', async ({ page }) => {
    const response = await page.goto('/')

    expect(response?.status()).toBe(200)
    await expect(page).toHaveURL('https://localhost:3000/')
    await expect(page).toHaveTitle('UK PharmaScan')
    await expect(page.getByRole('banner', { name: 'Site header' })).toBeVisible()
    await expect(page.getByRole('main')).toBeVisible()
    await expect(page.getByRole('contentinfo')).toBeVisible()
  })
})
