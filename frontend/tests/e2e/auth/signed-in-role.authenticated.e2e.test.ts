import { expect, test } from '../fixtures/test'
import { allRoles, forRoles } from '../helpers/roles'
import { signedInEmail } from '../helpers/test-environment'

// Checks each role project really acts as its own user, so role-specific specs can rely on it.
test.describe('Signed-in role', { tag: forRoles(allRoles) }, () => {
  test.use({ readOnly: true })

  test('acts as the role user', async ({ page, userRole }) => {
    if (!userRole) throw new Error('This spec only runs in an authenticated-<role> project')
    const email = signedInEmail(userRole)
    if (!email) throw new Error(`No signed-in email is known for the ${userRole} user`)

    await page.goto('/portal/user/me/edit-details/email')

    await expect(page.getByLabel('Work email address')).toHaveValue(email)
  })
})
