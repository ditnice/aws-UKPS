import { expect, test } from '../fixtures/test'
import { forRoles } from '../helpers/roles'
import { mutatesDataTag } from '../helpers/test-environment'

// Super users cannot create records, so this runs as the roles that can.
test.describe(
  'Success banners after discarding a record',
  { tag: forRoles(['Standard', 'Champion'], mutatesDataTag) },
  () => {
    test('are not shown again after opening a record and going Back, or after a reload', async ({
      page,
      organisationId,
    }) => {
      const recordsPath = `/portal/organisations/${organisationId}/records`
      const discardedBanner = page.getByRole('heading', { name: 'Record successfully discarded' })
      const recordsTable = page.getByRole('table', { name: 'Organisation Records' })

      // Create a draft, so there is a record this test can discard without touching existing data.
      await page.goto(`${recordsPath}/create`)
      await page.getByLabel('Company code').fill('E2E-BANNER')
      await page.getByLabel('Generic name').fill('e2e-banner-substance')
      await page.getByLabel('Record title').fill(`E2E banner test ${Date.now()}`)
      await page.getByRole('button', { name: 'Save and continue' }).click()

      await expect(page).toHaveURL(/\/portal\/records\/\d+\/task-list/)
      await expect(page.getByRole('heading', { name: 'Draft record saved' })).toBeVisible()
      await expect(page).not.toHaveURL(/action=/)
      const recordId = /\/portal\/records\/(\d+)\/task-list/.exec(new URL(page.url()).pathname)?.[1]
      if (!recordId) {
        throw new Error(`Could not read the new record's ID from ${page.url()}`)
      }

      // Discard it: the records list shows the banner once, and the parameter leaves the URL.
      await page.goto(`${recordsPath}/${recordId}/discard`)
      await page.getByRole('button', { name: 'Confirm' }).click()

      await expect(page).toHaveURL(new RegExp(`${recordsPath}(\\?|$)`))
      await expect(discardedBanner).toBeVisible()
      await expect(page).not.toHaveURL(/action=/)

      // Open a record, then go Back: the page is restored without the banner.
      const viewLinks = recordsTable.getByRole('link', { name: /^View/ })
      if ((await viewLinks.count()) === 0) {
        test.skip(true, `Organisation ${organisationId} has no other record to open`)
        return
      }
      await viewLinks.first().click()
      await expect(page).toHaveURL(new RegExp(`${recordsPath}/\\d+`))

      await page.goBack()

      await expect(page).toHaveURL(new RegExp(`${recordsPath}(\\?|$)`))
      await expect(recordsTable).toBeVisible()
      await expect(discardedBanner).toBeHidden()

      // A reload does not bring it back either.
      await page.reload()

      await expect(recordsTable).toBeVisible()
      await expect(discardedBanner).toBeHidden()
    })
  },
)

test.describe(
  'Discarding a record without saving',
  { tag: forRoles(['Standard', 'Champion']) },
  () => {
    test.use({ readOnly: true })

    test('opens discard and cancels', async ({ page, organisationId }) => {
      await page.goto(`/portal/organisations/${organisationId}/records`)
      const viewLinks = page
        .getByRole('table', { name: 'Organisation Records' })
        .getByRole('link', { name: /^View/ })
      const recordPaths = await viewLinks.evaluateAll((links) =>
        links.map((link) => link.getAttribute('href') ?? ''),
      )

      // Not every record can be discarded, so open them until one offers it.
      const discardLink = page.getByRole('link', { name: 'Discard record' })
      let recordPath: string | undefined
      for (const path of recordPaths) {
        await page.goto(path)
        if (await discardLink.isVisible()) {
          recordPath = path
          break
        }
      }
      if (!recordPath) {
        test.skip(true, `Organisation ${organisationId} has no record that can be discarded`)
        return
      }

      await discardLink.click()

      const heading = page.getByRole('heading', { name: 'Confirm decision to discard record' })
      await expect(heading).toBeVisible()
      await page.getByRole('button', { name: 'Cancel' }).click()

      await expect(heading).toBeHidden()
      await expect(page).toHaveURL(new RegExp(`${recordPath.split('?')[0]}`))
      await expect(discardLink).toBeVisible()
    })
  },
)
