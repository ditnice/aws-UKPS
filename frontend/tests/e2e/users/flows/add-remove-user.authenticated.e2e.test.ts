import { expect, test } from '../../fixtures/test'
import { forRoles } from '../../helpers/roles'
import { uniqueEmail } from '../../helpers/test-data'
import { mutatesDataTag } from '../../helpers/test-environment'
import { OrganisationUsersPage } from '../../pages/organisation-users.page'

// A unique email, so a retry or an earlier run against a shared environment can't collide.
const testUser = {
  fullName: 'Test user',
  workEmail: uniqueEmail('invite'),
  phoneNumber: '07123456789',
}

// In file order on one worker, so the read-only tests never open a user while it's being changed.
test.describe.configure({ mode: 'default' })

test.describe('Adding and removing a user', { tag: forRoles(['Super'], mutatesDataTag) }, () => {
  // 'removes the invited user' removes the user that 'invites a new user' invites.
  test.describe.configure({ mode: 'serial' })

  test('invites a new user', async ({ page, organisationId }) => {
    const usersPage = new OrganisationUsersPage(page)
    await usersPage.goto(organisationId)

    await page.getByRole('link', { name: 'Add a new user' }).click()
    await expect(page.getByRole('heading', { name: 'Add a new user' })).toBeVisible()
    await page.getByLabel('Full name').fill(testUser.fullName)
    await page.getByLabel('Work email address').fill(testUser.workEmail)
    await page.getByLabel('Phone number').fill(testUser.phoneNumber)

    await page.getByRole('button', { name: 'Send invite' }).click()

    await usersPage.expectLoaded()
    await usersPage.expectUser(testUser.workEmail, 'Pending')
    // The invitation email is queued on Floci, but the listener that sends it is off in the stack.
  })

  test('removes the invited user', async ({ page, organisationId }) => {
    const usersPage = new OrganisationUsersPage(page)
    await usersPage.goto(organisationId)
    await usersPage.openUserAction(testUser.workEmail, 'Edit')

    await expect(
      page.getByRole('heading', { name: 'Manage users in your organisation' }),
    ).toBeVisible()
    await usersPage.chooseAction('Remove user')

    await expect(page.getByRole('heading', { name: 'Remove user' })).toBeVisible()
    await page.getByRole('button', { name: 'Remove user' }).click()

    await usersPage.expectLoaded()
    await usersPage.filterByEmail(testUser.workEmail)
    await expect(usersPage.noUsersMessage).toBeVisible()
  })
})

test.describe('Adding and removing a user without saving', () => {
  test.use({ readOnly: true })

  test(
    'checks the invite form and cancels',
    { tag: forRoles(['Super', 'Champion']) },
    async ({ page, organisationId }) => {
      const usersPage = new OrganisationUsersPage(page)
      await usersPage.goto(organisationId)

      await page.getByRole('link', { name: 'Add a new user' }).click()
      await expect(page.getByRole('heading', { name: 'Add a new user' })).toBeVisible()
      await page.getByRole('button', { name: 'Send invite' }).click()
      await expect(page.getByText('Enter their full name')).toBeVisible()
      await expect(page.getByText('Enter an email address')).toBeVisible()
      await expect(page.getByText('Enter their phone number')).toBeVisible()

      await page.getByLabel('Full name').fill(testUser.fullName)
      await page.getByLabel('Work email address').fill(testUser.workEmail)
      await page.getByLabel('Phone number').fill(testUser.phoneNumber)
      // The phone error only clears on blur, and its removal moves Cancel out from under a click.
      await page.getByLabel('Phone number').blur()
      await expect(page.getByText('Enter their phone number')).toBeHidden()
      await usersPage.cancel()

      await usersPage.expectLoaded()
      await usersPage.filterByEmail(testUser.workEmail)
      await expect(usersPage.noUsersMessage).toBeVisible()
    },
  )

  // Removing a user is global across organisations, so only a super user can.
  test(
    'opens remove user and cancels',
    { tag: forRoles(['Super']) },
    async ({ page, organisationId }) => {
      const usersPage = new OrganisationUsersPage(page)
      // A seeded pending user, as other specs only change active users or ones they invite.
      const userEmail = await usersPage.openUser(
        organisationId,
        { status: 'AwaitingSetup', sortBy: 'Email', sortDirection: 'Ascending' },
        'Edit',
      )
      if (!userEmail) {
        test.skip(true, `Organisation ${organisationId} has no pending user to remove`)
        return
      }

      await expect(
        page.getByRole('heading', { name: 'Manage users in your organisation' }),
      ).toBeVisible()
      await usersPage.chooseAction('Remove user')

      const heading = page.getByRole('heading', { name: 'Remove user' })
      await expect(heading).toBeVisible()
      await usersPage.cancel()
      await expect(heading).toBeHidden()

      await usersPage.goto(organisationId)
      await usersPage.expectUser(userEmail, 'Pending')
    },
  )
})

test.describe(
  'Adding a user as a champion',
  { tag: forRoles(['Champion'], mutatesDataTag) },
  () => {
    test('invites a new user to their organisation', async ({ page, organisationId }) => {
      const workEmail = uniqueEmail('champion-invite')
      const usersPage = new OrganisationUsersPage(page)
      await usersPage.goto(organisationId)

      await page.getByRole('link', { name: 'Add a new user' }).click()
      await expect(page.getByRole('heading', { name: 'Add a new user' })).toBeVisible()
      await page.getByLabel('Full name').fill(testUser.fullName)
      await page.getByLabel('Work email address').fill(workEmail)
      await page.getByLabel('Phone number').fill(testUser.phoneNumber)
      await page.getByRole('button', { name: 'Send invite' }).click()

      await usersPage.expectLoaded()
      await usersPage.expectUser(workEmail, 'Pending')
    })
  },
)

test.describe('Removing a user as a champion', { tag: forRoles(['Champion']) }, () => {
  test.use({ readOnly: true })

  test("isn't offered, and the remove page isn't found", async ({
    page,
    organisationId,
    userRole,
  }) => {
    const usersPage = new OrganisationUsersPage(page)
    const userEmail = await usersPage.claimUser(userRole, organisationId, 'readonly', 'Edit', () =>
      usersPage.openActiveStandardUser(organisationId, 'Ascending'),
    )
    if (!userEmail) {
      test.skip(true, `Organisation ${organisationId} has no active standard user to open`)
      return
    }

    await expect(
      page.getByRole('heading', { name: 'Manage users in your organisation' }),
    ).toBeVisible()
    await expect(page.getByRole('radio', { name: 'Deactivate user', exact: true })).toBeVisible()
    await expect(page.getByRole('radio', { name: 'Remove user', exact: true })).toBeHidden()

    await page.goto(`${new URL(page.url()).pathname}/remove`)
    await expect(page.getByRole('heading', { name: 'Page not found' })).toBeVisible()
    await expect(page.getByRole('button', { name: 'Remove user' })).toBeHidden()
  })
})
