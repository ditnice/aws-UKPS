import { mkdir } from 'node:fs/promises'
import { dirname } from 'node:path'

import { expect, test as setup } from '@playwright/test'

import { userRoles } from '../helpers/roles'
import {
  authStatePath,
  requireAuthenticatedTarget,
  roleCredentials,
} from '../helpers/test-environment'
import { generateTotp } from '../helpers/totp'
import { SignInPage } from '../pages/sign-in.page'

/* Each role's auth-setup project greps its own title, so a role only signs in when its
 * authenticated project runs. */
for (const role of userRoles) {
  setup(`authenticate the ${role.toLowerCase()} user`, async ({ baseURL, page }) => {
    requireAuthenticatedTarget(baseURL)
    const credentials = roleCredentials(role)
    if (!credentials) {
      throw new Error(
        `E2E_${role.toUpperCase()}_EMAIL, _PASSWORD and _TOTP_SECRET must be set to sign in as the ${role} user.`,
      )
    }
    const signInPage = new SignInPage(page)

    await signInPage.goto()
    await signInPage.signIn(credentials.email, credentials.password)
    await page.waitForURL((url) => ['/portal', '/auth/sign-in/mfa'].includes(url.pathname))

    if (new URL(page.url()).pathname === '/auth/sign-in/mfa') {
      await page.getByLabel('Security code').fill(generateTotp(credentials.totpSecret))
      await page.getByRole('button', { name: 'Continue' }).click()
    }

    await expect(page).toHaveURL('/portal')
    await expect(page.getByRole('heading', { name: 'Your dashboard' })).toBeVisible()

    const statePath = authStatePath(role)
    await mkdir(dirname(statePath), { recursive: true })
    await page.context().storageState({ path: statePath })
  })
}
