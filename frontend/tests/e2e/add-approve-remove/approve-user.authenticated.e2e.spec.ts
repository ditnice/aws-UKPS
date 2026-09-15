import { expect, test } from '../fixtures/test'

const testUser = {
  fullName: 'Test user',
  workEmail: 'test@example.com',
  phoneNumber: '07123456789',
}
test('approves a user', async ({ page }) => {
  const response = await page.request.get('/api/users/me')
  const { organisationId } = await response.json()
  //const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
  await page.route(
    '**backend-api/organisations/{organisationId:int}/registration-requests/approve',
    async (route) => {
      await route.fulfill({ status: 204 })
    },
  )
  await page.goto(`/portal/organisations/${organisationId}/approve`)

  await page.getByRole('button', { name: 'Approve' }).click()
  await expect(page).toHaveURL('/portal/organisations/${organisationId}')
  await expect(page.getByLabel('Organisation name')).toBeVisible()
})

test('adding a user successfully', async ({ page }) => {
  const organisationSelect = page.getByLabel('Select the organisation you are requesting acess for')
  await organisationSelect.selectOption({ label: 'TestOrganisation' })
  await page.getByLabel('Full name').fill(testUser.fullName)
  await page.getByLabel('Work email address').fill(testUser.workEmail)
  await page.getByLabel('Phone number').fill(testUser.phoneNumber)

  await expect(organisationSelect).toHaveValue('1')
  await expect(page.getByLabel('Full name')).toHaveValue(testUser.fullName)
  await expect(page.getByLabel('Work email address')).toHaveValue(testUser.workEmail)
  await expect(page.getByLabel('Phone number')).toHaveValue(testUser.phoneNumber)
})

// need a test for removing a user
