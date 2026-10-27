import { describe, expect, it } from 'vitest'

import { buildRecordActionHref, parseRecordAction } from './recordActionAlert'

describe('parseRecordAction', () => {
  it('recognises the record discarded confirmation', () => {
    expect(parseRecordAction({ action: 'discarded' })).toBe('discarded')
  })

  it.each([undefined, '', 'unknown', 'Discarded', ' discarded '])(
    'does not show a confirmation for an unsupported action: %s',
    (action) => {
      expect(parseRecordAction({ action })).toBeUndefined()
    },
  )
})

describe('buildRecordActionHref', () => {
  it('links to the records list with the action', () => {
    expect(buildRecordActionHref(7, 'discarded')).toBe(
      '/portal/organisations/7/records?action=discarded',
    )
  })
})
