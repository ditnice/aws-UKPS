import { expect, test } from '../../fixtures/test'
import { forRoles } from '../../helpers/roles'
import { mutatesDataTag, signedInEmail } from '../../helpers/test-environment'
import { OrganisationUsersPage } from '../../pages/organisation-users.page'

// In file order on one worker, so the read-only tests never open a user while it's being changed.
test.describe.configure({ mode: 'default' })

test.describe(
  'Promoting and demoting a user',
  { tag: forRoles(['Super', 'Champion'], mutatesDataTag) },
  () => {
    // 'demotes the champion user' demotes the user that 'promotes a standard user' promotes.
    test.describe.configure({ mode: 'serial' })
    let promotedUserEmail: string | undefined

    test('promotes a standard user', async ({ page, organisationId, userRole }) => {
      const usersPage = new OrganisationUsersPage(page)
      // The deactivate spec claims its user from the other end of the list.
      const userEmail = await usersPage.claimUser(userRole, organisationId, 'promote', 'Edit', () =>
        usersPage.openActiveStandardUser(organisationId, 'Ascending'),
      )
      if (!userEmail) {
        throw new Error(`Organisation ${organisationId} has no active standard user to edit`)
      }

      await usersPage.chooseAction('Change user permissions')

      await expect(page.getByRole('heading', { name: 'Change user permissions' })).toBeVisible()
      await page.getByRole('button', { name: 'Make champion user' }).click()

      await usersPage.expectLoaded()
      await usersPage.expectUser(userEmail, 'Champion user')
      promotedUserEmail = userEmail
    })

    test('demotes the champion user', async ({ page, organisationId }) => {
      const userEmail = promotedUserEmail
      if (!userEmail) {
        throw new Error('No user was promoted by the previous test')
      }
      const usersPage = new OrganisationUsersPage(page)
      await usersPage.goto(organisationId)
      await usersPage.openUserAction(userEmail, 'Edit')

      await expect(
        page.getByRole('heading', { name: 'Manage users in your organisation' }),
      ).toBeVisible()
      await usersPage.chooseAction('Change user permissions')

      await expect(page.getByRole('heading', { name: 'Change user permissions' })).toBeVisible()
      await page.getByRole('button', { name: 'Make standard user' }).click()

      await usersPage.expectLoaded()
      await usersPage.expectUser(userEmail, 'Standard user')
    })
  },
)

test.describe(
  'Promoting and demoting a user without saving',
  { tag: forRoles(['Super', 'Champion']) },
  () => {
    test.use({ readOnly: true })

    test('opens change permissions for a standard user and cancels', async ({
      page,
      organisationId,
      userRole,
    }) => {
      const usersPage = new OrganisationUsersPage(page)
      const userEmail = await usersPage.claimUser(
        userRole,
        organisationId,
        'readonly',
        'Edit',
        () => usersPage.openActiveStandardUser(organisationId, 'Ascending'),
      )
      if (!userEmail) {
        test.skip(true, `Organisation ${organisationId} has no active standard user to edit`)
        return
      }

      await usersPage.chooseAction('Change user permissions')

      const heading = page.getByRole('heading', { name: 'Change user permissions' })
      await expect(heading).toBeVisible()
      await expect(page.getByRole('button', { name: 'Make champion user' })).toBeVisible()
      await usersPage.cancel()
      await expect(heading).toBeHidden()

      await usersPage.goto(organisationId)
      await usersPage.expectUser(userEmail, 'Standard user')
    })

    test('opens change permissions for a champion user and cancels', async ({
      page,
      organisationId,
      userRole,
    }) => {
      const usersPage = new OrganisationUsersPage(page)
      // Descending, as a user the other describe promotes sorts first in organisation 1.
      const userEmail = await usersPage.claimUser(
        userRole,
        organisationId,
        'champion',
        'Edit',
        () =>
          usersPage.openUser(
            organisationId,
            { status: 'Active', role: 'Champion', sortBy: 'Email', sortDirection: 'Descending' },
            'Edit',
          ),
      )
      if (!userEmail) {
        test.skip(true, `Organisation ${organisationId} has no active champion user to edit`)
        return
      }

      await expect(
        page.getByRole('heading', { name: 'Manage users in your organisation' }),
      ).toBeVisible()
      await usersPage.chooseAction('Change user permissions')

      const heading = page.getByRole('heading', { name: 'Change user permissions' })
      await expect(heading).toBeVisible()
      await expect(page.getByRole('button', { name: 'Make standard user' })).toBeVisible()
      await usersPage.cancel()
      await expect(heading).toBeHidden()

      await usersPage.goto(organisationId)
      await usersPage.expectUser(userEmail, 'Champion user')
    })
  },
)

test.describe('Managing your own access', { tag: forRoles(['Super', 'Champion']) }, () => {
  test.use({ readOnly: true })

  // Users can't change their own role or status, so their own row offers no actions.
  test('offers no actions on your own row', async ({ page, organisationId, userRole }) => {
    const email = userRole && signedInEmail(userRole)
    if (!email) throw new Error(`No signed-in email is known for the ${userRole} user`)
    const usersPage = new OrganisationUsersPage(page)
    await usersPage.goto(organisationId)

    await usersPage.filterByEmail(email)

    const row = usersPage.row(email)
    await expect(row).toBeVisible()
    for (const action of ['Edit', 'Reactivate', 'Approve', 'Reject']) {
      await expect(row.getByRole('link', { name: action, exact: true })).toHaveCount(0)
    }
  })
})
