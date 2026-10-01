import { expect, test } from '../fixtures/test'
import { requireEnvironmentVariable } from '../helpers/test-environment'

const testUser = {
  fullName: 'Test user',
  workEmail: 'test@example.com',
  phoneNumber: '07123456789',
}

test('registering a user successfully', async ({ page }) => {
  const organisationSelect = page.getByLabel(
    'Select the organisation you are requesting access for',
  )
  await page.goto(`register/provide-details`)
  await expect(page.getByRole('heading', { name: 'Provide your details' })).toBeVisible()

  await page.getByLabel('Full name').fill(testUser.fullName)
  await page.getByLabel('Work email address').fill(testUser.workEmail)
  await page.getByLabel('Phone number').fill(testUser.phoneNumber)
  await organisationSelect.selectOption({ label: 'Wisoky and Sons' })

  await page.getByRole('button', { name: 'Submit request' }).click()

  await expect(page.getByRole('heading', { name: 'Account request submitted' })).toBeVisible()
})

test('register a user - name not provided', async ({ page }) => {
  const organisationSelect = page.getByLabel(
    'Select the organisation you are requesting access for',
  )
  await page.goto(`register/provide-details`)
  await expect(page.getByRole('heading', { name: 'Provide your details' })).toBeVisible()

  await page.getByLabel('Work email address').fill(testUser.workEmail)
  await page.getByLabel('Phone number').fill(testUser.phoneNumber)
  await organisationSelect.selectOption({ label: 'Wisoky and Sons' })

  await page.getByRole('button', { name: 'Submit request' }).click()

  await expect(page.getByText('Enter your full name')).toBeVisible()
})

test('approving a user', async ({ page }) => {
  const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
  await page.goto(`portal/organisations/${organisationId}`)
  await expect(page.getByText('Organisation details')).toBeVisible()

  await page.getByRole('textbox', { name: 'Filter users' }).fill(testUser.workEmail)
  await page.getByRole('button', { name: 'Apply filter', exact: true }).click()

  await page.getByLabel('Approve').click()

  await expect(page.getByRole('heading', { name: 'Approve user' })).toBeVisible()
})

// need a test for removing a user once implemented
