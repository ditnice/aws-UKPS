import { expect, test } from '../fixtures/test'
import { currentUserId, expectConcealedApiResource, expectSameAsMissing } from '../helpers/pages'
import { forRoles } from '../helpers/roles'

/* The pages a champion or super user manages another user from, by their path after
 * /portal/organisations/<id>/, and a control that only shows when the page loads the user. */
const userManagementPages = [
  { path: 'manage-user-access/{userId}', control: 'radio', name: 'Deactivate user' },
  {
    path: 'manage-user-access/{userId}/change-permissions',
    control: 'button',
    name: /Make .* user/,
  },
  { path: 'manage-user-access/{userId}/remove', control: 'button', name: 'Remove user' },
  { path: 'users/{userId}/deactivate', control: 'button', name: 'Deactivate User' },
  { path: 'users/{userId}/reactivate', control: 'button', name: 'Reactivate user' },
] as const

/* Both pages use a pending request whose existence is verified by a separate Super session. */
const registrationRequestPages = [
  { action: 'approve', button: 'Approve' },
  { action: 'reject', button: 'Reject' },
] as const

test.describe('Managing users as a standard user', { tag: forRoles(['Standard']) }, () => {
  test.use({ readOnly: true })

  for (const { path, control, name } of userManagementPages) {
    test(`can't use /${path}, and can't tell whether the user exists`, async ({
      page,
      organisationId,
      protectedUser,
    }) => {
      // A real subject in the addressed organisation, not the caller: self-protection must not
      // accidentally satisfy the test in place of the role authorisation check.
      const existingUserId = protectedUser.userId
      expect(existingUserId).not.toBe(await currentUserId(page))
      const url = (userId: number) =>
        `/portal/organisations/${organisationId}/${path.replace('{userId}', String(userId))}`

      await expectConcealedApiResource(
        page,
        (id) => `/backend-api/users/${id}/organisations/${organisationId}`,
        existingUserId,
      )
      await expectSameAsMissing(page, url, existingUserId, async () => {
        await expect(page.getByRole(control, { name })).toHaveCount(0)
        await expect(page.getByText(protectedUser.workEmail, { exact: false })).toHaveCount(0)
      })
    })
  }

  for (const { action, button } of registrationRequestPages) {
    test(`can't ${action} a registration request, and can't tell whether it exists`, async ({
      page,
      organisationId,
      protectedRequest,
    }) => {
      const url = (requestId: number) =>
        `/portal/organisations/${organisationId}/registration-requests/${requestId}/${action}`

      await expectConcealedApiResource(
        page,
        (id) => `/backend-api/organisations/${organisationId}/membership-requests/${id}`,
        protectedRequest.id,
      )
      await expectSameAsMissing(page, url, protectedRequest.id, async () => {
        await expect(page.getByText(`You are about to ${action}`)).toHaveCount(0)
        await expect(page.getByRole('button', { name: button, exact: true })).toHaveCount(0)
        await expect(page.getByText(protectedRequest.workEmail, { exact: false })).toHaveCount(0)
      })
    })
  }

  test("isn't offered the invite form", async ({ page, organisationId }) => {
    await page.goto(`/portal/organisations/${organisationId}/onboard-user`)

    await expect(
      page.getByRole('heading', { name: 'User is not permitted to view this page' }),
    ).toBeVisible()
    await expect(page.getByLabel('Full name')).toHaveCount(0)
    await expect(page.getByRole('button', { name: 'Send invite' })).toHaveCount(0)
  })
})
