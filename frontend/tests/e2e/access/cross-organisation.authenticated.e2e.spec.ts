import { expect, test } from '../fixtures/test'
import { expectConcealedApiResource, expectSameAsMissing, missingId } from '../helpers/pages'
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

  test("can't manage a user's access, and can't tell whether the user exists", async ({
    page,
    foreignUser,
  }) => {
    // Unlike the caller's own ID, this subject genuinely belongs to the addressed organisation.
    const userId = foreignUser.userId
    const url = (id: number) => otherPath(`manage-user-access/${id}`)

    await expectConcealedApiResource(
      page,
      (id) => `/backend-api/users/${id}/organisations/${foreignUser.organisationId}`,
      userId,
    )
    await expectSameAsMissing(page, url, userId)
    await expect(page.getByRole('radio')).toHaveCount(0)
    await expect(page.getByText(foreignUser.workEmail, { exact: false })).toHaveCount(0)
  })

  test("can't see its records", async ({ page }) => {
    await page.goto(otherPath('records'))

    await expect(page.getByRole('heading', { name: 'Unable to load organisation' })).toBeVisible()
    await expect(page.getByRole('table', { name: 'Organisation Records' })).toHaveCount(0)
    await expect(page.getByRole('link', { name: 'Create medicine record' })).toHaveCount(0)
  })

  test("isn't offered the invite form", async ({ page }) => {
    await page.goto(otherPath('onboard-user'))

    await expect(page.getByRole('heading', { name: 'Unable to load organisation' })).toBeVisible()
    await expect(page.getByLabel('Work email address')).toHaveCount(0)
    await expect(page.getByRole('button', { name: 'Send invite' })).toHaveCount(0)
  })

  test("isn't offered an invite form for a missing organisation", async ({ page }) => {
    await page.goto(`/portal/organisations/${missingId}/onboard-user`)

    await expect(page.getByRole('heading', { name: 'Unable to load organisation' })).toBeVisible()
    await expect(page.getByRole('button', { name: 'Send invite' })).toHaveCount(0)
  })
})

test.describe('Inviting across organisations as a super user', { tag: forRoles(['Super']) }, () => {
  test.use({ readOnly: true })

  test('offers the invitation form for another organisation', async ({ page, organisationId }) => {
    const otherOrganisationId = organisationIdFor('Champion')
    test.skip(
      !otherOrganisationId || otherOrganisationId === organisationId,
      'Needs a distinct Champion organisation',
    )

    await page.goto(`/portal/organisations/${otherOrganisationId}/onboard-user`)

    await expect(page.getByRole('heading', { name: 'Add a new user' })).toBeVisible()
    await expect(page.getByLabel('Work email address')).toBeVisible()
    await expect(page.getByRole('button', { name: 'Send invite' })).toBeVisible()
  })
})
