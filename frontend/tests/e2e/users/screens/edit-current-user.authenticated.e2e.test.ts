import { expect, test } from '../../fixtures/test'
import { allRoles, forRoles } from '../../helpers/roles'
import { MePage } from '../../pages/me.page'

test.describe('Edit user details pages', { tag: forRoles(allRoles) }, () => {
  test.describe('Edit user full name screen', () => {
    test.describe('without saving', () => {
      test.use({ readOnly: true })
      test('loads the form', async ({ page }) => {
        const me = new MePage(page)
        await me.goto()
        const edit = await me.changeDetail('fullName')

        await expect(edit.heading).toBeVisible()
        await expect(edit.input).toBeVisible()
        await expect(edit.save).toBeVisible()
      })
      test('shows errors for empty fields', async ({ page }) => {
        const me = new MePage(page)
        await me.goto()
        const edit = await me.changeDetail('fullName')

        await edit.saveValue('')

        await edit.expectInvalid('Enter your full name')
      })
    })
  })

  test.describe('Edit user contact number screen', () => {
    test.describe('without saving', () => {
      test.use({ readOnly: true })
      test('loads the form', async ({ page }) => {
        const me = new MePage(page)
        await me.goto()
        const edit = await me.changeDetail('contactNumber')

        await expect(edit.heading).toBeVisible()
        await expect(edit.input).toBeVisible()
        await expect(edit.save).toBeVisible()
      })
      test('shows errors for empty fields', async ({ page }) => {
        const me = new MePage(page)
        await me.goto()
        const edit = await me.changeDetail('contactNumber')

        await edit.saveValue('')

        await edit.expectInvalid('Enter your contact number')
      })
    })
  })

  test.describe('Edit user email screen', () => {
    test.describe('without saving', () => {
      test.use({ readOnly: true })
      test('loads the form', async ({ page }) => {
        const me = new MePage(page)
        await me.goto()
        const edit = await me.changeEmail()

        await expect(edit.heading).toBeVisible()
        await expect(edit.input).toBeVisible()
        await expect(edit.submit).toBeVisible()
      })
      test('shows errors for empty fields', async ({ page }) => {
        const me = new MePage(page)
        await me.goto()
        const edit = await me.changeEmail()

        await edit.submitEmail('')

        await edit.expectInvalid('Enter your work email address')
      })
    })
  })
})
