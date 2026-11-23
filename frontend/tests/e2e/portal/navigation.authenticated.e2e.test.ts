import { expect, test } from '../fixtures/test'
import { allRoles, forRoles } from '../helpers/roles'

// The links break over two lines, so the names are matched across any whitespace.
const manageOrganisationLink = /manage my\s*organisation/i
const manageRecordsLink = /view and\s*manage records/i
const manageDetailsLink = /manage my\s*details/i

test.describe('Portal navigation', { tag: forRoles(allRoles) }, () => {
  test.use({ readOnly: true })

  test("opens on the user's own organisation's records", async ({ page, organisationId }) => {
    await page.goto('/portal')

    await expect(page).toHaveURL(`/portal/organisations/${organisationId}/records`)
  })

  test('shows the links for the role', async ({ page, organisationId, userRole }) => {
    await page.goto('/portal')
    const header = page.getByRole('banner', { name: 'Site header' })

    await expect(header.getByRole('link', { name: manageRecordsLink })).toHaveAttribute(
      'href',
      `/portal/organisations/${organisationId}/records`,
    )
    await expect(header.getByRole('link', { name: manageDetailsLink })).toBeVisible()
    // Standard users can't manage their organisation, so they aren't offered it.
    const organisationLink = header.getByRole('link', { name: manageOrganisationLink })
    if (userRole === 'Standard') {
      await expect(organisationLink).toHaveCount(0)
    } else {
      await expect(organisationLink).toHaveAttribute(
        'href',
        `/portal/organisations/${organisationId}`,
      )
    }
  })
})
