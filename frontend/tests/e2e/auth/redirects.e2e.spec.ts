import { expect, test } from '../fixtures/test'
import { isLocalBaseURL } from '../helpers/test-environment'

test.describe('unauthenticated portal access', () => {
  test.beforeEach(({ baseURL }) => {
    test.skip(isLocalBaseURL(baseURL), 'Local development uses AUTHENTICATION_MODE=DEV.')
  })

  test('redirects portal visits to sign-in', async ({ page }) => {
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
