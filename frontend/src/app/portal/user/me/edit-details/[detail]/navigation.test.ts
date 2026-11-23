import { describe, it, expect } from 'vitest'

import {
  findTypedPageProps,
  buildUpdateUserDetailsHref,
  type PageProps,
  type UserDetail,
} from './navigation'

const buildProps = (
  detail: string,
  searchParams: { verificationLinkResent?: string } = {},
): PageProps => ({
  params: Promise.resolve({ detail }),
  searchParams: Promise.resolve(searchParams),
})

describe('findTypedPageProps', () => {
  describe('params.detail', () => {
    describe('when the detail param is valid', () => {
      it.each(['fullName', 'contactNumber', 'email'] as const)(
        'returns "%s" as the detail',
        async (detail) => {
          const result = await findTypedPageProps(buildProps(detail))

          expect(result.params).toEqual({ detail })
        },
      )
    })

    describe('when the detail param is invalid', () => {
      it.each([
        ['an unknown value', 'address'],
        ['an empty string', ''],
        ['a different casing', 'FullName'],
        ['a value with surrounding whitespace', ' email '],
        ['a snake_case variant', 'contact_number'],
      ])('returns undefined for %s', async (_label, detail) => {
        const result = await findTypedPageProps(buildProps(detail))

        expect(result.params).toEqual({ detail: undefined })
      })
    })

    describe('when the detail param is missing', () => {
      it('returns undefined if the detail is not present on params', async () => {
        const props = {
          params: Promise.resolve({}),
          searchParams: Promise.resolve({}),
        } as unknown as PageProps

        const result = await findTypedPageProps(props)

        expect(result.params).toEqual({ detail: undefined })
      })
    })
  })

  describe('searchParams.verificationLinkResent', () => {
    it.each([
      ['lowercase "true"', 'true'],
      ['uppercase "TRUE"', 'TRUE'],
      ['mixed case "TrUe"', 'TrUe'],
    ])('returns true for %s', async (_label, value) => {
      const result = await findTypedPageProps(
        buildProps('email', { verificationLinkResent: value }),
      )

      expect(result.searchParams).toEqual({ verificationLinkResent: true })
    })

    it.each([
      ['"false"', 'false'],
      ['an empty string', ''],
      ['"1"', '1'],
      ['an arbitrary string', 'yes'],
      ['"true" with surrounding whitespace', ' true '],
    ])('returns undefined for %s', async (_label, value) => {
      const result = await findTypedPageProps(
        buildProps('email', { verificationLinkResent: value }),
      )

      expect(result.searchParams).toEqual({ verificationLinkResent: undefined })
    })

    it('returns undefined when the param is not present', async () => {
      const result = await findTypedPageProps(buildProps('email'))

      expect(result.searchParams).toEqual({ verificationLinkResent: undefined })
    })

    it('returns undefined when searchParams resolves to undefined', async () => {
      const props = {
        params: Promise.resolve({ detail: 'email' }),
        searchParams: Promise.resolve(undefined),
      } as unknown as PageProps

      const result = await findTypedPageProps(props)

      expect(result.searchParams).toEqual({ verificationLinkResent: undefined })
    })
  })

  describe('full result shape', () => {
    it('returns both params and searchParams', async () => {
      const result = await findTypedPageProps(
        buildProps('email', { verificationLinkResent: 'true' }),
      )

      expect(result).toEqual({
        params: { detail: 'email' },
        searchParams: { verificationLinkResent: true },
      })
    })
  })

  describe('async behaviour', () => {
    it('awaits the params promise before resolving', async () => {
      const props: PageProps = {
        params: new Promise((resolve) => setTimeout(() => resolve({ detail: 'email' }), 10)),
        searchParams: Promise.resolve({}),
      }

      const result = await findTypedPageProps(props)

      expect(result.params.detail).toBe('email')
    })

    it('awaits the searchParams promise before resolving', async () => {
      const props: PageProps = {
        params: Promise.resolve({ detail: 'email' }),
        searchParams: new Promise((resolve) =>
          setTimeout(() => resolve({ verificationLinkResent: 'true' }), 10),
        ),
      }

      const result = await findTypedPageProps(props)

      expect(result.searchParams.verificationLinkResent).toBe(true)
    })

    it('rejects if the params promise rejects', async () => {
      const props: PageProps = {
        params: Promise.reject(new Error('params failed')),
        searchParams: Promise.resolve({}),
      }

      await expect(findTypedPageProps(props)).rejects.toThrow('params failed')
    })

    it('rejects if the searchParams promise rejects', async () => {
      const props: PageProps = {
        params: Promise.resolve({ detail: 'email' }),
        searchParams: Promise.reject(new Error('searchParams failed')),
      }

      await expect(findTypedPageProps(props)).rejects.toThrow('searchParams failed')
    })
  })
})

describe('buildUpdateUserDetailsHref', () => {
  it.each([
    ['fullName', '/portal/user/me/edit-details/fullName'],
    ['contactNumber', '/portal/user/me/edit-details/contactNumber'],
    ['email', '/portal/user/me/edit-details/email'],
  ] as [UserDetail, string][])('builds the correct href for "%s"', (detail, expected) => {
    expect(
      buildUpdateUserDetailsHref({ detail } as Parameters<typeof buildUpdateUserDetailsHref>[0]),
    ).toBe(expected)
  })

  it('does not add a trailing slash', () => {
    const href = buildUpdateUserDetailsHref({ detail: 'email' })

    expect(href.endsWith('/')).toBe(false)
  })

  describe('when detail is "email"', () => {
    it('appends the verificationLinkResent query when emailLinkResent is true', () => {
      const href = buildUpdateUserDetailsHref({ detail: 'email', emailLinkResent: true })

      expect(href).toBe('/portal/user/me/edit-details/email?verificationLinkResent=true')
    })

    it('omits the query string when emailLinkResent is not provided', () => {
      const href = buildUpdateUserDetailsHref({ detail: 'email' })

      expect(href).toBe('/portal/user/me/edit-details/email')
    })
  })

  describe('when detail is not "email"', () => {
    it.each(['fullName', 'contactNumber'] as const)(
      'never includes a query string for "%s"',
      (detail) => {
        const href = buildUpdateUserDetailsHref({ detail })

        expect(href).not.toContain('?')
      },
    )
  })

  describe('round trip with findTypedPageProps', () => {
    it('produces a query string that findTypedPageProps parses back to true', async () => {
      const href = buildUpdateUserDetailsHref({ detail: 'email', emailLinkResent: true })
      const qs = new URLSearchParams(href.split('?')[1])

      const result = await findTypedPageProps(
        buildProps('email', {
          verificationLinkResent: qs.get('verificationLinkResent') ?? undefined,
        }),
      )

      expect(result.searchParams.verificationLinkResent).toBe(true)
    })
  })
})
