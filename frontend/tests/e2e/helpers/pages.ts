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
 * that doesn't exist, so it can't tell whether it exists. expectNothingRevealed runs while the
 * existing resource's page is open, as this helper finishes on the missing resource's page. */
export async function expectSameAsMissing(
  page: Page,
  url: (id: number) => string,
  id: number,
  expectNothingRevealed: () => Promise<void>,
) {
  await page.goto(url(id))
  await expect(page.getByRole('heading', { name: 'Page not found', exact: true })).toBeVisible()
  await expectNothingRevealed()
  const existingPage = await mainText(page)

  await page.goto(url(missingId))
  await expect(page.getByRole('heading', { name: 'Page not found', exact: true })).toBeVisible()
  expect(await mainText(page)).toBe(existingPage)
}

/** Verify HTTP-boundary concealment as well as the page's neutral presentation. Trace IDs are
 * request-specific diagnostics, not resource information. */
export async function expectConcealedApiResource(
  page: Page,
  url: (id: number) => string,
  existingId: number,
) {
  const existing = await page.request.get(url(existingId))
  const missing = await page.request.get(url(missingId))
  expect(existing.status()).toBe(404)
  expect(missing.status()).toBe(404)
  const { traceId: _existingTrace, ...existingBody } = await existing.json()
  const { traceId: _missingTrace, ...missingBody } = await missing.json()
  expect(existingBody).toEqual(missingBody)
}
