import AxeBuilder from '@axe-core/playwright'
import { expect, test as base } from '@playwright/test'

type AccessibilityOptions = {
  exclude?: string[]
  include?: string
}

type Fixtures = {
  checkAccessibility: (options?: AccessibilityOptions) => Promise<void>
}

export const test = base.extend<Fixtures>({
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
