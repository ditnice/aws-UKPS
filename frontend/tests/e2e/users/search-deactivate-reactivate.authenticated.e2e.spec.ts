import { expect, test } from '../fixtures/test'
import { requireEnvironmentVariable } from '../helpers/test-environment'

test('deactivate a user', async ({ page }) => {
  const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
  const userId = 2
  await page.goto(`/portal/organisations/${organisationId}/users/${userId}/deactivate`)
  await expect(page.getByRole('heading', { name: 'Deactivate user' })).toBeVisible()

  await page.getByRole('button', { name: 'Deactivate User' }).click()

  await expect(page.getByText('Organisation details')).toBeVisible()
})

test('reactivate a user', async ({ page }) => {
  const userId = 2
  const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
  await page.goto(`/portal/organisations/${organisationId}/users/${userId}/reactivate`)
  await expect(page.getByRole('heading', { name: 'Reactivate user' })).toBeVisible()

  await page.getByRole('button', { name: 'Reactivate User' }).click()

  await expect(page.getByText('Organisation details')).toBeVisible()
})

test('search for a user', async ({ page }) => {
  const userEmail = requireEnvironmentVariable('E2E_USER_EMAIL')
  const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
  await page.goto(`/portal/organisations/${organisationId}`)

  await page.getByRole('textbox', { name: 'Filter users' }).fill(userEmail)
  await page.getByRole('button', { name: 'Apply filter', exact: true }).click()

  await expect(page.getByText(userEmail, { exact: true })).toBeVisible()
})

test('filter standard user', async ({ page }) => {
  const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
  await page.goto(`/portal/organisations/${organisationId}`)

  await page.getByLabel('Standard user').check()

  await expect(page.getByText('Champion user')).toHaveCount(1)
})

// test('filter active and pending users', async ({ page }) => {
//   const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
//   await page.goto(`/portal/organisations/${organisationId}`)

//   await page.getByLabel('Deactivated').check()
//   await page.getByLabel('Pending').check()

//   await expect(page.getByText('Active')).toHaveCount(4)
//   await expect(page.getByText('Inactive')).toHaveCount(1)
//   await expect(page.getByText('Requested')).toHaveCount(1)
// })
