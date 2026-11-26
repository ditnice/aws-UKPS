import { describe, it, expect, vi } from 'vitest'

import {
  findTypedPageProps,
  buildUpdateUserDetailsHref,
  type PageProps,
  type UserDetail,
} from './navigation'

vi.mock('next/navigation', () => ({
  notFound: vi.fn(() => {
    // Mirrors Next.js behaviour: notFound() throws to halt rendering
    throw new Error('NEXT_NOT_FOUND')
  }),
}))

type Params = Awaited<PageProps['params']>
type SearchParams = Awaited<NonNullable<PageProps['searchParams']>>

const ORGANISATION_ID = 7
const USER_ID = 42
const RETURN_LINK = `/portal/organisations/${ORGANISATION_ID}/users/${USER_ID}/details`

const buildProps = (
  detail: string,
  searchParams: SearchParams = {},
  params: Partial<Params> = {},
): PageProps => ({
  params: Promise.resolve({
    id: String(ORGANISATION_ID),
    userId: String(USER_ID),
    detail,
    ...params,
  }),
  searchParams: Promise.resolve(searchParams),
})

describe('findTypedPageProps', () => {
  describe('params.detail', () => {
    describe('when the detail param is valid', () => {
      it.each(['fullName', 'contactNumber', 'email'] as const)(
        'returns "%s" as the detail',
        async (detail) => {
          const result = await findTypedPageProps(buildProps(detail))

          expect(result.detail).toBe(detail)
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
      ])('calls notFound for %s', async (_label, detail) => {
        await expect(findTypedPageProps(buildProps(detail))).rejects.toThrow('NEXT_NOT_FOUND')
      })
    })
  })

  describe('params.id and params.userId', () => {
    it('parses the organisation id and user id as numbers', async () => {
      const result = await findTypedPageProps(buildProps('email'))

      expect(result.organisationId).toBe(ORGANISATION_ID)
      expect(result.userId).toBe(USER_ID)
    })

    it.each([
      ['an empty string', ''],
      ['whitespace', ' '],
      ['a non-numeric value', 'abc'],
      ['a decimal', '1.5'],
      ['a negative number', '-1'],
    ])('calls notFound when the organisation id is %s', async (_label, id) => {
      await expect(findTypedPageProps(buildProps('email', {}, { id }))).rejects.toThrow(
        'NEXT_NOT_FOUND',
      )
    })

    it.each([
      ['an empty string', ''],
      ['a non-numeric value', 'abc'],
      ['a negative number', '-1'],
    ])('calls notFound when the user id is %s', async (_label, userId) => {
      await expect(findTypedPageProps(buildProps('email', {}, { userId }))).rejects.toThrow(
        'NEXT_NOT_FOUND',
      )
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

      expect(result).toMatchObject({ verificationLinkResent: true })
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

      expect(result).toMatchObject({ verificationLinkResent: undefined })
    })

    it('returns undefined when searchParams resolves to undefined', async () => {
      const props = {
        params: Promise.resolve({ id: '1', userId: '2', detail: 'email' }),
        searchParams: Promise.resolve(undefined),
      } as unknown as PageProps

      const result = await findTypedPageProps(props)

      expect(result).toMatchObject({ verificationLinkResent: undefined })
    })
  })

  describe('searchParams.error', () => {
    it('returns true when the error flag is "true"', async () => {
      const result = await findTypedPageProps(buildProps('email', { error: 'true' }))

      expect(result).toMatchObject({ error: true })
    })

    it('returns undefined when the error flag is not "true"', async () => {
      const result = await findTypedPageProps(buildProps('email', { error: 'nope' }))

      expect(result).toMatchObject({ error: undefined })
    })
  })

  describe('searchParams.returnLink', () => {
    it('returns a same-site relative path as-is', async () => {
      const result = await findTypedPageProps(buildProps('email', { returnLink: RETURN_LINK }))

      expect(result.returnLink).toBe(RETURN_LINK)
    })

    it.each([
      ['missing', undefined],
      ['an absolute url', 'https://evil.com'],
      ['a protocol-relative url', '//evil.com'],
      ['a backslash-prefixed url', '/\\evil.com'],
    ])('falls back to /portal when it is %s', async (_label, value) => {
      const result = await findTypedPageProps(buildProps('email', { returnLink: value }))

      expect(result.returnLink).toBe('/portal')
    })
  })

  describe('searchParams.onSuccessFlag', () => {
    it('returns the flag as-is', async () => {
      const result = await findTypedPageProps(buildProps('email', { onSuccessFlag: 'updated' }))

      expect(result.onSuccessFlag).toBe('updated')
    })

    it('defaults to an empty string when missing', async () => {
      const result = await findTypedPageProps(buildProps('email'))

      expect(result.onSuccessFlag).toBe('')
    })
  })

  describe('full result shape', () => {
    it('returns the ids, detail, navigation and flags', async () => {
      const result = await findTypedPageProps(
        buildProps('email', {
          verificationLinkResent: 'true',
          returnLink: RETURN_LINK,
          onSuccessFlag: 'requestedNewEmail',
        }),
      )

      expect(result).toEqual({
        organisationId: ORGANISATION_ID,
        userId: USER_ID,
        detail: 'email',
        returnLink: RETURN_LINK,
        onSuccessFlag: 'requestedNewEmail',
        verificationLinkResent: true,
        error: undefined,
      })
    })
  })

  describe('async behaviour', () => {
    it('awaits the params promise before resolving', async () => {
      const props: PageProps = {
        params: new Promise((resolve) =>
          setTimeout(() => resolve({ id: '1', userId: '2', detail: 'email' }), 10),
        ),
        searchParams: Promise.resolve({}),
      }

      const result = await findTypedPageProps(props)

      expect(result.detail).toBe('email')
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
        params: Promise.resolve({ id: '1', userId: '2', detail: 'email' }),
        searchParams: Promise.reject(new Error('searchParams failed')),
      }

      await expect(findTypedPageProps(props)).rejects.toThrow('searchParams failed')
    })
  })
})

describe('buildUpdateUserDetailsHref', () => {
  const base = {
    organisationId: ORGANISATION_ID,
    userId: USER_ID,
    returnLink: RETURN_LINK,
    onSuccessFlag: 'updated',
  }
  const encodedNavigation = `returnLink=${encodeURIComponent(RETURN_LINK)}&onSuccessFlag=updated`
  const basePath = `/portal/organisations/${ORGANISATION_ID}/users/${USER_ID}/details/edit-details`

  it.each(['fullName', 'contactNumber', 'email'] as UserDetail[])(
    'builds the correct href for "%s"',
    (detail) => {
      expect(buildUpdateUserDetailsHref({ ...base, detail })).toBe(
        `${basePath}/${detail}?${encodedNavigation}`,
      )
    },
  )

  describe('when detail is "email"', () => {
    it('appends the verificationLinkResent query when verificationLinkResent is true', () => {
      const href = buildUpdateUserDetailsHref({
        ...base,
        detail: 'email',
        verificationLinkResent: true,
      })

      expect(href).toBe(`${basePath}/email?verificationLinkResent=true&${encodedNavigation}`)
    })

    it('appends the error query when error is true', () => {
      const href = buildUpdateUserDetailsHref({ ...base, detail: 'email', error: true })

      expect(href).toBe(`${basePath}/email?error=true&${encodedNavigation}`)
    })
  })

  describe('round trip with findTypedPageProps', () => {
    it('produces a url that findTypedPageProps parses back', async () => {
      const href = buildUpdateUserDetailsHref({
        ...base,
        detail: 'email',
        verificationLinkResent: true,
      })
      const qs = new URLSearchParams(href.split('?')[1])

      const result = await findTypedPageProps(buildProps('email', Object.fromEntries(qs.entries())))

      expect(result).toEqual({
        ...base,
        detail: 'email',
        verificationLinkResent: true,
        error: undefined,
      })
    })
  })
})
