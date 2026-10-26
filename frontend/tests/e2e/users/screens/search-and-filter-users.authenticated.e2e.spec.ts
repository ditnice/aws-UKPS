import {
  lastActiveLabels,
  lastActivePresetDays,
  lastActivePresets,
} from '../../../../src/app/portal/organisations/[id]/_lib/userLabels'
import { expect, test } from '../../fixtures/test'
import { testEmailPrefix } from '../../helpers/test-data'
import { requireEnvironmentVariable, visualTag } from '../../helpers/test-environment'
import { OrganisationUsersPage } from '../../pages/organisation-users.page'

/** Whether a dd/mm/yyyy date from the table is on or after `cutoff`. Anything else, such as the
 * table's N/A for a user never active, is a user the filter shouldn't list. */
function isActiveSince(date: string, cutoff: Date): boolean {
  const [, day, month, year] = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(date) ?? []
  return Boolean(day) && new Date(Number(year), Number(month) - 1, Number(day)) >= cutoff
}

test.describe('Searching and filtering users', () => {
  test('shows the filter panel', { tag: visualTag }, async ({ page }) => {
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
    const usersPage = new OrganisationUsersPage(page)
    await usersPage.goto(organisationId)
    await expect(usersPage.filterPanel).toHaveScreenshot('filter-panel.png')
  })

  test('searches for a user by email', async ({ page }) => {
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
    const usersPage = new OrganisationUsersPage(page)
    await usersPage.goto(organisationId)
    const emails = usersPage.cells('email')
    await expect(emails.first()).toBeVisible()
    // Other specs remove users they create, so this picks a seeded user, which no spec removes.
    const seededEmails = emails.filter({ hasNotText: new RegExp(`^\\s*${testEmailPrefix}`) })
    const userEmail = (await seededEmails.last().innerText()).trim()

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

      // The app filters from exactly this many days ago, but the table shows only dates, in the
      // frontend server's time zone, which may differ from this machine's. So the cutoff is the
      // start of the day before, which no time zone can push a matching user past.
      const cutoffDate = new Date(Date.now() - lastActivePresetDays[preset] * 24 * 60 * 60 * 1000)
      cutoffDate.setDate(cutoffDate.getDate() - 1)
      cutoffDate.setHours(0, 0, 0, 0)
      await expect
        .poll(
          async () =>
            (await dates.allTextContents())
              .map((date) => date.trim())
              .filter((date) => !isActiveSince(date, cutoffDate)),
          {
            message: `"${label}" should only list users with a last-active date on or after ${cutoffDate.toLocaleDateString('en-GB')}`,
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
