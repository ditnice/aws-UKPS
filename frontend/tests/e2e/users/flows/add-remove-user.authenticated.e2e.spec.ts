import { expect, test } from '../../fixtures/test'
import { mutatesDataTag, requireEnvironmentVariable } from '../../helpers/test-environment'
import { OrganisationUsersPage } from '../../pages/organisation-users.page'

// A unique email, so a retry or an earlier run against a shared environment can't collide.
const testUser = {
  fullName: 'Test user',
  workEmail: `playwright-${Date.now()}@example.com`,
  phoneNumber: '07123456789',
}

// In file order on one worker, so the read-only tests never open a user while it's being changed.
test.describe.configure({ mode: 'default' })

test.describe('Adding and removing a user', { tag: mutatesDataTag }, () => {
  // 'removes the invited user' removes the user that 'invites a new user' invites.
  test.describe.configure({ mode: 'serial' })

  test('invites a new user', async ({ page }) => {
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
    const usersPage = new OrganisationUsersPage(page)
    await usersPage.goto(organisationId)

    await page.getByRole('link', { name: 'Add a new user' }).click()
    await expect(page.getByRole('heading', { name: 'Add a new user' })).toBeVisible()
    await page.getByLabel('Full name').fill(testUser.fullName)
    await page.getByLabel('Work email address').fill(testUser.workEmail)
    await page.getByLabel('Phone number').fill(testUser.phoneNumber)

    await page.getByRole('button', { name: 'Send invite' }).click()

    await usersPage.expectLoaded()
    await usersPage.expectUser(testUser.workEmail, 'Pending')
    // The invitation email is queued on Floci, but the listener that sends it is off in the stack.
  })

  test('removes the invited user', async ({ page }) => {
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
    const usersPage = new OrganisationUsersPage(page)
    await usersPage.goto(organisationId)
    await usersPage.openUserAction(testUser.workEmail, 'Edit')

    await expect(page.getByRole('heading', { name: "Manage user's access" })).toBeVisible()
    await page.getByText('Remove user', { exact: true }).click()
    await page.getByRole('button', { name: 'Continue' }).click()

    await expect(page.getByRole('heading', { name: 'Remove user' })).toBeVisible()
    await page.getByRole('button', { name: 'Remove user' }).click()

    await usersPage.expectLoaded()
    await usersPage.filterByEmail(testUser.workEmail)
    await expect(usersPage.noUsersMessage).toBeVisible()
  })
})

test.describe('Adding and removing a user without saving', () => {
  test.use({ readOnly: true })

  test('checks the invite form and cancels', async ({ page }) => {
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
    const usersPage = new OrganisationUsersPage(page)
    await usersPage.goto(organisationId)

    await page.getByRole('link', { name: 'Add a new user' }).click()
    await expect(page.getByRole('heading', { name: 'Add a new user' })).toBeVisible()
    await page.getByRole('button', { name: 'Send invite' }).click()
    await expect(page.getByText('Enter their full name')).toBeVisible()
    await expect(page.getByText('Enter an email address')).toBeVisible()
    await expect(page.getByText('Enter their phone number')).toBeVisible()

    await page.getByLabel('Full name').fill(testUser.fullName)
    await page.getByLabel('Work email address').fill(testUser.workEmail)
    await page.getByLabel('Phone number').fill(testUser.phoneNumber)
    // The phone error only clears on blur, and its removal moves Cancel out from under a click.
    await page.getByLabel('Phone number').blur()
    await expect(page.getByText('Enter their phone number')).toBeHidden()
    await usersPage.cancel()

    await usersPage.expectLoaded()
    await usersPage.filterByEmail(testUser.workEmail)
    await expect(usersPage.noUsersMessage).toBeVisible()
  })

  test('opens remove user and cancels', async ({ page }) => {
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
    const usersPage = new OrganisationUsersPage(page)
    // A seeded pending user, as other specs only change active users or ones they invite.
    const userEmail = await usersPage.openUser(
      organisationId,
      { status: 'AwaitingSetup', sortBy: 'Email', sortDirection: 'Ascending' },
      'Edit',
    )
    if (!userEmail) {
      test.skip(true, `Organisation ${organisationId} has no pending user to remove`)
      return
    }

    await expect(page.getByRole('heading', { name: "Manage user's access" })).toBeVisible()
    await page.getByText('Remove user', { exact: true }).click()
    await page.getByRole('button', { name: 'Continue' }).click()

    const heading = page.getByRole('heading', { name: 'Remove user' })
    await expect(heading).toBeVisible()
    await usersPage.cancel()
    await expect(heading).toBeHidden()

    await usersPage.goto(organisationId)
    await usersPage.expectUser(userEmail, 'Pending')
  })
})
