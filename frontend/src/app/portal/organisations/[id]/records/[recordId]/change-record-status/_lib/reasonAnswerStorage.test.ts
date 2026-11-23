// @vitest-environment jsdom

import { afterEach, describe, expect, it, vi } from 'vitest'

import { clearReasonAnswers, loadReasonAnswers, saveReasonAnswers } from './reasonAnswerStorage'

afterEach(() => {
  sessionStorage.clear()
  vi.restoreAllMocks()
})

describe('reasonAnswerStorage', () => {
  it('loads the answers saved for the record and change', () => {
    expect(saveReasonAnswers(42, 'Archive', { reason: 'Other', note: 'Detail' })).toBe(true)

    expect(loadReasonAnswers(42, 'Archive')).toEqual({ reason: 'Other', note: 'Detail' })
  })

  it.each([
    ['another record', 43, 'Archive'],
    ['another change', 42, 'PlaceOnHold'],
  ] as const)('keeps answers apart from %s', (_, recordId, action) => {
    saveReasonAnswers(42, 'Archive', { reason: 'DevelopmentDiscontinued' })

    expect(loadReasonAnswers(recordId, action)).toBeUndefined()
  })

  it('forgets cleared answers', () => {
    saveReasonAnswers(42, 'Archive', { reason: 'DevelopmentDiscontinued' })

    clearReasonAnswers(42, 'Archive')

    expect(loadReasonAnswers(42, 'Archive')).toBeUndefined()
  })

  it.each([
    ['not JSON', '{'],
    ['not an object', '"Other"'],
    ['an unknown reason', '{"reason":"Banana"}'],
    ['a note that is not text', '{"reason":"Other","note":5}'],
  ])('ignores saved answers that are %s', (_, value) => {
    sessionStorage.setItem('record-status-change:42:Archive', value)

    expect(loadReasonAnswers(42, 'Archive')).toBeUndefined()
  })

  it('reports when the browser blocks storage', () => {
    vi.spyOn(Storage.prototype, 'setItem').mockImplementation(() => {
      throw new Error('blocked')
    })
    vi.spyOn(Storage.prototype, 'getItem').mockImplementation(() => {
      throw new Error('blocked')
    })
    vi.spyOn(Storage.prototype, 'removeItem').mockImplementation(() => {
      throw new Error('blocked')
    })

    expect(saveReasonAnswers(42, 'Archive', { reason: 'Other', note: 'Detail' })).toBe(false)
    expect(loadReasonAnswers(42, 'Archive')).toBeUndefined()
    expect(() => clearReasonAnswers(42, 'Archive')).not.toThrow()
  })
})
