import { expect, test } from '../fixtures/test'
import { forRoles } from '../helpers/roles'

test.describe('Creating records as a super user', { tag: forRoles(['Super']) }, () => {
  test.use({ readOnly: true })

  test("isn't offered the create record form", async ({ page, organisationId }) => {
    await page.goto(`/portal/organisations/${organisationId}/records/create`)

    await expect(
      page.getByRole('heading', { name: 'User is not permitted to view this page' }),
    ).toBeVisible()
    await expect(page.getByLabel('Company code')).toHaveCount(0)
    await expect(page.getByRole('button', { name: 'Save and continue' })).toHaveCount(0)
  })
})
