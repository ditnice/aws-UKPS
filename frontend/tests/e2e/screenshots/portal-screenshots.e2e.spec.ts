import { expect, test } from '../fixtures/test'

// Only known, read-only screen routes are eligible. Ignore Components, logout,
// pagination/filter variants, external links, and controls that change data.
const screenRoutes = [
  /^\/portal$/,
  /^\/portal\/notfound$/,
  /^\/portal\/user\/me(?:\/edit-details)?$/,
  /^\/portal\/organisations\/\d+(?:\/edit|\/onboard-user)?$/,
  /^\/portal\/organisations\/\d+\/records(?:\/create|\/\d+)?$/,
  /^\/portal\/organisations\/\d+\/manage-user-access\/\d+(?:\/change-permissions|\/remove)?$/,
  /^\/portal\/organisations\/\d+\/users\/\d+\/(?:deactivate|reactivate)$/,
  /^\/portal\/organisations\/\d+\/registration-requests\/\d+\/(?:approve|reject)$/,
]

function screenKey(url: URL) {
  return `${url.pathname.replace(/\/\d+(?=\/|$)/g, '/id')}${url.search}`
}

test.use({ viewport: { width: 1440, height: 1000 } })
test.skip(({ headless }) => headless, 'Manual sign-in requires --headed (or --ui --headed).')

test('screenshots the available portal screens without changing data', async ({
  page,
}, testInfo) => {
  // Allow as much time as needed for this interactive, manually started tour.
  test.setTimeout(0)

  await page.goto('/portal')
  // Sign in normally in this browser, then click Resume in Playwright Inspector.
  // Credentials and browser session state are not saved by this test.
  await page.pause()
  await page.goto('/portal')
  await expect(page.getByRole('heading', { name: 'Your dashboard', exact: true })).toBeVisible()

  const origin = new URL(page.url()).origin
  const pending = [`${origin}/portal`]
  const discovered = new Set([screenKey(new URL('/portal', origin))])
  const captures: { url: string; screenshot: string }[] = []

  function enqueue(href: string) {
    const url = new URL(href, origin)
    if (url.origin !== origin || !screenRoutes.some((route) => route.test(url.pathname))) return

    // Retain the record type needed by record details, but do not crawl every
    // pagination, sorting, filter, fragment, or success-message combination.
    const recordType = url.searchParams.get('recordType')
    url.hash = ''
    url.search = ''
    if (/\/records\/\d+$/.test(url.pathname) && recordType) {
      url.searchParams.set('recordType', recordType)
    }

    // Different organisations, users, requests, and records share the same
    // screen. Visit only the first example of each route shape and record type.
    const key = screenKey(url)
    if (!discovered.has(key)) {
      discovered.add(key)
      pending.push(url.href)
    }
  }

  for (const url of pending) {
    await test.step(`Capture ${new URL(url).pathname}`, async () => {
      await page.goto(url)
      await expect(page).toHaveURL(url)
      const main = page.getByRole('main')
      await expect(main).toBeVisible()
      await expect(main.getByRole('heading', { level: 1 })).toBeVisible()
      await expect(main.getByText(/^Loading(?: users| records)?\.\.\.$/)).toHaveCount(0)
      await page.evaluate(async () => {
        await document.fonts.ready
        await Promise.all(
          Array.from(document.images, (image) => image.decode().catch(() => undefined)),
        )
      })

      const screenName = screenKey(new URL(url))
        .slice(1)
        .replace(/[^\w-]/g, '-')
      // Keep global.scss lines 31–49 commented out. Screenshot styles apply
      // only during capture and leave the page unchanged for the next version.
      for (const variant of ['without-spacing', 'with-spacing']) {
        const screenshot = `screenshots/${variant}/${screenName}.png`
        await page.screenshot({
          path: screenshot,
          fullPage: true,
          animations: 'disabled',
          style:
            variant === 'with-spacing' ? 'p, h2, input, dl, select { margin-bottom: 2rem; }' : '',
        })
        await testInfo.attach(`${screenName}-${variant}`, {
          path: screenshot,
          contentType: 'image/png',
        })
        captures.push({ url, screenshot })
      }

      // Link-styled buttons are anchors too. Only follow visible navigation;
      // do not click submit, export, sign-out, or record-management buttons.
      for (const link of await page.getByRole('link').all()) {
        if (!(await link.isVisible())) continue
        const href = await link.getAttribute('href')
        if (href) enqueue(new URL(href, page.url()).href)
      }

      if (/\/manage-user-access\/\d+$/.test(new URL(url).pathname)) {
        const actions = await main
          .getByRole('radio')
          .evaluateAll((radios) => radios.map((radio) => (radio as HTMLInputElement).value))

        // These are the destinations used by ManageUserAccessActions. Queue
        // each available option directly instead of revisiting this menu for
        // every Continue click. Never submit a destination form.
        const destinations: Record<string, string> = {
          EditUserRole: `${url}/change-permissions`,
          RemoveUser: `${url}/remove`,
          DeactivateMembership: `${url.replace('/manage-user-access/', '/users/')}/deactivate`,
          ReactivateMembership: `${url.replace('/manage-user-access/', '/users/')}/reactivate`,
        }
        for (const action of actions) {
          const destination = destinations[action]
          if (destination) enqueue(destination)
        }
      }
    })
  }

  await testInfo.attach('captured-screens', {
    body: JSON.stringify(captures, null, 2),
    contentType: 'application/json',
  })
})
