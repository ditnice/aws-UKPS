import { expect, test } from '../fixtures/test'

test.describe('authentication page accessibility', () => {
  test('sign-in page has no automatically detectable violations', async ({
    checkAccessibility,
    page,
  }) => {
    await page.goto('/auth/sign-in')
    await expect(page.getByRole('heading', { name: 'Sign-in' })).toBeVisible()

    await checkAccessibility()
  })

  test('sign-in validation errors have no automatically detectable violations', async ({
    checkAccessibility,
    page,
  }) => {
    await page.goto('/auth/sign-in')
    await page.getByRole('button', { name: 'Continue' }).click()
    await expect(page.getByLabel('Email address')).toHaveAttribute('aria-invalid', 'true')

    await checkAccessibility()
  })

  test('sign-up terms have no automatically detectable violations', async ({
    checkAccessibility,
    page,
  }) => {
    await page.goto('/auth/sign-up/terms-and-conditions?setupToken=test-token')
    await expect(page.getByRole('heading', { name: 'Terms and conditions' })).toBeVisible()

    await checkAccessibility()
  })
})
