import { expect, test } from '../fixtures/test'
import { currentUserId, mainText, missingId } from '../helpers/pages'
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

/* The pages a champion or super user reviews a registration request from. Nothing seeds
 * registration requests, so request 1 may or may not exist: either way, the page must look the
 * same as one for a request that doesn't. */
const registrationRequestPages = [
  { action: 'approve', button: 'Approve' },
  { action: 'reject', button: 'Reject' },
] as const
const someRequestId = 1

test.describe('Managing users as a standard user', { tag: forRoles(['Standard']) }, () => {
  test.use({ readOnly: true })

  for (const { path, control, name } of userManagementPages) {
    test(`can't use /${path}, and can't tell whether the user exists`, async ({
      page,
      organisationId,
    }) => {
      // The standard user's own id: a user that exists in their organisation.
      const existingUserId = await currentUserId(page)
      const url = (userId: number) =>
        `/portal/organisations/${organisationId}/${path.replace('{userId}', String(userId))}`

      await page.goto(url(existingUserId))
      await expect(page.getByRole(control, { name })).toHaveCount(0)
      const existingUserPage = await mainText(page)

      await page.goto(url(missingId))
      await expect(page.getByRole(control, { name })).toHaveCount(0)
      // ADR-004: a user the caller can't see looks the same as one that doesn't exist.
      expect(await mainText(page)).toBe(existingUserPage)
    })
  }

  for (const { action, button } of registrationRequestPages) {
    test(`can't ${action} a registration request, and can't tell whether it exists`, async ({
      page,
      organisationId,
    }) => {
      const url = (requestId: number) =>
        `/portal/organisations/${organisationId}/registration-requests/${requestId}/${action}`

      await page.goto(url(someRequestId))
      await expect(page.getByText(`You are about to ${action}`)).toHaveCount(0)
      await expect(page.getByRole('button', { name: button, exact: true })).toHaveCount(0)
      const somePage = await mainText(page)

      await page.goto(url(missingId))
      await expect(page.getByRole('button', { name: button, exact: true })).toHaveCount(0)
      expect(await mainText(page)).toBe(somePage)
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
