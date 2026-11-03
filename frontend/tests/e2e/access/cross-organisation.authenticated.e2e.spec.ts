import { expect, test } from '../fixtures/test'
import { currentUserId, expectSameAsMissing } from '../helpers/pages'
import { forRoles } from '../helpers/roles'
import { organisationIdFor } from '../helpers/test-environment'

/* A champion only manages their own organisation. organisation-details covers the details page;
 * these are the other pages under another organisation. */
test.describe('Using another organisation as a champion', { tag: forRoles(['Champion']) }, () => {
  test.use({ readOnly: true })

  let otherOrganisationId: string | undefined
  test.beforeEach(({ organisationId }) => {
    otherOrganisationId = organisationIdFor('Super')
    test.skip(
      !otherOrganisationId || otherOrganisationId === organisationId,
      'Needs the Super organisation to differ from the Champion organisation',
    )
  })

  const otherPath = (path: string) => `/portal/organisations/${otherOrganisationId}/${path}`

  test("can't edit its details", async ({ page }) => {
    await page.goto(otherPath('edit'))

    await expect(page.getByRole('heading', { name: 'Unable to load organisation' })).toBeVisible()
    await expect(
      page.getByRole('heading', { name: "Edit your organisation's details" }),
    ).toBeHidden()
    await expect(page.getByRole('button', { name: 'Submit' })).toHaveCount(0)
  })

  test("can't manage a user's access, and can't tell whether the user exists", async ({ page }) => {
    // The champion's own id: a user that exists, though not in the other organisation.
    const userId = await currentUserId(page)
    const url = (id: number) => otherPath(`manage-user-access/${id}`)

    await page.goto(url(userId))
    await expect(page.getByRole('radio')).toHaveCount(0)
    await expectSameAsMissing(page, url, userId)
  })

  test("can't see its records", async ({ page }) => {
    await page.goto(otherPath('records'))

    await expect(page.getByRole('table', { name: 'Organisation Records' })).toHaveCount(0)
    await expect(page.getByRole('link', { name: 'Create medicine record' })).toHaveCount(0)
  })

  // The invite page only checks the role, not the organisation, so a champion still sees the form
  // for another organisation; only the API refuses the invite.
  test.fixme("isn't offered the invite form", async ({ page }) => {
    await page.goto(otherPath('onboard-user'))

    await expect(page.getByRole('button', { name: 'Send invite' })).toHaveCount(0)
  })
})
