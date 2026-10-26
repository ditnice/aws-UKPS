import { expect, test } from '../../fixtures/test'
import { mutatesDataTag, requireEnvironmentVariable } from '../../helpers/test-environment'
import { OrganisationUsersPage } from '../../pages/organisation-users.page'

test.describe('Deactivating and reactivating a user', { tag: mutatesDataTag }, () => {
  // 'reactivates the deactivated user' reactivates the user the previous test deactivates.
  test.describe.configure({ mode: 'serial' })
  let deactivatedUserEmail: string | undefined

  test('deactivates an active user', async ({ page }) => {
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
    const usersPage = new OrganisationUsersPage(page)
    // The promote spec claims its user from the other end of the list.
    const userEmail = await usersPage.openActiveStandardUser(organisationId, 'Descending')

    await page.getByText('Deactivate user', { exact: true }).click()
    await page.getByRole('button', { name: 'Continue' }).click()

    await expect(page.getByRole('heading', { name: 'Deactivate user' })).toBeVisible()
    await page.getByRole('button', { name: 'Deactivate User' }).click()

    await usersPage.expectLoaded()
    await usersPage.expectUser(userEmail, 'Deactivated')
    deactivatedUserEmail = userEmail
  })

  test('reactivates the deactivated user', async ({ page }) => {
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
    const userEmail = deactivatedUserEmail
    if (!userEmail) {
      throw new Error('No user was deactivated by the previous test')
    }
    const usersPage = new OrganisationUsersPage(page)
    await usersPage.goto(organisationId)
    await usersPage.openUserAction(userEmail, 'Reactivate')

    await expect(page.getByRole('heading', { name: 'Reactivate user' })).toBeVisible()
    await page.getByRole('button', { name: 'Reactivate user' }).click()

    await usersPage.expectLoaded()
    await usersPage.expectUser(userEmail, 'Active')
  })
})
