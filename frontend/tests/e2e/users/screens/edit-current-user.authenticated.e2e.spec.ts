import { expect, test } from '../../fixtures/test'

test.describe('User details screen', () => {
  test('user details page', async ({ page }) => {
    await page.goto('/portal/user/me/edit-details')
    await expect(page).toHaveScreenshot('user-details.png', {
      fullPage: true,
    })
  })

  test('loads the current-user details form', async ({ page }) => {
    await page.goto('/portal/user/me/edit-details')

    await expect(page).toHaveURL('/portal/user/me/edit-details')
    await expect(page.getByLabel('Full name')).toBeVisible()
    await expect(page.getByLabel('Work email address')).toBeVisible()
    await expect(page.getByLabel('Contact number')).toBeVisible()
    await expect(page.getByRole('button', { name: 'Save' })).toBeVisible()
  })

  test('saves current-user details', async () => {
    test.fixme(true, 'The form does not call the user API or show a success state.')
  })
})
