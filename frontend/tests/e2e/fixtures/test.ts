import AxeBuilder from '@axe-core/playwright'
import { expect, test as base, type APIRequestContext } from '@playwright/test'

import {
  accessSubjectEmail,
  verifiedRegistrationRequest,
  verifiedUser,
} from '../helpers/access-subjects'
import { devRoleCookieName, roleTag, type UserRole } from '../helpers/roles'
import { accessSubjects } from '../helpers/subjects'
import {
  authStatePath,
  hasRoleCredentials,
  isDevAuthMode,
  isLocalBaseURL,
  organisationIdFor,
  signedInEmail,
} from '../helpers/test-environment'

import type {
  RegisterUserDetailsDto,
  UserInformationDto,
} from '../../../src/client/generated/types.gen'

type AccessibilityOptions = {
  exclude?: string[]
  include?: string
}

export type TestOptions = {
  /** Aborts and fails on any write request the browser sends to the app, so a read-only test
   * can't change data. It routes the browser context only: calls through `page.request` or the
   * `request` fixture aren't checked, so a read-only test mustn't use them to write. */
  readOnly: boolean
  /** The role this run is signed in as. Each authenticated-<role> project sets it, and it's
   * undefined in the public projects. Read it to assert on content that differs by role. */
  userRole: UserRole | undefined
  /** Set by the authenticated-untagged project, which only runs authenticated tests that have
   * no role tag, so it fails them rather than letting them silently never run. */
  requireRoleTag: boolean
}

type Fixtures = {
  checkAccessibility: (options?: AccessibilityOptions) => Promise<void>
  readOnlyGuard: void
  roleAnnotation: void
  /** The organisation this run's role acts on (see organisationIdFor). */
  organisationId: string
  /** Independent Super session used only for GETs to verify protected subjects exist. */
  accessVerifier: APIRequestContext
  protectedUser: UserInformationDto
  foreignUser: UserInformationDto
  protectedRequest: RegisterUserDetailsDto
}

const readMethods = new Set(['GET', 'HEAD', 'OPTIONS'])
// Renewing the session changes no data, and the browser does it whenever a token expires.
const allowedWritePaths = new Set(['/backend-api/auth/refresh'])

export const test = base.extend<TestOptions & Fixtures>({
  readOnly: [false, { option: true }],

  userRole: [undefined, { option: true }],

  requireRoleTag: [false, { option: true }],

  // Shows each run's role in the reports, and fails a test that a project runs as a role the
  // test doesn't declare, which would mean the project's grep is wrong.
  roleAnnotation: [
    async ({ requireRoleTag, userRole }, fixtureUse, testInfo) => {
      if (requireRoleTag) {
        throw new Error(
          'Authenticated tests must declare the roles they run as: add `{ tag: forRoles([...]) }` ' +
            'from tests/e2e/helpers/roles.ts to the test or its describe.',
        )
      }
      if (userRole) {
        testInfo.annotations.push({ type: 'role', description: userRole })
        expect(
          testInfo.tags,
          `Running as ${userRole}, but the test isn't tagged ${roleTag(userRole)}`,
        ).toContain(roleTag(userRole))
      }
      await fixtureUse()
    },
    { auto: true },
  ],

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

  organisationId: async ({ userRole }, fixtureUse) => {
    const organisationId = userRole
      ? organisationIdFor(userRole)
      : process.env.E2E_ORGANISATION_ID?.trim()
    if (!organisationId) {
      throw new Error(
        `E2E_${userRole?.toUpperCase() ?? 'ROLE'}_ORGANISATION_ID or E2E_ORGANISATION_ID must be set to run authenticated Playwright tests.`,
      )
    }
    await fixtureUse(organisationId)
  },

  accessVerifier: async ({ playwright, baseURL }, fixtureUse) => {
    if (!isDevAuthMode() && (!hasRoleCredentials('Super') || !organisationIdFor('Super'))) {
      throw new Error(
        'Access-control checks need a configured Super account to verify existing subjects',
      )
    }
    const verifier = await playwright.request.newContext({
      baseURL,
      ignoreHTTPSErrors: isLocalBaseURL(baseURL),
      storageState: isDevAuthMode()
        ? {
            cookies: [
              {
                name: devRoleCookieName,
                value: 'Super',
                domain: new URL(baseURL!).hostname,
                path: '/',
                expires: -1,
                httpOnly: false,
                secure: false,
                sameSite: 'Lax',
              },
            ],
            origins: [],
          }
        : authStatePath('Super'),
    })
    try {
      const response = await verifier.get('/backend-api/users/me')
      expect(response.ok(), 'The verifier has an authenticated Super session').toBe(true)
      expect(await response.json()).toMatchObject({
        userRole: 'Super',
        workEmail: signedInEmail('Super'),
      })
      await fixtureUse(verifier)
    } finally {
      await verifier.dispose()
    }
  },

  protectedUser: async ({ accessVerifier, organisationId }, fixtureUse) => {
    const email = accessSubjectEmail('E2E_ACCESS_USER_EMAIL', accessSubjects.user)
    await fixtureUse(await verifiedUser(accessVerifier, organisationId, email))
  },

  foreignUser: async ({ accessVerifier, organisationId }, fixtureUse) => {
    const foreignOrganisationId = organisationIdFor('Super')
    if (!foreignOrganisationId || foreignOrganisationId === organisationId) {
      throw new Error('Cross-organisation access checks need a distinct Super organisation')
    }
    const email = accessSubjectEmail('E2E_ACCESS_FOREIGN_USER_EMAIL', accessSubjects.foreignUser)
    await fixtureUse(await verifiedUser(accessVerifier, foreignOrganisationId, email))
  },

  protectedRequest: async ({ accessVerifier, organisationId }, fixtureUse) => {
    const email = accessSubjectEmail('E2E_ACCESS_REQUEST_EMAIL', accessSubjects.registrationRequest)
    await fixtureUse(await verifiedRegistrationRequest(accessVerifier, organisationId, email))
  },

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
