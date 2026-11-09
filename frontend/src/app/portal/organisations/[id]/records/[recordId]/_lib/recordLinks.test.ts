import { describe, expect, it } from 'vitest'

import { buildRecordHref } from './recordLinks'

describe('buildRecordHref', () => {
  it('links to the record page', () => {
    expect(buildRecordHref(7, 42, 'Medicine')).toBe(
      '/portal/organisations/7/records/42?recordType=Medicine',
    )
  })

  it('links to a page beneath the record', () => {
    expect(buildRecordHref(7, 42, 'Vaccine', 'discard')).toBe(
      '/portal/organisations/7/records/42/discard?recordType=Vaccine',
    )
  })

  it('links to the record page with an alert', () => {
    expect(buildRecordHref(7, 42, 'Medicine', undefined, 'archived')).toBe(
      '/portal/organisations/7/records/42?recordType=Medicine&action=archived',
    )
  })
})
