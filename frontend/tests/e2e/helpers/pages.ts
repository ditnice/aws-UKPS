import { expect, type Page } from '@playwright/test'

/** No user or registration request has this id, so a page for it shows what a page for one the
 * caller can't see should. */
export const missingId = 2_147_483_647

export async function currentUserId(page: Page): Promise<number> {
  const response = await page.request.get('/backend-api/users/me')
  expect(response.ok(), 'Loading the current user').toBe(true)
  return ((await response.json()) as { userId: number }).userId
}

export async function mainText(page: Page): Promise<string> {
  return (await page.getByRole('main').innerText()).trim()
}

/* ADR-004: a page for something the caller can't see must look the same as one for something
 * that doesn't exist, so it can't tell whether it exists. */
export async function expectSameAsMissing(page: Page, url: (id: number) => string, id: number) {
  await page.goto(url(id))
  const existingPage = await mainText(page)

  await page.goto(url(missingId))
  expect(await mainText(page)).toBe(existingPage)
}
