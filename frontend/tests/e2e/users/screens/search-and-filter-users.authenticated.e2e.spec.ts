import {
  lastActiveLabels,
  lastActivePresetDays,
  lastActivePresets,
} from '../../../../src/app/portal/organisations/[id]/_lib/userLabels'
import { expect, test } from '../../fixtures/test'
import { requireEnvironmentVariable, visualTag } from '../../helpers/test-environment'
import { OrganisationUsersPage } from '../../pages/organisation-users.page'

test.describe('Searching and filtering users', () => {
  test('shows the filter panel', { tag: visualTag }, async ({ page }) => {
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
    const usersPage = new OrganisationUsersPage(page)
    await usersPage.goto(organisationId)
    await expect(usersPage.filterPanel).toHaveScreenshot('filter-panel.png', {
      maxDiffPixelRatio: 0.02,
    })
  })

  test('searches for a user by email', async ({ page }) => {
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
    const usersPage = new OrganisationUsersPage(page)
    await usersPage.goto(organisationId)
    const emails = usersPage.cells('email')
    await expect(emails.first()).toBeVisible()
    const userEmail = (await emails.last().innerText()).trim()

    await usersPage.filterByEmail(userEmail)

    await expect(usersPage.activeFilter(userEmail)).toBeVisible()
    await expect(emails).toHaveText([userEmail])
  })

  test('filters to standard users', async ({ page }) => {
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
    const usersPage = new OrganisationUsersPage(page)
    await usersPage.goto(organisationId)

    await page.getByRole('checkbox', { name: 'Standard user', exact: true }).check()

    await expect(usersPage.activeFilter('Standard user')).toBeVisible()
    const roles = usersPage.cells('role')
    await expect(roles.first()).toBeVisible()
    await expect(roles.filter({ hasNotText: /^\s*Standard user\s*$/ })).toHaveCount(0)
  })

  for (const preset of lastActivePresets) {
    const label = lastActiveLabels[preset]
    test(`filters to users active ${label.toLowerCase()}`, async ({ page }) => {
      const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
      const usersPage = new OrganisationUsersPage(page)
      await usersPage.goto(organisationId)

      const filter = page.getByRole('checkbox', { name: label, exact: true })
      await filter.check()
      await expect(filter).toBeChecked()
      await expect(usersPage.activeFilter(label)).toBeVisible()

      const dates = usersPage.cells('lastActive')
      await expect(usersPage.noUsersMessage.or(dates.first())).toBeVisible()

      if (await usersPage.noUsersMessage.isVisible()) {
        await expect(page.getByText('Showing results 0 to 0 of 0')).toBeVisible()
        await expect(dates).toHaveCount(0)
        return
      }

      // The same window as the app's filter, from the start of that day as the table shows dates only.
      const cutoffDate = new Date(Date.now() - lastActivePresetDays[preset] * 24 * 60 * 60 * 1000)
      cutoffDate.setHours(0, 0, 0, 0)
      await expect
        .poll(
          async () =>
            (await dates.allTextContents())
              .map((date) => date.trim())
              .filter((date) => {
                const [day, month, year] = date.split('/').map(Number)
                return new Date(year, month - 1, day) < cutoffDate
              }),
          {
            message: `"${label}" should only list users active since ${cutoffDate.toLocaleDateString('en-GB')}`,
          },
        )
        .toEqual([])
    })
  }

  test('filters to active and pending users', async ({ page }) => {
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
    const usersPage = new OrganisationUsersPage(page)
    await usersPage.goto(organisationId)

    // Checked back to back, so the second change is made before the first has landed.
    await page.getByRole('checkbox', { name: 'Active', exact: true }).check()
    await page.getByRole('checkbox', { name: 'Pending', exact: true }).check()
    await expect(usersPage.activeFilter('Active')).toBeVisible()
    await expect(usersPage.activeFilter('Pending')).toBeVisible()

    await expect(page.getByRole('checkbox', { name: 'Active', exact: true })).toBeChecked()
    await expect(page.getByRole('checkbox', { name: 'Pending', exact: true })).toBeChecked()

    await expect(page.getByRole('button', { name: 'Filters (2 Active)' })).toBeVisible()
    const statuses = usersPage.cells('status')
    await expect(statuses.first()).toBeVisible()
    await expect(statuses.filter({ hasNotText: /^\s*(Active|Pending)\s*$/ })).toHaveCount(0)
  })
})
