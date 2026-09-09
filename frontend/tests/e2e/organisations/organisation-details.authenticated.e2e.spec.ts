import { expect, test } from '../fixtures/test'
import { requireEnvironmentVariable } from '../helpers/test-environment'

test('shows organisation details and users', async ({ page }) => {
  const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
  await page.goto(`/portal/organisations/${organisationId}`)

  await expect(page.getByRole('heading', { level: 1 })).toHaveCount(1)
  await expect(page.getByRole('heading', { name: 'Organisation details' })).toBeVisible()
  await expect(page.getByRole('table', { name: 'Organisation Users' })).toBeVisible()
  await expect(page.getByRole('region', { name: 'Filter results' })).toBeVisible()
})

test('updates the URL when filtering users by email', async ({ page }) => {
  const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
  await page.goto(`/portal/organisations/${organisationId}?page=3`)

  await page.getByLabel('Filter users').fill('example')
  await page.getByRole('button', { name: 'Apply filter' }).click()

  await expect(page).toHaveURL((url) => {
    return url.searchParams.get('email') === 'example' && url.searchParams.get('page') === '1'
  })
})
