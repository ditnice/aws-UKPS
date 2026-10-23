import { expect, test } from '../../fixtures/test'

test.describe('User details screen', () => {
  test('user details page', async ({ page }) => {
    await page.goto('/portal/user/me/edit-details')
    await expect(page.getByRole('heading', { name: 'Edit personal details' })).toBeVisible()
    // The field values come from whichever user is signed in, so they're masked.
    await expect(page).toHaveScreenshot('user-details.png', {
      fullPage: true,
      mask: [page.getByRole('textbox')],
      maxDiffPixelRatio: 0.02,
    })
  })

  test('loads the current-user details form', async ({ page }) => {
    await page.goto('/portal/user/me/edit-details')

    await expect(page).toHaveURL('/portal/user/me/edit-details')
    await expect(page.getByRole('heading', { name: 'Edit personal details' })).toBeVisible()
    await expect(page.getByLabel('Full name')).toBeVisible()
    await expect(page.getByLabel('Work email address')).toBeVisible()
    await expect(page.getByLabel('Contact number')).toBeVisible()
    await expect(page.getByRole('button', { name: 'Save' })).toBeVisible()
  })

  test('saves current-user details', async () => {
    test.fixme(true, 'The form does not call the user API or show a success state.')
  })

  test('error message on empty input fields', async ({ page }) => {
    await page.goto('/portal/user/me/edit-details')
    await expect(page.getByRole('heading', { name: 'Edit personal details' })).toBeVisible()

    await page.getByLabel('Full name').fill('')
    await page.getByLabel('Work email address').fill('')
    await page.getByLabel('Contact number').fill('')
    await page.getByRole('button', { name: 'Save' }).click()

    await expect(page.getByLabel('Full name')).toHaveAttribute('aria-invalid', 'true')
    await expect(page.getByLabel('Work email address')).toHaveAttribute('aria-invalid', 'true')
    await expect(page.getByLabel('Contact number')).toHaveAttribute('aria-invalid', 'true')

    await expect(page.getByText('Enter your full name')).toBeVisible()
    await expect(page.getByText('Enter your work email address')).toBeVisible()
    await expect(page.getByText('Enter your phone number')).toBeVisible()
  })
})
