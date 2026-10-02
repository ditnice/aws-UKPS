import { expect, test } from '../fixtures/test'
import { SignInPage } from '../pages/sign-in.page'

test.describe('sign-in', () => {
  test('shows accessible validation errors for empty fields', async ({ page }) => {
    const signInPage = new SignInPage(page)
    await signInPage.goto()

    await signInPage.submit.click()

    await expect(signInPage.email).toHaveAttribute('aria-invalid', 'true')
    await expect(signInPage.password).toHaveAttribute('aria-invalid', 'true')
    await expect(page.getByText('Enter your email address')).toBeVisible()
    await expect(page.getByText('Enter your password')).toBeVisible()
  })

  test('shows invalid credential errors returned by the API', async ({ page }) => {
    await page.route('**/backend-api/auth/login', async (route) => {
      await route.fulfill({
        contentType: 'application/problem+json',
        json: { status: 401, title: 'Unauthorized' },
        status: 401,
      })
    })
    const signInPage = new SignInPage(page)
    await signInPage.goto()

    await signInPage.signIn('person@example.com', 'incorrect-password')

    await expect(page.getByText('Invalid email or password')).toHaveCount(2)
    await expect(signInPage.email).toHaveAttribute('aria-invalid', 'true')
    await expect(signInPage.password).toHaveAttribute('aria-invalid', 'true')
  })

  test('continues to MFA and preserves the return URL', async ({ page }) => {
    await page.route('**/backend-api/auth/login', async (route) => {
      await route.fulfill({
        contentType: 'application/problem+json',
        json: {
          authenticationSession: 'test-authentication-session',
          challengeType: 'MultiFactorAuthenticationRequired',
          status: 401,
        },
        status: 401,
      })
    })
    const signInPage = new SignInPage(page)
    await signInPage.goto('/portal/organisations/1')

    await signInPage.signIn('person@example.com', 'valid-password')

    await expect(page).toHaveURL(
      (url) => {
        return (
          url.pathname === '/auth/sign-in/mfa' &&
          url.searchParams.get('username') === 'person@example.com' &&
          url.searchParams.get('session') === 'test-authentication-session' &&
          url.searchParams.get('returnTo') === '/portal/organisations/1'
        )
      },
      { timeout: 15_000 },
    )
    await expect(
      page.getByRole('heading', { name: 'Enter the code from your authenticator app' }),
    ).toBeVisible()
  })
})
