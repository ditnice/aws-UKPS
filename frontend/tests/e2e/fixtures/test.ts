import AxeBuilder from '@axe-core/playwright'
import { expect, test as base } from '@playwright/test'

type AccessibilityOptions = {
  exclude?: string[]
  include?: string
}

type Options = {
  /** Aborts and fails on any write request the browser sends to the app, so a read-only test
   * can't change data. It routes the browser context only: calls through `page.request` or the
   * `request` fixture aren't checked, so a read-only test mustn't use them to write. */
  readOnly: boolean
}

type Fixtures = {
  checkAccessibility: (options?: AccessibilityOptions) => Promise<void>
  readOnlyGuard: void
}

const readMethods = new Set(['GET', 'HEAD', 'OPTIONS'])
// Renewing the session changes no data, and the browser does it whenever a token expires.
const allowedWritePaths = new Set(['/backend-api/auth/refresh'])

export const test = base.extend<Options & Fixtures>({
  readOnly: [false, { option: true }],

  readOnlyGuard: [
    async ({ baseURL, context, readOnly }, fixtureUse) => {
      if (!readOnly) {
        await fixtureUse()
        return
      }

      // Server actions are POSTs to the app, so aborting them stops the backend being called.
      const appOrigin = baseURL ? new URL(baseURL).origin : undefined
      const blocked: string[] = []
      await context.route('**/*', async (route) => {
        const request = route.request()
        const url = new URL(request.url())
        if (
          readMethods.has(request.method()) ||
          url.origin !== appOrigin ||
          allowedWritePaths.has(url.pathname)
        ) {
          await route.fallback()
          return
        }
        blocked.push(`${request.method()} ${url.pathname}`)
        await route.abort('blockedbyclient')
      })

      await fixtureUse()

      expect(blocked, 'A read-only test sent a write request').toEqual([])
    },
    { auto: true },
  ],

  checkAccessibility: async ({ page }, fixtureUse, testInfo) => {
    await fixtureUse(async ({ exclude = [], include } = {}) => {
      let builder = new AxeBuilder({ page }).withTags([
        'wcag2a',
        'wcag2aa',
        'wcag21a',
        'wcag21aa',
        'wcag22aa',
      ])

      if (include) builder = builder.include(include)
      for (const selector of exclude) builder = builder.exclude(selector)

      const results = await builder.analyze()
      await testInfo.attach('accessibility-results', {
        body: Buffer.from(JSON.stringify(results.violations, null, 2)),
        contentType: 'application/json',
      })

      const violations = results.violations.map(({ description, id, impact, nodes }) => ({
        description,
        id,
        impact,
        nodes: nodes.map(({ html, target }) => ({ html, target })),
      }))

      expect(violations, JSON.stringify(violations, null, 2)).toEqual([])
    })
  },
})

export { expect } from '@playwright/test'
