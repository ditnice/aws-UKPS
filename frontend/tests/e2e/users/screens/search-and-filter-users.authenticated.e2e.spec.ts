import { expect, test } from '../../fixtures/test'
import { requireEnvironmentVariable } from '../../helpers/test-environment'

test.describe('Searching and filtering users', () => {
  test('search and filter users page', async ({ page }) => {
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
    await page.goto(`/portal/organisations/${organisationId}`)
    await expect(page.locator('.filter-panel')).toHaveScreenshot('filter-panel.png', {
      maxDiffPixelRatio: 0.02,
    })
  })

  test('search for a user', async ({ page }) => {
    const userEmail = requireEnvironmentVariable('E2E_USER_EMAIL')
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
    await page.goto(`/portal/organisations/${organisationId}`)

    await page.getByRole('textbox', { name: 'Filter users' }).fill(userEmail)
    await page.getByRole('button', { name: 'Apply filter', exact: true }).click()

    await expect(page.getByText(userEmail, { exact: true })).toBeVisible()
  })

  test('filter standard user', async ({ page }) => {
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
    await page.goto(`/portal/organisations/${organisationId}`)

    await page.getByLabel('Standard user').check()

    await expect(page.getByText('Champion user')).toHaveCount(1)
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

  // test('filter for active in last 6 months', async ({ page }) => {
  //   const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')

  //   await page.goto(`/portal/organisations/${organisationId}`)
  //   const filter = page.getByRole('checkbox', {
  //     name: 'In the last 6 months',
  //   })
  //   await filter.check()
  //   await expect(filter).toBeChecked()
  //   const dates = page.locator('tbody tr td:nth-child(4)')
  //   const noUsersMessage = page.getByRole('cell', {
  //     name: 'No users found for this organisation.',
  //   })
  //   await expect
  //     .poll(async () => {
  //       if (await noUsersMessage.isVisible()) return 'empty'
  //       if ((await dates.count()) > 0) return 'results'
  //       return 'loading'
  //     })
  //     .not.toBe('loading')

  //   if (await noUsersMessage.isVisible()) {
  //     await expect(page.getByText('Showing results 0 to 0 of 0')).toBeVisible()
  //     await expect(dates).toHaveCount(0)
  //   } else {
  //     await expect(page.getByText('Filters (1 Active)')).toBeVisible()
  //     await expect(dates.first()).toBeVisible()
  //     const sixMonthsAgo = new Date()
  //     sixMonthsAgo.setMonth(sixMonthsAgo.getMonth() - 6)
  //     sixMonthsAgo.setHours(0, 0, 0, 0)

  //     for (const date of await dates.allTextContents()) {
  //       const [day, month, year] = date.trim().split('/').map(Number)
  //       const lastActive = new Date(year, month - 1, day)

  //       expect(lastActive.getTime()).toBeGreaterThanOrEqual(sixMonthsAgo.getTime())
  //     }
  //   }
  // })

  test('filter users by active and pending', async ({ page }) => {
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')

    await page.goto(`/portal/organisations/${organisationId}`)
    await page.getByRole('checkbox', { name: 'Active', exact: true }).check()
    await page.getByRole('checkbox', { name: 'Pending', exact: true }).check()

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
