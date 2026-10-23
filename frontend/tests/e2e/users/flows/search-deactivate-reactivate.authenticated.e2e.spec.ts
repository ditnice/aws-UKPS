import { expect, test } from '../../fixtures/test'
import { goToNextPage } from '../../helpers/pagination'
import { mutatesDataTag, requireEnvironmentVariable } from '../../helpers/test-environment'

test.describe('Searching, activating and deactivating a user', { tag: mutatesDataTag }, () => {
  // 'reactivate a deactive user' reactivates the user that 'deactivate an active user' deactivates.
  test.describe.configure({ mode: 'serial' })
  let deactivatedUserEmail: string | undefined

  test('deactivate an active user', async ({ page }) => {
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')

    await page.goto(`/portal/organisations/${organisationId}`)
    await expect(page.getByText('Organisation details')).toBeVisible()
    const activeUserRow = page
      .getByRole('row')
      .filter({
        has: page.getByRole('cell', {
          name: 'Active',
          exact: true,
        }),
      })
      .filter({
        has: page.getByRole('link', {
          name: 'Edit',
          exact: true,
        }),
      })

    let userEmail: string | undefined
    await expect(page.getByText('Loading users', { exact: false })).toBeHidden({ timeout: 15000 })

    while (!userEmail) {
      const matchingRows = await activeUserRow.count()
      for (let i = 0; i < matchingRows; i++) {
        const row = activeUserRow.nth(i)
        const editLink = row.getByRole('link', {
          name: 'Edit',
          exact: true,
        })

        if (await editLink.isVisible()) {
          userEmail = (await row.getByRole('cell').first().innerText()).trim()
          await editLink.click()
          break
        }
      }

      if (userEmail) break
      if (!(await goToNextPage(page))) {
        throw new Error('No active user found to deactivate')
      }
    }

    await expect(page.getByRole('heading', { name: "Manage user's access" })).toBeVisible()
    await page.getByText('Deactivate user', { exact: true }).click()
    await page.getByRole('button', { name: 'Continue' }).click()

    await expect(page.getByRole('heading', { name: 'Deactivate user' })).toBeVisible()
    await page.getByRole('button', { name: 'Deactivate User' }).click()

    await expect(page.getByText('Organisation details')).toBeVisible()
    await page.getByRole('textbox', { name: 'Filter users' }).fill(userEmail)

    await page
      .getByRole('button', {
        name: 'Apply filter',
        exact: true,
      })
      .click()
    const userRow = page.getByRole('row').filter({
      has: page.getByRole('cell', {
        name: userEmail,
        exact: true,
      }),
    })
    await expect(userRow).toBeVisible()
    await expect(
      userRow.getByRole('cell', {
        name: 'Deactivated',
        exact: true,
      }),
    ).toBeVisible()
    deactivatedUserEmail = userEmail
  })

  test('reactivate a deactive user', async ({ page }) => {
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
    const userEmail = deactivatedUserEmail
    if (!userEmail) {
      throw new Error('No user was deactivated by the previous test')
    }

    await page.goto(`/portal/organisations/${organisationId}`)
    await expect(page.getByText('Organisation details')).toBeVisible()
    await page.getByRole('textbox', { name: 'Filter users' }).fill(userEmail)
    await page
      .getByRole('button', {
        name: 'Apply filter',
        exact: true,
      })
      .click()
    await page
      .getByRole('row')
      .filter({
        has: page.getByRole('cell', {
          name: userEmail,
          exact: true,
        }),
      })
      .getByRole('link', { name: 'Reactivate', exact: true })
      .click()

    await expect(page.getByRole('heading', { name: 'Reactivate user' })).toBeVisible()
    await page.getByRole('button', { name: 'Reactivate user' }).click()

    await expect(page.getByText('Organisation details')).toBeVisible()
    await page.getByRole('textbox', { name: 'Filter users' }).fill(userEmail)

    await page
      .getByRole('button', {
        name: 'Apply filter',
        exact: true,
      })
      .click()
    const userRow = page.getByRole('row').filter({
      has: page.getByRole('cell', {
        name: userEmail,
        exact: true,
      }),
    })
    await expect(userRow).toBeVisible()
    await expect(
      userRow.getByRole('cell', {
        name: 'Active',
        exact: true,
      }),
    ).toBeVisible()
  })
})
