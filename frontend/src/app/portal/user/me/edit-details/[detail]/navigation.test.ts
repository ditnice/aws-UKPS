import { describe, it, expect } from 'vitest'

import {
  findTypedPageProps,
  buildUpdateUserDetailsHref,
  type PageProps,
  type UserDetail,
} from './navigation'

const buildProps = (detail: string): PageProps => ({
  params: Promise.resolve({ detail }),
})

describe('findTypedPageProps', () => {
  describe('when the detail param is valid', () => {
    it.each(['fullName', 'contactNumber', 'email'] as const)(
      'returns "%s" as the detail',
      async (detail) => {
        const result = await findTypedPageProps(buildProps(detail))

        expect(result).toEqual({ params: { detail } })
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

      expect(result).toEqual({ params: { detail: undefined } })
    })
  })

  describe('when the detail param is missing', () => {
    it('returns undefined if the detail is not present on params', async () => {
      const props = {
        params: Promise.resolve({}),
      } as unknown as PageProps

      const result = await findTypedPageProps(props)

      expect(result).toEqual({ params: { detail: undefined } })
    })
  })

  describe('async behaviour', () => {
    it('awaits the params promise before resolving', async () => {
      const props: PageProps = {
        params: new Promise((resolve) => setTimeout(() => resolve({ detail: 'email' }), 10)),
      }

      const result = await findTypedPageProps(props)

      expect(result.params.detail).toBe('email')
    })

    it('rejects if the params promise rejects', async () => {
      const error = new Error('params failed')
      const props: PageProps = {
        params: Promise.reject(error),
      }

      await expect(findTypedPageProps(props)).rejects.toThrow('params failed')
    })
  })
})

describe('buildUpdateUserDetailsHref', () => {
  it.each([
    ['fullName', '/portal/user/me/edit-details/fullName'],
    ['contactNumber', '/portal/user/me/edit-details/contactNumber'],
    ['email', '/portal/user/me/edit-details/email'],
  ] as [UserDetail, string][])('builds the correct href for "%s"', (detail, expected) => {
    expect(buildUpdateUserDetailsHref({ detail })).toBe(expected)
  })

  it('does not add a trailing slash', () => {
    const href = buildUpdateUserDetailsHref({ detail: 'email' })

    expect(href.endsWith('/')).toBe(false)
  })
})
