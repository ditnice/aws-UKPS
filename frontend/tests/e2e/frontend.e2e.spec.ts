import { expect, test } from '@playwright/test'

test.describe('Frontend', () => {
  test('loads the homepage', async ({ page }) => {
    const response = await page.goto('/')

    expect(response?.status()).toBe(200)
    await expect(page).toHaveURL('/')
    await expect(page).toHaveTitle('UK PharmaScan')
    await expect(page.getByRole('banner', { name: 'Site header' })).toBeVisible()
    await expect(page.getByRole('main')).toBeVisible()
    await expect(page.getByRole('contentinfo')).toBeVisible()
  })

  test('redirects unauthenticated portal visits to sign-in', async ({ page }) => {
    await page.goto('/portal', { waitUntil: 'domcontentloaded' })

    await expect(page).toHaveURL((url) => {
      return url.pathname === '/auth/sign-in' && url.searchParams.get('returnTo') === '/portal'
    })
    await expect(page.getByRole('heading', { name: 'Sign-in' })).toBeVisible()
  })

  test('preserves the organisation URL when redirecting to sign-in', async ({ page }) => {
    const returnTo =
      '/portal/organisations/1?lastActive=week&page=1&status=AwaitingSetup&status=Active'

    await page.goto(returnTo, { waitUntil: 'domcontentloaded' })

    await expect(page).toHaveURL((url) => {
      return url.pathname === '/auth/sign-in' && url.searchParams.get('returnTo') === returnTo
    })
    await expect(page.getByRole('heading', { name: 'Sign-in' })).toBeVisible()
  })
})
