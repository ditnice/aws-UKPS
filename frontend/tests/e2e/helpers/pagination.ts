import { expect, Page } from '@playwright/test'

export async function goToNextPage(page: Page): Promise<boolean> {
  const nextPageLink = page.getByRole('link', {
    name: 'Next page',
    exact: true,
  })
  if (!(await nextPageLink.isVisible()) || !(await nextPageLink.isEnabled())) {
    return false
  }
  const nextPageHref = await nextPageLink.getAttribute('href')

  if (!nextPageHref) {
    throw new Error('Next page link has no href')
  }
  const nextPageUrl = new URL(nextPageHref, page.url()).toString()

  await page.goto(nextPageUrl)

  await expect(page).toHaveURL(nextPageUrl)
  await expect(page.getByText('Organisation details')).toBeVisible()
  await expect(page.getByText('Loading users...')).toBeHidden()

  return true
}
