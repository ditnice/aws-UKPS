import { createServer } from 'node:http'

import { expect, test as base } from '@playwright/test'

import { userRoles, type UserRole } from '../helpers/roles'
import { signInRole } from '../helpers/sign-in-role'
import { type RoleCredentials } from '../helpers/test-environment'

import type {
  OrganisationListDto,
  UserInformationDto,
} from '../../../src/client/generated/types.gen'

const credentials: RoleCredentials = {
  email: 'setup-user@example.test',
  password: 'test-only-password',
  totpSecret: 'JBSWY3DPEHPK3PXP',
}

type Options = {
  role: UserRole
  mfa: boolean
  multipleOrganisations: boolean
  userOverride: Partial<UserInformationDto>
}

type AuthApp = {
  origin: string
  selections: number[]
  mfaCodes: string[]
}

// A local HTTP stub exercises the setup helper in a real browser without a deployed account or
// Cognito. It models navigation, not the application's own rendering or authorisation rules.
const test = base.extend<Options & { authApp: AuthApp }>({
  role: ['Champion', { option: true }],
  mfa: [false, { option: true }],
  multipleOrganisations: [false, { option: true }],
  userOverride: [{}, { option: true }],
  authApp: async ({ role, mfa, multipleOrganisations, userOverride }, fixtureUse) => {
    const selections: number[] = []
    const mfaCodes: string[] = []
    const organisations: OrganisationListDto[] = [
      ...(multipleOrganisations ? [{ id: 1, organisationName: 'Other Pharma' }] : []),
      { id: 2, organisationName: 'Expected Pharma' },
    ]
    let selectedOrganisationId = multipleOrganisations ? 1 : 2
    const server = createServer(async (request, response) => {
      const url = new URL(request.url ?? '/', 'http://localhost')
      const redirect = (path: string) => {
        response.writeHead(302, { location: path }).end()
      }
      const html = (body: string) => {
        response.writeHead(200, { 'content-type': 'text/html' }).end(body)
      }
      const json = (body: unknown) => {
        response.writeHead(200, { 'content-type': 'application/json' }).end(JSON.stringify(body))
      }

      switch (url.pathname) {
        case '/auth/sign-in':
          html(`<form method="post" action="/auth/login">
            <label>Email address<input name="email"></label>
            <label>Password<input name="password" type="password"></label>
            <button>Continue</button></form>`)
          break
        case '/auth/login':
          redirect(mfa ? '/auth/sign-in/mfa' : '/portal/organisations/select')
          break
        case '/auth/sign-in/mfa':
          html(`<form method="post" action="/auth/mfa">
            <label>Security code<input name="code"></label>
            <button>Continue</button></form>`)
          break
        case '/auth/mfa': {
          let body = ''
          for await (const chunk of request) body += chunk.toString()
          mfaCodes.push(new URLSearchParams(body).get('code') ?? '')
          redirect('/portal/organisations/select')
          break
        }
        case '/portal/organisations/select':
          if (!multipleOrganisations) {
            redirect('/portal/organisations/2/records')
            break
          }
          html(`<h1>Select the organisation to manage</h1>
            ${organisations
              .map(
                ({
                  id,
                  organisationName,
                }) => `<form method="post" action="/select-organisation?id=${id}">
                  <button aria-label="Manage Organisation - ${organisationName}">Manage</button></form>`,
              )
              .join('')}`)
          break
        case '/select-organisation':
          selectedOrganisationId = Number(url.searchParams.get('id'))
          selections.push(selectedOrganisationId)
          redirect(`/portal/organisations/${selectedOrganisationId}/records`)
          break
        case '/backend-api/users/me/organisations':
          json(organisations)
          break
        case '/backend-api/users/me':
          json({
            workEmail: credentials.email,
            userRole: role,
            organisationId: selectedOrganisationId,
            ...userOverride,
          })
          break
        default:
          html('<h1>Organisation records</h1>')
      }
    })
    await new Promise<void>((resolve) => server.listen(0, '127.0.0.1', resolve))
    const address = server.address()
    if (!address || typeof address === 'string') throw new Error('Auth stub did not start')
    try {
      await fixtureUse({ origin: `http://127.0.0.1:${address.port}`, selections, mfaCodes })
    } finally {
      await new Promise<void>((resolve, reject) => {
        server.close((error) => (error ? reject(error) : resolve()))
        server.closeAllConnections()
      })
    }
  },
})

// The helper uses relative URLs just like the setup project. A fresh context supplies the stub's
// origin instead of the application's configured baseURL.
test.describe('Real-auth setup flow', () => {
  for (const role of userRoles) {
    for (const mfa of [false, true]) {
      for (const multipleOrganisations of [false, true]) {
        test.describe(`${role}, MFA=${mfa}, multiple organisations=${multipleOrganisations}`, () => {
          test.use({ role, mfa, multipleOrganisations })

          test('verifies the session after completing sign-in and organisation selection', async ({
            browser,
            authApp,
          }) => {
            const context = await browser.newContext({ baseURL: authApp.origin })
            try {
              await signInRole(await context.newPage(), role, credentials, '2')

              expect(authApp.selections).toEqual(multipleOrganisations ? [2] : [])
              expect(authApp.mfaCodes).toHaveLength(mfa ? 1 : 0)
              if (mfa) expect(authApp.mfaCodes[0]).toMatch(/^\d{6}$/)
            } finally {
              await context.close()
            }
          })
        })
      }
    }
  }

  for (const userOverride of [
    { workEmail: 'wrong-user@example.test' },
    { userRole: 'Standard' as const },
    { organisationId: 1 },
  ]) {
    test.describe(`Mismatched session: ${Object.keys(userOverride)[0]}`, () => {
      test.use({ userOverride })

      test('rejects the session before it can be saved', async ({ browser, authApp }) => {
        const context = await browser.newContext({ baseURL: authApp.origin })
        try {
          await expect(
            signInRole(await context.newPage(), 'Champion', credentials, '2'),
          ).rejects.toThrow('The saved session must match the role project')
        } finally {
          await context.close()
        }
      })
    })
  }

  test('rejects an organisation the user cannot select', async ({ browser, authApp }) => {
    const context = await browser.newContext({ baseURL: authApp.origin })
    try {
      await expect(
        signInRole(await context.newPage(), 'Champion', credentials, '3'),
      ).rejects.toThrow('The Champion user cannot select E2E organisation 3.')
      expect(authApp.selections).toEqual([])
    } finally {
      await context.close()
    }
  })
})
