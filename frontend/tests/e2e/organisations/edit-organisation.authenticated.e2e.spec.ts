import { expect, test } from '../fixtures/test'
import { requireEnvironmentVariable } from '../helpers/test-environment'

test('validates organisation details without changing authenticated-dev data', async ({ page }) => {
  const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
  await page.goto(`/portal/organisations/${organisationId}/edit`)

  await expect(
    page.getByRole('heading', { name: "Edit your organisation's details" }),
  ).toBeVisible()
  await page.getByLabel('Organisation name').fill('')
  await page.getByRole('button', { name: 'Save changes' }).click()

  await expect(page.getByLabel('Organisation name')).toHaveAttribute('aria-invalid', 'true')
  await expect(page.getByText('Enter the organisation name')).toBeVisible()
})

test('cancels an organisation edit without saving', async ({ page }) => {
  const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
  await page.goto(`/portal/organisations/${organisationId}`)
  await page.getByRole('link', { name: 'Edit details' }).click()
  await page.getByLabel('Organisation name').fill('Do not save this value')

  await page.getByRole('button', { name: 'Cancel' }).click()

  await expect(page).toHaveURL(`/portal/organisations/${organisationId}`)
})
