import { Page } from '@playwright/test'

import { expect, test } from '../../fixtures/test'
import { requireEnvironmentVariable } from '../../helpers/test-environment'

function activeFilter(page: Page, label: string) {
  return page.getByRole('list', { name: 'Active filters' }).getByRole('listitem').filter({
    hasText: label,
  })
}

test.describe('Searching and filtering users', () => {
  test('search and filter users page', async ({ page }) => {
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
    await page.goto(`/portal/organisations/${organisationId}`)
    await expect(page.locator('.filter-panel')).toHaveScreenshot('filter-panel.png', {
      maxDiffPixelRatio: 0.02,
    })
  })

  test('search for a user', async ({ page }) => {
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
    await page.goto(`/portal/organisations/${organisationId}`)
    await expect(page.getByText('Loading users', { exact: false })).toBeHidden()
    const emails = page.locator('tbody tr td:nth-child(1)')
    const userEmail = (await emails.last().innerText()).trim()

    await page.getByRole('textbox', { name: 'Filter users' }).fill(userEmail)
    await page.getByRole('button', { name: 'Apply filter', exact: true }).click()

    await expect(activeFilter(page, userEmail)).toBeVisible()
    await expect(emails).toHaveText([userEmail])
  })

  test('filter standard user', async ({ page }) => {
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
    await page.goto(`/portal/organisations/${organisationId}`)

    await page.getByRole('checkbox', { name: 'Standard user', exact: true }).check()

    await expect(activeFilter(page, 'Standard user')).toBeVisible()
    const roles = page.locator('tbody tr td:nth-child(2)')
    await expect(roles.first()).toBeVisible()
    for (const role of await roles.allTextContents()) {
      expect(role.trim()).toBe('Standard user')
    }
  })

  const dateFilters = [
    { name: 'In the last week', days: 7 },
    { name: 'In the last month', months: 1 },
    { name: 'In the last 6 months', months: 6 },
    { name: 'In the last year', months: 12 },
  ]
  for (const filterOption of dateFilters) {
    test(`should filter users ${filterOption.name.toLowerCase()}`, async ({ page }) => {
      const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')

      await page.goto(`/portal/organisations/${organisationId}`)
      const filter = page.getByRole('checkbox', {
        name: filterOption.name,
        exact: true,
      })
      await filter.check()
      await expect(filter).toBeChecked()
      // The previous, unfiltered rows stay on screen until the filtered results arrive.
      await expect(activeFilter(page, filterOption.name)).toBeVisible()
      await expect(page.getByText('Loading users', { exact: false })).toBeHidden({ timeout: 15000 })
      const dates = page.locator('tbody tr td:nth-child(4)')
      const noUsersMessage = page.getByRole('cell', {
        name: 'No users found for this organisation.',
      })

      await expect
        .poll(
          async () => {
            if (await noUsersMessage.isVisible()) return 'empty'
            if ((await dates.count()) > 0) return 'results'
            return 'loading'
          },
          {
            timeout: 15000,
          },
        )
        .not.toBe('loading')

      if (await noUsersMessage.isVisible()) {
        await expect(page.getByText('Showing results 0 to 0 of 0')).toBeVisible()
        await expect(dates).toHaveCount(0)
      } else {
        const cutoffDate = new Date()
        if (filterOption.days) {
          cutoffDate.setDate(cutoffDate.getDate() - filterOption.days)
        }
        if (filterOption.months) {
          cutoffDate.setMonth(cutoffDate.getMonth() - filterOption.months)
        }
        cutoffDate.setHours(0, 0, 0, 0)

        for (const date of await dates.allTextContents()) {
          const [day, month, year] = date.trim().split('/').map(Number)
          const lastActive = new Date(year, month - 1, day)

          expect(
            lastActive.getTime(),
            `Filter "${filterOption.name}" returned user with Last Active date ${date.trim()}, which is before ${cutoffDate.toLocaleDateString('en-GB')}`,
          ).toBeGreaterThanOrEqual(cutoffDate.getTime())
        }
      }
    })
  }

  test('filter users by active and pending', async ({ page }) => {
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')

    await page.goto(`/portal/organisations/${organisationId}`)
    // Checked back to back, so the second change is made before the first has landed.
    await page.getByRole('checkbox', { name: 'Active', exact: true }).check()
    await page.getByRole('checkbox', { name: 'Pending', exact: true }).check()
    await expect(activeFilter(page, 'Active')).toBeVisible()
    await expect(activeFilter(page, 'Pending')).toBeVisible()

    await expect(page.getByRole('checkbox', { name: 'Active', exact: true })).toBeChecked()
    await expect(page.getByRole('checkbox', { name: 'Pending', exact: true })).toBeChecked()

    await expect(page.getByRole('button', { name: 'Filters (2 Active)' })).toBeVisible()
    const statuses = page.locator('tbody tr td:nth-child(3)')
    await expect(statuses.first()).toBeVisible()
    for (const status of await statuses.allTextContents()) {
      expect(['Active', 'Pending']).toContain(status.trim())
    }
  })
})
