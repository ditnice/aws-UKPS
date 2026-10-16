import { goToNextPage } from 'tests/e2e/helpers/pagination'

import { expect, test } from '../../fixtures/test'
import { requireEnvironmentVariable } from '../../helpers/test-environment'

const testUser = {
  fullName: 'Test user',
  workEmail: 'test@example.com',
  phoneNumber: '07123456789',
}
test.describe('Adding and removing a user', () => {
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
    const activeUserRow = page.getByRole('row').filter({
      has: page.getByRole('link', {
        name: 'Edit',
        exact: true,
      }),
    })

    let userEmail: string | undefined
    await expect(page.getByText('Loading users...')).toBeHidden()
    while (!userEmail) {
      const matchingRows = await activeUserRow.count()

      for (let i = 0; i < matchingRows; i++) {
        const row = activeUserRow.nth(i)
        const editLink = row.getByRole('link', {
          name: 'Edit',
          exact: true,
        })

        if (await editLink.isVisible()) {
          userEmail = (await row.getByRole('cell').first().innerText()).trim()
          await editLink.click()
          break
        }
      }

      if (userEmail) break

      if (!(await goToNextPage(page))) {
        throw new Error('No active user to be removed')
      }
    }
    await expect(page.getByRole('heading', { name: "Manage user's access" })).toBeVisible()
    await page.getByText('Remove user', { exact: true }).click()
    await page.getByRole('button', { name: 'Continue' }).click()

    await expect(page.getByRole('heading', { name: 'Remove user' })).toBeVisible()
    await page.getByRole('button', { name: 'Remove user' }).click()

    await expect(page.getByText('Organisation details')).toBeVisible()
    await page.getByRole('textbox', { name: 'Filter users' }).fill(userEmail)

    await page
      .getByRole('button', {
        name: 'Apply filter',
        exact: true,
      })
      .click()
    await expect(page.getByText('No users found for this organisation'))
  })
})
