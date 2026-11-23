import { mkdir } from 'node:fs/promises'
import { dirname } from 'node:path'

import { test as setup } from '@playwright/test'

import { userRoles } from '../helpers/roles'
import { signInRole } from '../helpers/sign-in-role'
import {
  authStatePath,
  organisationIdFor,
  requireAuthenticatedTarget,
  roleCredentials,
} from '../helpers/test-environment'

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
    const organisationId = organisationIdFor(role)
    if (!organisationId) {
      throw new Error(
        `E2E_${role.toUpperCase()}_ORGANISATION_ID or E2E_ORGANISATION_ID must be set to sign in as the ${role} user.`,
      )
    }

    await signInRole(page, role, credentials, organisationId)

    const statePath = authStatePath(role)
    await mkdir(dirname(statePath), { recursive: true })
    await page.context().storageState({ path: statePath })
  })
}
