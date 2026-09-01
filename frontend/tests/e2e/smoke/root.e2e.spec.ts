import { expect, test } from '../fixtures/test'

test.describe('public root', () => {
  test('loads the homepage', async ({ page }) => {
    const response = await page.goto('/')

    expect(response?.status()).toBe(200)
    await expect(page).toHaveURL('/')
    await expect(page).toHaveTitle('UK PharmaScan')
    await expect(page.getByRole('heading', { level: 1 })).toHaveCount(1)
    await expect(page.getByRole('heading', { level: 1, name: 'Home' })).toBeVisible()
    await expect(page.getByRole('heading', { level: 2, name: 'UK PharmaScan' })).toBeVisible()
    await expect(page.getByRole('banner', { name: 'Site header' })).toBeVisible()
    await expect(page.getByRole('main')).toBeVisible()
    await expect(page.getByRole('contentinfo')).toBeVisible()
  })
})
