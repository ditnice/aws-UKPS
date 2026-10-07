import { describe, expect, it } from 'vitest'

import { parseOrganisationAction } from './organisationActionsAlert'

describe('parseOrganisationAction', () => {
  it('recognises the organisation details update confirmation', () => {
    expect(parseOrganisationAction({ action: 'updated-details' })).toEqual({
      action: 'updated-details',
    })
  })

  it.each([undefined, '', 'unknown', 'Updated-details', ' updated-details '])(
    'does not show a confirmation for an unsupported action: %s',
    (action) => {
      expect(parseOrganisationAction({ action })).toBeUndefined()
    },
  )
})
