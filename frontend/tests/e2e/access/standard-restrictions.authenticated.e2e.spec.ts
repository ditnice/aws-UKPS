import { type Page } from '@playwright/test'

import { expect, test } from '../fixtures/test'
import { forRoles } from '../helpers/roles'

// No user has this id, so a page for it shows what a page for a user the caller can't see should.
const missingUserId = 2_147_483_647

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

async function currentUserId(page: Page): Promise<number> {
  const response = await page.request.get('/backend-api/users/me')
  expect(response.ok(), 'Loading the current user').toBe(true)
  return ((await response.json()) as { userId: number }).userId
}

async function mainText(page: Page): Promise<string> {
  return (await page.getByRole('main').innerText()).trim()
}

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

      await page.goto(url(missingUserId))
      await expect(page.getByRole(control, { name })).toHaveCount(0)
      // ADR-004: a user the caller can't see looks the same as one that doesn't exist.
      expect(await mainText(page)).toBe(existingUserPage)
    })
  }
})
