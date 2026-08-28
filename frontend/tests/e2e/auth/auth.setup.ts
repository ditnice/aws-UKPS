import { mkdir } from 'node:fs/promises'
import { dirname } from 'node:path'

import { expect, test as setup } from '@playwright/test'

import { authStatePath, requireEnvironmentVariable } from '../helpers/test-environment'
import { generateTotp } from '../helpers/totp'
import { SignInPage } from '../pages/sign-in.page'

setup('authenticate the dev user', async ({ page }) => {
  const email = requireEnvironmentVariable('E2E_USER_EMAIL')
  const password = requireEnvironmentVariable('E2E_USER_PASSWORD')
  const totpSecret = requireEnvironmentVariable('E2E_TOTP_SECRET')
  const signInPage = new SignInPage(page)

  await signInPage.goto()
  await signInPage.signIn(email, password)
  await page.waitForURL((url) => ['/portal', '/auth/sign-in/mfa'].includes(url.pathname))

  if (new URL(page.url()).pathname === '/auth/sign-in/mfa') {
    await page.getByLabel('Security code').fill(generateTotp(totpSecret))
    await page.getByRole('button', { name: 'Continue' }).click()
  }

  await expect(page).toHaveURL('/portal')
  await expect(page.getByRole('heading', { name: 'Your dashboard' })).toBeVisible()

  await mkdir(dirname(authStatePath), { recursive: true })
  await page.context().storageState({ path: authStatePath })
})
