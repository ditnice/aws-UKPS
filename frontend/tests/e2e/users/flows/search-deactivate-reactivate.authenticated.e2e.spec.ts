import { expect, test } from '../../fixtures/test'
import { requireEnvironmentVariable } from '../../helpers/test-environment'

// need to add in if not on this screen then navigate to the next screen
test.describe('Searching, activating and deactivating a user', () => {
  test('deactivate an active user', async ({ page }) => {
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')

    await page.goto(`/portal/organisations/${organisationId}`)
    await expect(page.getByText('Organisation details')).toBeVisible()
    const activeUserRow = page
      .getByRole('row')
      .filter({
        has: page.getByRole('cell', {
          name: 'Active',
          exact: true,
        }),
      })
      .filter({
        has: page.getByRole('link', {
          name: 'Edit',
          exact: true,
        }),
      })
      .first()
    await expect(activeUserRow).toBeVisible()
    const userEmail = (await activeUserRow.getByRole('cell').first().innerText()).trim()
    await activeUserRow.getByRole('link', { name: 'Edit' }).click()
    await expect(page.getByRole('heading', { name: "Manage user's access" })).toBeVisible()
    await page.getByText('Deactivate user', { exact: true }).click()
    await page.getByRole('button', { name: 'Continue' }).click()

    await expect(page.getByRole('heading', { name: 'Deactivate user' })).toBeVisible()
    await page.getByRole('button', { name: 'Deactivate User' }).click()

    await expect(page.getByText('Organisation details')).toBeVisible()
    await page.getByRole('textbox', { name: 'Filter users' }).fill(userEmail)

    await page
      .getByRole('button', {
        name: 'Apply filter',
        exact: true,
      })
      .click()
    const userRow = page.getByRole('row').filter({
      has: page.getByRole('cell', {
        name: userEmail,
        exact: true,
      }),
    })
    await expect(userRow).toBeVisible()
    await expect(
      userRow.getByRole('cell', {
        name: 'Deactivated',
        exact: true,
      }),
    ).toBeVisible()
  })

  // need to add in if not on this screen then navigate to the next screen
  test('reactivate a deactive user', async ({ page }) => {
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')

    await page.goto(`/portal/organisations/${organisationId}`)
    await expect(page.getByText('Organisation details')).toBeVisible()
    const activeUserRow = page
      .getByRole('row')
      .filter({
        has: page.getByRole('cell', {
          name: 'Deactivated',
          exact: true,
        }),
      })
      .filter({
        has: page.getByRole('link', {
          name: 'Reactivate',
          exact: true,
        }),
      })
      .first()
    await expect(activeUserRow).toBeVisible()
    const userEmail = (await activeUserRow.getByRole('cell').first().innerText()).trim()
    await activeUserRow.getByRole('link', { name: 'Reactivate' }).click()
    await expect(page.getByRole('heading', { name: 'Reactivate user' })).toBeVisible()
    await page.getByRole('button', { name: 'Reactivate user' }).click()

    await expect(page.getByText('Organisation details')).toBeVisible()
    await page.getByRole('textbox', { name: 'Filter users' }).fill(userEmail)

    await page
      .getByRole('button', {
        name: 'Apply filter',
        exact: true,
      })
      .click()
    const userRow = page.getByRole('row').filter({
      has: page.getByRole('cell', {
        name: userEmail,
        exact: true,
      }),
    })
    await expect(userRow).toBeVisible()
    await expect(
      userRow.getByRole('cell', {
        name: 'Active',
        exact: true,
      }),
    ).toBeVisible()
  })

  test('search for a user', async ({ page }) => {
    const userEmail = requireEnvironmentVariable('E2E_USER_EMAIL')
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
    await page.goto(`/portal/organisations/${organisationId}`)

    await page.getByRole('textbox', { name: 'Filter users' }).fill(userEmail)
    await page.getByRole('button', { name: 'Apply filter', exact: true }).click()

    await expect(page.getByText(userEmail, { exact: true })).toBeVisible()
  })
})
