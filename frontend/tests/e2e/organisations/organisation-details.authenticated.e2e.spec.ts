import { expect, test } from '../fixtures/test'
import { forRoles } from '../helpers/roles'
import { organisationIdFor } from '../helpers/test-environment'

test.describe('Organisation details', { tag: forRoles(['Super', 'Champion']) }, () => {
  test.use({ readOnly: true })

  test('shows organisation details and users', async ({ page, organisationId }) => {
    await page.goto(`/portal/organisations/${organisationId}`)

    await expect(page.getByRole('heading', { level: 1 })).toHaveCount(1)
    await expect(page.getByRole('heading', { name: 'Organisation details' })).toBeVisible()
    await expect(page.getByRole('table', { name: 'Organisation Users' })).toBeVisible()
    await expect(page.getByRole('region', { name: 'Filter results' })).toBeVisible()
  })

  test('updates the URL when filtering users by email', async ({ page, organisationId }) => {
    await page.goto(`/portal/organisations/${organisationId}?page=3`)

    await page.getByLabel('Filter users').fill('example')
    await page.getByRole('button', { name: 'Apply filter' }).click()

    await expect(page).toHaveURL((url) => {
      return url.searchParams.get('email') === 'example' && url.searchParams.get('page') === '1'
    })
  })
})

test.describe('Another organisation', () => {
  test.use({ readOnly: true })

  test(
    "lets a super user view a champion's organisation",
    { tag: forRoles(['Super']) },
    async ({ page, organisationId }) => {
      const otherOrganisationId = organisationIdFor('Champion')
      test.skip(
        !otherOrganisationId || otherOrganisationId === organisationId,
        'Needs the Champion organisation to differ from the Super organisation',
      )

      await page.goto(`/portal/organisations/${otherOrganisationId}`)

      await expect(page.getByRole('heading', { name: 'Organisation details' })).toBeVisible()
      await expect(page.getByRole('table', { name: 'Organisation Users' })).toBeVisible()
    },
  )

  test(
    "doesn't let a champion view another organisation",
    { tag: forRoles(['Champion']) },
    async ({ page, organisationId }) => {
      const otherOrganisationId = organisationIdFor('Super')
      test.skip(
        !otherOrganisationId || otherOrganisationId === organisationId,
        'Needs the Super organisation to differ from the Champion organisation',
      )

      await page.goto(`/portal/organisations/${otherOrganisationId}`)

      await expect(page.getByRole('heading', { name: 'Unable to load organisation' })).toBeVisible()
      await expect(page.getByRole('heading', { name: 'Organisation details' })).toBeHidden()
      await expect(page.getByRole('table', { name: 'Organisation Users' })).toBeHidden()
    },
  )
})

test.describe('Organisation details as a standard user', { tag: forRoles(['Standard']) }, () => {
  test.use({ readOnly: true })

  test("doesn't show the organisation's details or users", async ({ page, organisationId }) => {
    await page.goto(`/portal/organisations/${organisationId}`)

    await expect(
      page.getByRole('heading', { name: 'User is not permitted to view this page' }),
    ).toBeVisible()
    await expect(page.getByRole('table', { name: 'Organisation Users' })).toBeHidden()
    await expect(page.getByRole('heading', { name: 'Organisation details' })).toBeHidden()
  })
})
