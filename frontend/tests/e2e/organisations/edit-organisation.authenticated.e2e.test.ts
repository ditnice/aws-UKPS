import { type Page } from '@playwright/test'

import { expect, test } from '../fixtures/test'
import { forRoles } from '../helpers/roles'
import { mutatesDataTag, visualTag } from '../helpers/test-environment'

type Field = {
  label: string
  /** What the data-changing tests save. */
  value: string
  /** Never saved, so the read-only test can tell a cancel that saved from one that didn't. */
  unsavedValue: string
  emptyError: string
}

const fields: Field[] = [
  {
    label: 'Organisation name',
    value: 'Test organisation',
    unsavedValue: 'Unsaved organisation',
    emptyError: 'Enter the organisation name',
  },
  {
    label: 'Organisation address',
    value: 'The Old Bank Old Market Place Altrincham Cheshire WA14 4PA',
    unsavedValue: 'Unsaved address',
    emptyError: 'Enter organisation address',
  },
  {
    label: 'Head office email address',
    value: 'test@example.com',
    unsavedValue: 'unsaved@example.com',
    emptyError: 'Enter an email address',
  },
  {
    label: 'Head office phone number',
    value: '07123456789',
    unsavedValue: '07000000000',
    emptyError: 'Enter the head office phone number',
  },
]

async function gotoEditPage(page: Page, organisationId: string) {
  await page.goto(`/portal/organisations/${organisationId}/edit`)
  await expect(page.getByRole('heading', { name: 'Edit organisation details' })).toBeVisible()
}

// In file order on one worker, so no test reads the organisation while another changes it, and
// no stale form overwrites another test's change, as each submit saves the whole form.
test.describe.configure({ mode: 'default' })

test(
  'shows the edit organisation page',
  { tag: forRoles(['Super'], visualTag) },
  async ({ page, organisationId }) => {
    await gotoEditPage(page, organisationId)
    // The values depend on which tests in this file have run.
    await expect(page).toHaveScreenshot('edit-organisation.png', {
      fullPage: true,
      mask: [page.getByRole('textbox')],
    })
  },
)

test.describe(
  'Editing organisation details',
  { tag: forRoles(['Super', 'Champion'], mutatesDataTag) },
  () => {
    for (const { label, value } of fields) {
      test(`edits the ${label.toLowerCase()}`, async ({ page, organisationId }) => {
        await gotoEditPage(page, organisationId)

        await page.getByLabel(label).fill(value)
        await page.getByRole('button', { name: 'Submit' }).click()

        await expect(page.getByText('Organisation Details Updated')).toBeVisible()
        // The details page shows the name as its heading, and the other fields as text.
        const savedValue =
          label === 'Organisation name'
            ? page.getByRole('heading', { name: value })
            : page.getByText(value)
        await expect(savedValue).toBeVisible()
      })
    }
  },
)

test.describe(
  'Editing organisation details without saving',
  { tag: forRoles(['Super', 'Champion']) },
  () => {
    test.use({ readOnly: true })

    test('edits the details and cancels', async ({ page, organisationId }) => {
      const editHeading = page.getByRole('heading', { name: 'Edit organisation details' })
      await page.goto(`/portal/organisations/${organisationId}`)
      await page.getByRole('link', { name: 'Edit details' }).click()
      await expect(editHeading).toBeVisible()

      const savedValues = await Promise.all(
        fields.map(({ label }) => page.getByLabel(label).inputValue()),
      )
      for (const { label, unsavedValue } of fields) {
        await page.getByLabel(label).fill(unsavedValue)
      }
      await page.getByRole('button', { name: 'Cancel' }).click()
      await expect(page).toHaveURL(`/portal/organisations/${organisationId}`)

      await gotoEditPage(page, organisationId)
      for (const [index, { label }] of fields.entries()) {
        await expect(page.getByLabel(label)).toHaveValue(savedValues[index])
      }
    })

    test('shows errors for empty fields', async ({ page, organisationId }) => {
      await gotoEditPage(page, organisationId)

      for (const { label } of fields) {
        await page.getByLabel(label).fill('')
      }
      await page.getByRole('button', { name: 'Submit' }).click()

      for (const { label, emptyError } of fields) {
        // The shared Textarea doesn't set aria-invalid, so only the address skips that check.
        if (label !== 'Organisation address') {
          await expect(page.getByLabel(label)).toHaveAttribute('aria-invalid', 'true')
        }
        await expect(page.getByText(emptyError)).toBeVisible()
      }
    })
  },
)

test.describe(
  'Editing organisation details as a standard user',
  { tag: forRoles(['Standard']) },
  () => {
    test.use({ readOnly: true })

    test("doesn't show the edit form", async ({ page, organisationId }) => {
      await page.goto(`/portal/organisations/${organisationId}/edit`)

      await expect(
        page.getByRole('heading', { name: 'User is not permitted to view this page' }),
      ).toBeVisible()
      await expect(page.getByRole('heading', { name: 'Edit organisation details' })).toBeHidden()
      await expect(page.getByRole('button', { name: 'Submit' })).toBeHidden()
    })
  },
)
