import { expect, test } from '../../fixtures/test'
import { mutatesDataTag, requireEnvironmentVariable } from '../../helpers/test-environment'

// A unique email, so a retry or an earlier run against a shared environment can't collide.
const testUser = {
  fullName: 'Test user',
  workEmail: `playwright-${Date.now()}@example.com`,
  phoneNumber: '07123456789',
}
test.describe('Adding and removing a user', { tag: mutatesDataTag }, () => {
  // 'remove a user' removes the user that 'adding a new user' invites.
  test.describe.configure({ mode: 'serial' })

  test('adding a new user', async ({ page }) => {
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
    await page.goto(`/portal/organisations/${organisationId}`)
    await expect(page.getByText('Organisation details')).toBeVisible()

    await page.getByRole('link', { name: 'Add a new user' }).click()
    await expect(page.getByRole('heading', { name: 'Add a new user' })).toBeVisible()
    await page.getByLabel('Full name').fill(testUser.fullName)
    await page.getByLabel('Work email address').fill(testUser.workEmail)
    await page.getByLabel('Phone number').fill(testUser.phoneNumber)

    await page.getByRole('button', { name: 'Send invite' }).click()

    await expect(page.getByText('Organisation details')).toBeVisible()
    await page.getByRole('textbox', { name: 'Filter users' }).fill(testUser.workEmail)
    await page
      .getByRole('button', {
        name: 'Apply filter',
        exact: true,
      })
      .click()
    const userRow = page.getByRole('row').filter({
      has: page.getByRole('cell', {
        name: testUser.workEmail,
        exact: true,
      }),
    })
    await expect(userRow).toBeVisible()
    await expect(
      userRow.getByRole('cell', {
        name: 'Pending',
        exact: true,
      }),
    ).toBeVisible()
    // cannot test the email sending functionality yet
  })

  test('remove a user', async ({ page }) => {
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')

    await page.goto(`/portal/organisations/${organisationId}`)
    await expect(page.getByText('Organisation details')).toBeVisible()
    await page.getByRole('textbox', { name: 'Filter users' }).fill(testUser.workEmail)
    await page
      .getByRole('button', {
        name: 'Apply filter',
        exact: true,
      })
      .click()
    const userRow = page.getByRole('row').filter({
      has: page.getByRole('cell', {
        name: testUser.workEmail,
        exact: true,
      }),
    })
    await userRow.getByRole('link', { name: 'Edit', exact: true }).click()

    await expect(page.getByRole('heading', { name: "Manage user's access" })).toBeVisible()
    await page.getByText('Remove user', { exact: true }).click()
    await page.getByRole('button', { name: 'Continue' }).click()

    await expect(page.getByRole('heading', { name: 'Remove user' })).toBeVisible()
    await page.getByRole('button', { name: 'Remove user' }).click()

    await expect(page.getByText('Organisation details')).toBeVisible()
    await page.getByRole('textbox', { name: 'Filter users' }).fill(testUser.workEmail)

    await page
      .getByRole('button', {
        name: 'Apply filter',
        exact: true,
      })
      .click()
    await expect(page.getByText('No users found for this organisation')).toBeVisible()
  })
})
