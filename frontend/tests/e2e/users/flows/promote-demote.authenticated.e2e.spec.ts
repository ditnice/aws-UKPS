import { expect, test } from '../../fixtures/test'
import { mutatesDataTag, requireEnvironmentVariable } from '../../helpers/test-environment'
import { OrganisationUsersPage } from '../../pages/organisation-users.page'

test.describe('Promoting and demoting a user', { tag: mutatesDataTag }, () => {
  // 'demotes the champion user' demotes the user that 'promotes a standard user' promotes.
  test.describe.configure({ mode: 'serial' })
  let promotedUserEmail: string | undefined

  test('promotes a standard user', async ({ page }) => {
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
    const usersPage = new OrganisationUsersPage(page)
    // The deactivate spec claims its user from the other end of the list.
    const userEmail = await usersPage.openActiveStandardUser(organisationId, 'Ascending')

    await page.getByText('Change user permissions', { exact: true }).click()
    await page.getByRole('button', { name: 'Continue' }).click()

    await expect(page.getByRole('heading', { name: 'Change user permissions' })).toBeVisible()
    await page.getByRole('button', { name: 'Make champion user' }).click()

    await usersPage.expectLoaded()
    await usersPage.expectUser(userEmail, 'Champion user')
    promotedUserEmail = userEmail
  })

  test('demotes the champion user', async ({ page }) => {
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
    const userEmail = promotedUserEmail
    if (!userEmail) {
      throw new Error('No user was promoted by the previous test')
    }
    const usersPage = new OrganisationUsersPage(page)
    await usersPage.goto(organisationId)
    await usersPage.openUserAction(userEmail, 'Edit')

    await expect(page.getByRole('heading', { name: "Manage user's access" })).toBeVisible()
    await page.getByText('Change user permissions', { exact: true }).click()
    await page.getByRole('button', { name: 'Continue' }).click()

    await expect(page.getByRole('heading', { name: 'Change user permissions' })).toBeVisible()
    await page.getByRole('button', { name: 'Make standard user' }).click()

    await usersPage.expectLoaded()
    await usersPage.expectUser(userEmail, 'Standard user')
  })
})
