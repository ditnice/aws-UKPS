import { goToNextPage } from 'tests/e2e/helpers/pagination'

import { expect, test } from '../../fixtures/test'
import { requireEnvironmentVariable } from '../../helpers/test-environment'

test.describe('Promoting and demoting a user', () => {
  test('promoting a standard user', async ({ page }) => {
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')

    await page.goto(`/portal/organisations/${organisationId}`)
    await expect(page.getByText('Organisation details')).toBeVisible()
    const activeUserRow = page
      .getByRole('row')
      .filter({
        has: page.getByRole('cell', {
          name: 'Standard user',
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
    await expect(page.getByText('Loading users')).toBeHidden()
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
        throw new Error('No champion user with a visible Edit link found')
      }
    }
    await expect(page.getByRole('heading', { name: "Manage user's access" })).toBeVisible()
    await page.getByText('Change user permissions', { exact: true }).click()
    await page.getByRole('button', { name: 'Continue' }).click()

    await expect(page.getByRole('heading', { name: 'Change user permissions' })).toBeVisible()
    await page.getByRole('button', { name: 'Make champion user' }).click()

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
        name: 'Champion user',
        exact: true,
      }),
    ).toBeVisible()
  })

  test('demoting a champion user', async ({ page }) => {
    const organisationId = requireEnvironmentVariable('E2E_ORGANISATION_ID')

    await page.goto(`/portal/organisations/${organisationId}`)
    await expect(page.getByText('Organisation details')).toBeVisible()

    const activeUserRow = page
      .getByRole('row')
      .filter({
        has: page.getByRole('cell', {
          name: 'Champion user',
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
    await expect(page.getByText('Loading users...')).toBeHidden()
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
        throw new Error('No champion user with a visible Edit link found')
      }
    }

    await expect(page.getByRole('heading', { name: "Manage user's access" })).toBeVisible()
    await page.getByText('Change user permissions', { exact: true }).click()
    await page.getByRole('button', { name: 'Continue' }).click()

    await expect(page.getByRole('heading', { name: 'Change user permissions' })).toBeVisible()
    await page.getByRole('button', { name: 'Make standard user' }).click()

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
        name: 'Standard user',
        exact: true,
      }),
    ).toBeVisible()
  })
})
