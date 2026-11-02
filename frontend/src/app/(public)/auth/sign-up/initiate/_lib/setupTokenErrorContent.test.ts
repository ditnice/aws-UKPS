import { describe, expect, it } from 'vitest'

import { getSetupTokenErrorContent } from './setupTokenErrorContent'

describe('getSetupTokenErrorContent', () => {
  it.each([
    [400, 'This sign-up link is not valid.'],
    [404, 'This sign-up link could not be found.'],
    [409, 'This sign-up link has already been used.'],
    [500, 'We could not check your sign-up link. Try again later.'],
    [undefined, 'We could not check your sign-up link. Try again later.'],
  ])('provides a fallback for status %s', (status, detail) => {
    expect(getSetupTokenErrorContent({}, status)).toEqual({
      title: 'There is a problem with your sign-up link',
      detail,
    })
  })

  it.each([400, 404, 409, 500, undefined])(
    'preserves backend title and detail for status %s',
    (status) => {
      const error = { title: 'Backend title.', detail: 'Backend explanation.' }
      expect(getSetupTokenErrorContent(error, status)).toEqual(error)
    },
  )

  it('falls back independently for a missing title', () => {
    expect(getSetupTokenErrorContent({ title: null, detail: 'Backend explanation.' }, 404)).toEqual(
      { title: 'There is a problem with your sign-up link', detail: 'Backend explanation.' },
    )
  })

  it('falls back independently for a missing detail', () => {
    expect(getSetupTokenErrorContent({ title: 'Backend title.', detail: null }, 409)).toEqual({
      title: 'Backend title.',
      detail: 'This sign-up link has already been used.',
    })
  })
})
