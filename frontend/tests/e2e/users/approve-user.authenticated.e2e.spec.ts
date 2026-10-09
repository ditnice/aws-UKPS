import { expect, test } from '../fixtures/test'
import { requireEnvironmentVariable } from '../helpers/test-environment'

const testUser = {
  fullName: 'Test user',
  workEmail: 'test@example.com',
  phoneNumber: '07123456789',
}

test('adding a new user', async ({ page }) => {
  const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
  await page.goto(`/portal/organisations/${organisationId}`)
  await expect(page.getByText('Organisation details')).toBeVisible()

  await page.getByRole('button', { name: 'Add a new user' }).click()
  await expect(page.getByRole('heading', { name: 'Add a new user' })).toBeVisible()
  await page.getByLabel('Full name').fill(testUser.fullName)
  await page.getByLabel('Work email address').fill(testUser.workEmail)
  await page.getByLabel('Phone number').fill(testUser.phoneNumber)

  await page.getByRole('button', { name: 'Send invite' }).click()

  await expect(page.getByText('Organisation details')).toBeVisible()
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

// need to click on a link in an email for approve to appear
// test('approve a user', async ({ page }) => {
//   const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')

//   await page.goto(`/portal/organisations/${organisationId}`)
//   await expect(page.getByText('Organisation details')).toBeVisible()
//   await page.getByRole('textbox', { name: 'Filter users' }).fill(testUser.workEmail)
//   await page.getByRole('button', { name: 'Apply filter', exact: true }).click()
//   const approveUserRow = page.getByRole('row').filter({
//     has: page.getByText(testUser.workEmail, { exact: true }),
//   })

//   await expect(approveUserRow).toBeVisible()
//   await approveUserRow.getByRole('link', { name: 'Approve' }).click()
//   await expect(page.getByRole('heading', { name: 'Approve user' })).toBeVisible()
//   await page.getByText('Approve user', { exact: true }).click()
//   await page.getByRole('button', { name: 'Continue' }).click()

//   await expect(page.getByText('Organisation details')).toBeVisible()
//   await page.getByRole('textbox', { name: 'Filter users' }).fill(testUser.workEmail)

//   await page
//     .getByRole('button', {
//       name: 'Apply filter',
//       exact: true,
//     })
//     .click()
//   const userRow = page.getByRole('row').filter({
//     has: page.getByRole('cell', {
//       name: testUser.workEmail,
//       exact: true,
//     }),
//   })
//   await expect(userRow).toBeVisible()
//   await expect(
//     userRow.getByRole('cell', {
//       name: 'Pending',
//       exact: true,
//     }),
//   ).toBeVisible()
// })

// this will only pass once the remove user functionality has been added
test('remove a user', async ({ page }) => {
  const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')

  await page.goto(`/portal/organisations/${organisationId}`)
  await expect(page.getByText('Organisation details')).toBeVisible()
  const activeUserRow = page
    .getByRole('row')
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
