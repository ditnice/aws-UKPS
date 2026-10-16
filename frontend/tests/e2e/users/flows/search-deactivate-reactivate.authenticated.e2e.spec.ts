import { goToNextPage } from 'tests/e2e/helpers/pagination'

import { expect, test } from '../../fixtures/test'
import { requireEnvironmentVariable } from '../../helpers/test-environment'

test.describe('Searching, activating and deactivating a user', () => {
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
  })

  test('reactivate a deactive user', async ({ page }) => {
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')

    await page.goto(`/portal/organisations/${organisationId}`)
    await expect(page.getByText('Organisation details')).toBeVisible()
    const deactivatedUserRow = page
      .getByRole('row')
      .filter({
        has: page.getByRole('cell', {
          name: 'Deactivated',
          exact: true,
        }),
      })
      .filter({
        has: page.getByRole('link', {
          name: 'Reactivate',
          exact: true,
        }),
      })

    let userEmail: string | undefined
    await expect(page.getByText('Loading users', { exact: false })).toBeHidden({ timeout: 15000 })
    while (!userEmail) {
      const matchingRows = await deactivatedUserRow.count()
      for (let i = 0; i < matchingRows; i++) {
        const row = deactivatedUserRow.nth(i)
        const reactivateLink = row.getByRole('link', {
          name: 'Reactivate',
          exact: true,
        })
        if (await reactivateLink.isVisible()) {
          userEmail = (await row.getByRole('cell').first().innerText()).trim()
          await reactivateLink.click()
          break
        }
      }
      if (userEmail) break
      if (!(await goToNextPage(page))) {
        throw new Error('No champion user with a visible Edit link found')
      }
    }

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

  test('search for a user', async ({ page }) => {
    const userEmail = requireEnvironmentVariable('E2E_USER_EMAIL')
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')
    await page.goto(`/portal/organisations/${organisationId}`)

    await page.getByRole('textbox', { name: 'Filter users' }).fill(userEmail)
    await page.getByRole('button', { name: 'Apply filter', exact: true }).click()

    await expect(page.getByText(userEmail, { exact: true })).toBeVisible()
  })
})
