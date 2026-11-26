import { expect, test } from '../../fixtures/test'
import { allRoles, forRoles } from '../../helpers/roles'

import type { Page } from '@playwright/test'

type UserDetail = 'fullName' | 'contactNumber' | 'email'

// The edit pages live under /portal/organisations/{id}/users/{userId}/details/edit-details/{detail},
// so reach them via the current user's "Change" links rather than hardcoding ids.
const goToEditDetail = async (page: Page, detail: UserDetail) => {
  await page.goto('/portal/user/me')
  await page.locator(`a[href*="/details/edit-details/${detail}?"]`).click()
  await expect(page).toHaveURL(
    new RegExp(`/portal/organisations/\\d+/users/\\d+/details/edit-details/${detail}\\?`),
  )
}

test.describe('Edit user details pages', { tag: forRoles(allRoles) }, () => {
  test.describe('Edit user full name screen', () => {
    test.describe('without saving', () => {
      test.use({ readOnly: true })
      test('loads the form', async ({ page }) => {
        await goToEditDetail(page, 'fullName')

        await expect(page.getByRole('heading', { name: 'Edit Full name' })).toBeVisible()
        await expect(page.getByLabel('Full name')).toBeVisible()
        await expect(page.getByRole('button', { name: 'Save' })).toBeVisible()
      })
      test('shows errors for empty fields', async ({ page }) => {
        await goToEditDetail(page, 'fullName')
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
      test('loads the form', async ({ page }) => {
        await goToEditDetail(page, 'contactNumber')

        await expect(page.getByRole('heading', { name: 'Edit Contact number' })).toBeVisible()
        await expect(page.getByLabel('Contact number')).toBeVisible()
        await expect(page.getByRole('button', { name: 'Save' })).toBeVisible()
      })
      test('shows errors for empty fields', async ({ page }) => {
        await goToEditDetail(page, 'contactNumber')
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
      test('loads the form', async ({ page }) => {
        await goToEditDetail(page, 'email')

        await expect(page.getByRole('heading', { name: 'Edit Work email address' })).toBeVisible()
        await expect(page.getByLabel('Work email address')).toBeVisible()
        await expect(page.getByRole('button', { name: 'Submit' })).toBeVisible()
      })
      test('shows errors for empty fields', async ({ page }) => {
        await goToEditDetail(page, 'email')
        await expect(page.getByRole('heading', { name: 'Edit Work email address' })).toBeVisible()

        await page.getByLabel('Work email address').fill('')
        await page.getByRole('button', { name: 'Submit' }).click()

        await expect(page.getByLabel('Work email address')).toHaveAttribute('aria-invalid', 'true')

        await expect(page.getByText('Enter your work email address')).toBeVisible()
      })
    })
  })
})
