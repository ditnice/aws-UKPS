import { expect, test } from '../../fixtures/test'
import { devRoleOrganisationIds, forRoles, type UserRole } from '../../helpers/roles'
import { championSubjects, type ChampionSubject, hasSeededSubjects } from '../../helpers/subjects'
import { OrganisationUsersPage } from '../../pages/organisation-users.page'

const changePermissions = 'Change user permissions'
const deactivate = 'Deactivate user'
const reactivate = 'Reactivate user'
const remove = 'Remove user'
const manageDetails = 'Manage user details and sign in method'

/* The actions each role is offered for the seeded subjects that never change. Only a Super user
 * can remove a user, and a deactivated user can only be reactivated (or removed). */
const cases: { subject: ChampionSubject; actions: Record<'Super' | 'Champion', string[]> }[] = [
  {
    subject: 'readonly',
    actions: {
      Super: [changePermissions, deactivate, remove, manageDetails],
      Champion: [changePermissions, deactivate, manageDetails],
    },
  },
  {
    subject: 'champion',
    actions: {
      Super: [changePermissions, deactivate, remove, manageDetails],
      Champion: [changePermissions, deactivate, manageDetails],
    },
  },
  {
    subject: 'deactivated',
    actions: { Super: [reactivate, remove], Champion: [reactivate] },
  },
]

function managingRole(userRole: UserRole | undefined): 'Super' | 'Champion' {
  if (userRole !== 'Super' && userRole !== 'Champion') {
    throw new Error(`Expected to run as a Super or Champion user, not ${userRole}`)
  }
  return userRole
}

test.describe("Manage user's access options", { tag: forRoles(['Super', 'Champion']) }, () => {
  test.use({ readOnly: true })

  for (const { subject, actions } of cases) {
    test(`offers the right actions for the ${subject} subject`, async ({ page, userRole }) => {
      test.skip(!hasSeededSubjects(), 'The fixed subjects are only seeded with dev authentication')
      const usersPage = new OrganisationUsersPage(page)

      // The subjects are in the Champion's organisation, which a Super user can also manage.
      await usersPage.openManageUserAccess(
        devRoleOrganisationIds.Champion,
        championSubjects[subject],
      )

      await usersPage.expectAccessActions(actions[managingRole(userRole)])
    })
  }

  /* The users table links to Edit when any of change permissions, deactivate or remove is allowed,
   * so a deactivated user's row has Edit for a Super user (who can remove them) but not for a
   * Champion. */
  const rowLinks: { subject: ChampionSubject; links: Record<'Super' | 'Champion', string[]> }[] = [
    { subject: 'readonly', links: { Super: ['Edit'], Champion: ['Edit'] } },
    { subject: 'champion', links: { Super: ['Edit'], Champion: ['Edit'] } },
    { subject: 'deactivated', links: { Super: ['Reactivate', 'Edit'], Champion: ['Reactivate'] } },
  ]

  for (const { subject, links } of rowLinks) {
    test(`links the ${subject} subject's row to the right actions`, async ({ page, userRole }) => {
      test.skip(!hasSeededSubjects(), 'The fixed subjects are only seeded with dev authentication')
      const usersPage = new OrganisationUsersPage(page)
      await usersPage.goto(devRoleOrganisationIds.Champion)

      await usersPage.expectRowLinks(championSubjects[subject], links[managingRole(userRole)])
    })
  }

  test('offers no actions on your own access', async ({ page, organisationId }) => {
    const response = await page.request.get('/backend-api/users/me')
    expect(response.ok(), 'Loading the current user').toBe(true)
    const { userId } = (await response.json()) as { userId: number }

    await page.goto(`/portal/organisations/${organisationId}/manage-user-access/${userId}`)

    await expect(
      page.getByRole('heading', { name: 'Manage users in your organisation' }),
    ).toBeVisible()
    await expect(page.getByText('You cannot manage your own access from this page.')).toBeVisible()
    await expect(page.getByRole('radio')).toHaveCount(0)
  })
})
