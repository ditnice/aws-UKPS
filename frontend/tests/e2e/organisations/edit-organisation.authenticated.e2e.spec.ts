import { expect, test } from '../fixtures/test'
import { requireEnvironmentVariable } from '../helpers/test-environment'

const testOrganisation = {
  organisationName: 'Test organisation',
  organisationAddress: 'The Old Bank Old Market Place Altrincham Cheshire WA14 4PA',
  organisationEmail: 'test@example.com',
  phoneNumber: '07123456789',
}

test('validates organisation details without changing authenticated-dev data', async ({ page }) => {
  const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
  await page.goto(`/portal/organisations/${organisationId}/edit`)

  await expect(
    page.getByRole('heading', { name: "Edit your organisation's details" }),
  ).toBeVisible()
  await page.getByLabel('Organisation name').fill('')
  await page.getByRole('button', { name: 'Submit' }).click()

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

test('successfully edits the oganisation name', async ({ page }) => {
  const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
  await page.goto(`/portal/organisations/${organisationId}/edit`)

  await expect(
    page.getByRole('heading', { name: "Edit your organisation's details" }),
  ).toBeVisible()
  await page.getByLabel('Organisation name').fill(testOrganisation.organisationName)
  await page.getByRole('button', { name: 'Submit' }).click()

  await expect(page.getByRole('heading', { name: testOrganisation.organisationName })).toBeVisible()
  await expect(page.getByText('Organisation Details Updated')).toBeVisible()
})

test('successfully edits the oganisation address', async ({ page }) => {
  const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
  await page.goto(`/portal/organisations/${organisationId}/edit`)

  await expect(
    page.getByRole('heading', { name: "Edit your organisation's details" }),
  ).toBeVisible()
  await page.getByLabel('Organisation address').fill(testOrganisation.organisationAddress)
  await page.getByRole('button', { name: 'Submit' }).click()

  await expect(page.getByText(testOrganisation.organisationAddress)).toBeVisible()
  await expect(page.getByText('Organisation Details Updated')).toBeVisible()
})

test('successfully edits the email', async ({ page }) => {
  const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
  await page.goto(`/portal/organisations/${organisationId}/edit`)

  await expect(
    page.getByRole('heading', { name: "Edit your organisation's details" }),
  ).toBeVisible()
  await page.getByLabel('Head office email address').fill(testOrganisation.organisationEmail)
  await page.getByRole('button', { name: 'Submit' }).click()

  await expect(page.getByText(testOrganisation.organisationEmail)).toBeVisible()
  await expect(page.getByText('Organisation Details Updated')).toBeVisible()
})

test('successfully edits the phone number', async ({ page }) => {
  const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
  await page.goto(`/portal/organisations/${organisationId}/edit`)

  await expect(
    page.getByRole('heading', { name: "Edit your organisation's details" }),
  ).toBeVisible()
  await page.getByLabel('Head office phone number').fill(testOrganisation.phoneNumber)
  await page.getByRole('button', { name: 'Submit' }).click()

  await expect(page.getByText(testOrganisation.phoneNumber)).toBeVisible()
  await expect(page.getByText('Organisation Details Updated')).toBeVisible()
})
