import { expect, test } from '../../fixtures/test'
import { allRoles, forRoles } from '../../helpers/roles'

test.describe('Edit user details pages', { tag: forRoles(allRoles) }, () => {
  test.describe('Edit user full name screen', () => {
    test.describe('without saving', () => {
      test.use({ readOnly: true })
      const path = `/portal/user/me/edit-details/fullName`
      test('loads the form', async ({ page }) => {
        await page.goto(path)

        await expect(page).toHaveURL(path)
        await expect(page.getByRole('heading', { name: 'Edit Full name' })).toBeVisible()
        await expect(page.getByLabel('Full name')).toBeVisible()
        await expect(page.getByRole('button', { name: 'Save' })).toBeVisible()
      })
      test('shows errors for empty fields', async ({ page }) => {
        await page.goto(path)
        await expect(page.getByRole('heading', { name: 'Edit Full name' })).toBeVisible()

        await page.getByLabel('Full name').fill('')
        await page.getByRole('button', { name: 'Save' }).click()

        await expect(page.getByLabel('Full name')).toHaveAttribute('aria-invalid', 'true')

        await expect(page.getByText('Enter your full name')).toBeVisible()
      })
    })
  })

  test.describe('Edit user contact number screen', () => {
    test.describe('without saving', () => {
      test.use({ readOnly: true })
      const path = `/portal/user/me/edit-details/contactNumber`
      test('loads the form', async ({ page }) => {
        await page.goto(path)

        await expect(page).toHaveURL(path)
        await expect(page.getByRole('heading', { name: 'Edit Contact number' })).toBeVisible()
        await expect(page.getByLabel('Contact number')).toBeVisible()
        await expect(page.getByRole('button', { name: 'Save' })).toBeVisible()
      })
      test('shows errors for empty fields', async ({ page }) => {
        await page.goto(path)
        await expect(page.getByRole('heading', { name: 'Edit Contact number' })).toBeVisible()

        await page.getByLabel('Contact number').fill('')
        await page.getByRole('button', { name: 'Save' }).click()

        await expect(page.getByLabel('Contact number')).toHaveAttribute('aria-invalid', 'true')

        await expect(page.getByText('Enter your contact number')).toBeVisible()
      })
    })
  })

  test.describe('Edit user email screen', () => {
    test.describe('without saving', () => {
      test.use({ readOnly: true })
      const path = `/portal/user/me/edit-details/email`
      test('loads the form', async ({ page }) => {
        await page.goto(path)

        await expect(page).toHaveURL(path)
        await expect(page.getByRole('heading', { name: 'Edit Work email address' })).toBeVisible()
        await expect(page.getByLabel('Work email address')).toBeVisible()
        await expect(page.getByRole('button', { name: 'Submit' })).toBeVisible()
      })
      test('shows errors for empty fields', async ({ page }) => {
        await page.goto(path)
        await expect(page.getByRole('heading', { name: 'Edit Work email address' })).toBeVisible()

        await page.getByLabel('Work email address').fill('')
        await page.getByRole('button', { name: 'Submit' }).click()

        await expect(page.getByLabel('Work email address')).toHaveAttribute('aria-invalid', 'true')

        await expect(page.getByText('Enter your work email address')).toBeVisible()
      })
    })
  })
})
