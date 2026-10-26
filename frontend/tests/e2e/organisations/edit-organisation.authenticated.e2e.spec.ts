import { expect, test } from '../fixtures/test'
import { mutatesDataTag, requireEnvironmentVariable, visualTag } from '../helpers/test-environment'

const testOrganisation = {
  organisationName: 'Test organisation',
  organisationAddress: 'The Old Bank Old Market Place Altrincham Cheshire WA14 4PA',
  organisationEmail: 'test@example.com',
  phoneNumber: '07123456789',
}
const fieldLabels = [
  'Organisation name',
  'Organisation address',
  'Head office email address',
  'Head office phone number',
] as const

// In file order on one worker, so no test reads the organisation while another changes it.
test.describe.configure({ mode: 'default' })

test('shows the edit organisation page', { tag: visualTag }, async ({ page }) => {
  const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
  await page.goto(`/portal/organisations/${organisationId}/edit`)
  await expect(
    page.getByRole('heading', { name: "Edit your organisation's details" }),
  ).toBeVisible()
  // The values depend on which tests in this file have run.
  await expect(page).toHaveScreenshot('edit-organisation.png', {
    fullPage: true,
    mask: [page.getByRole('textbox')],
    maxDiffPixelRatio: 0.02,
  })
})

test.describe('Editing organisation details', { tag: mutatesDataTag }, () => {
  // Each submit saves the whole form, so in parallel one test's stale form would overwrite
  // another's change to the same organisation.
  test.describe.configure({ mode: 'serial' })

  test('edits the organisation name', async ({ page }) => {
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
    await page.goto(`/portal/organisations/${organisationId}/edit`)

    await expect(
      page.getByRole('heading', { name: "Edit your organisation's details" }),
    ).toBeVisible()
    await page.getByLabel('Organisation name').fill(testOrganisation.organisationName)
    await page.getByRole('button', { name: 'Submit' }).click()

    await expect(
      page.getByRole('heading', { name: testOrganisation.organisationName }),
    ).toBeVisible()
    await expect(page.getByText('Organisation Details Updated')).toBeVisible()
  })

  test('edits the organisation address', async ({ page }) => {
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

  test('edits the head office email address', async ({ page }) => {
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

  test('edits the head office phone number', async ({ page }) => {
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
})

test.describe('Editing organisation details without saving', () => {
  test.use({ readOnly: true })

  test('edits the details and cancels', async ({ page }) => {
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
    const editHeading = page.getByRole('heading', { name: "Edit your organisation's details" })
    await page.goto(`/portal/organisations/${organisationId}`)
    await page.getByRole('link', { name: 'Edit details' }).click()
    await expect(editHeading).toBeVisible()

    const savedValues = await Promise.all(
      fieldLabels.map((label) => page.getByLabel(label).inputValue()),
    )
    await page.getByLabel('Organisation name').fill(testOrganisation.organisationName)
    await page.getByLabel('Organisation address').fill(testOrganisation.organisationAddress)
    await page.getByLabel('Head office email address').fill(testOrganisation.organisationEmail)
    await page.getByLabel('Head office phone number').fill(testOrganisation.phoneNumber)
    await page.getByRole('button', { name: 'Cancel' }).click()
    await expect(page).toHaveURL(`/portal/organisations/${organisationId}`)

    await page.goto(`/portal/organisations/${organisationId}/edit`)
    await expect(editHeading).toBeVisible()
    for (const [index, label] of fieldLabels.entries()) {
      await expect(page.getByLabel(label)).toHaveValue(savedValues[index])
    }
  })
})

test('shows errors for empty fields', async ({ page }) => {
  const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
  await page.goto(`/portal/organisations/${organisationId}/edit`)

  await expect(
    page.getByRole('heading', { name: "Edit your organisation's details" }),
  ).toBeVisible()
  await page.getByLabel('Organisation name').fill('')
  await page.getByLabel('Organisation address').fill('')
  await page.getByLabel('Head office email address').fill('')
  await page.getByLabel('Head office phone number').fill('')
  await page.getByRole('button', { name: 'Submit' }).click()

  await expect(page.getByText('Enter the organisation name')).toBeVisible()
  await expect(page.getByText('Enter organisation address')).toBeVisible()
  await expect(page.getByText('Enter an email address')).toBeVisible()
  await expect(page.getByText('Enter a phone number')).toBeVisible()
})
