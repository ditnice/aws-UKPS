import { expect, test } from '../../fixtures/test'
import { forRoles } from '../../helpers/roles'
import { mutatesDataTag } from '../../helpers/test-environment'
import { OrganisationUsersPage } from '../../pages/organisation-users.page'

// In file order on one worker, so the read-only tests never open a user while it's being changed.
test.describe.configure({ mode: 'default' })

test.describe(
  'Deactivating and reactivating a user',
  { tag: forRoles(['Super', 'Champion'], mutatesDataTag) },
  () => {
    // 'reactivates the deactivated user' reactivates the user the previous test deactivates.
    test.describe.configure({ mode: 'serial' })
    let deactivatedUserEmail: string | undefined

    test('deactivates an active user', async ({ page, organisationId, userRole }) => {
      const usersPage = new OrganisationUsersPage(page)
      // The promote spec claims its user from the other end of the list.
      const userEmail = await usersPage.claimUser(
        userRole,
        organisationId,
        'deactivate',
        'Edit',
        () => usersPage.openActiveStandardUser(organisationId, 'Descending'),
      )
      if (!userEmail) {
        throw new Error(`Organisation ${organisationId} has no active standard user to edit`)
      }

      await usersPage.chooseAction('Deactivate user')

      await expect(page.getByRole('heading', { name: 'Deactivate user' })).toBeVisible()
      await page.getByRole('button', { name: 'Deactivate User' }).click()

      await usersPage.expectLoaded()
      await usersPage.expectUser(userEmail, 'Deactivated')
      deactivatedUserEmail = userEmail
    })

    test('reactivates the deactivated user', async ({ page, organisationId }) => {
      const userEmail = deactivatedUserEmail
      if (!userEmail) {
        throw new Error('No user was deactivated by the previous test')
      }
      const usersPage = new OrganisationUsersPage(page)
      await usersPage.goto(organisationId)
      await usersPage.openUserAction(userEmail, 'Reactivate')

      await expect(page.getByRole('heading', { name: 'Reactivate user' })).toBeVisible()
      await page.getByRole('button', { name: 'Reactivate user' }).click()

      await usersPage.expectLoaded()
      await usersPage.expectUser(userEmail, 'Active')
    })
  },
)

test.describe(
  'Deactivating and reactivating a user without saving',
  { tag: forRoles(['Super', 'Champion']) },
  () => {
    test.use({ readOnly: true })

    test('opens deactivate and cancels', async ({ page, organisationId, userRole }) => {
      const usersPage = new OrganisationUsersPage(page)
      const userEmail = await usersPage.claimUser(
        userRole,
        organisationId,
        'readonly',
        'Edit',
        () => usersPage.openActiveStandardUser(organisationId, 'Descending'),
      )
      if (!userEmail) {
        test.skip(true, `Organisation ${organisationId} has no active standard user to edit`)
        return
      }

      await usersPage.chooseAction('Deactivate user')

      const heading = page.getByRole('heading', { name: 'Deactivate user' })
      await expect(heading).toBeVisible()
      await usersPage.cancel()
      await expect(heading).toBeHidden()

      await usersPage.goto(organisationId)
      await usersPage.expectUser(userEmail, 'Active')
    })

    test('opens reactivate and cancels', async ({ page, organisationId, userRole }) => {
      const usersPage = new OrganisationUsersPage(page)
      const userEmail = await usersPage.claimUser(
        userRole,
        organisationId,
        'deactivated',
        'Reactivate',
        () =>
          usersPage.openUser(
            organisationId,
            { status: 'Deactivated', sortBy: 'Email', sortDirection: 'Ascending' },
            'Reactivate',
          ),
      )
      if (!userEmail) {
        test.skip(true, `Organisation ${organisationId} has no deactivated user to reactivate`)
        return
      }

      const heading = page.getByRole('heading', { name: 'Reactivate user' })
      await expect(heading).toBeVisible()
      await usersPage.cancel()
      await expect(heading).toBeHidden()

      await usersPage.goto(organisationId)
      await usersPage.expectUser(userEmail, 'Deactivated')
    })
  },
)
