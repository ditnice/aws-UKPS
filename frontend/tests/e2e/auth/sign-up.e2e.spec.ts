import { expect, test } from '../fixtures/test'

test.describe('sign-up', () => {
  test('explains when the sign-up link has no setup token', async ({ page }) => {
    await page.goto('/auth/sign-up/initiate')

    await expect(
      page.getByRole('heading', { name: 'There is a problem with your sign-up link' }),
    ).toBeVisible()
    await expect(page.getByText('This sign-up link is missing a setup token.')).toBeVisible()
  })

  test('continues from terms and conditions to password creation', async ({ page }) => {
    await page.goto('/auth/sign-up/terms-and-conditions?setupToken=test-token')

    await page.getByRole('link', { name: 'Accept and continue' }).click()

    await expect(page).toHaveURL('/auth/sign-up/set-password?setupToken=test-token')
    await expect(page.getByRole('heading', { name: 'Create a password' })).toBeVisible()
  })

  test('validates the password before submitting it', async ({ page }) => {
    await page.goto('/auth/sign-up/set-password?setupToken=test-token')

    const password = page.getByLabel('Password', { exact: true })
    await password.fill('short')
    await page.getByRole('button', { name: 'Continue' }).click()

    await expect(password).toHaveAttribute('aria-invalid', 'true')
    await expect(page.getByText('Password must be at least 8 characters long')).toBeVisible()
  })

  test('loads MFA setup from same-tab session storage', async ({ page }) => {
    await page.addInitScript(() => {
      sessionStorage.setItem(
        'signUpMfaSetup:v1',
        JSON.stringify({
          authenticationSession: 'test-authentication-session',
          otpAuthUri: 'otpauth://totp/UKPS:test@example.com?secret=JBSWY3DPEHPK3PXP',
          setupToken: 'test-token',
        }),
      )
    })

    await page.goto('/auth/sign-up/set-mfa')

    await expect(
      page.getByRole('heading', { name: 'Set up two-factor authentication' }),
    ).toBeVisible()
    await expect(page.getByLabel('QR code for authenticator app setup')).toBeVisible()
    await expect(page.getByLabel('Authentication code')).toBeVisible()
  })

  test('completes invitation, password and MFA setup', async () => {
    test.fixme(true, 'Authenticated dev has no per-user Cognito and database cleanup.')
  })
})
