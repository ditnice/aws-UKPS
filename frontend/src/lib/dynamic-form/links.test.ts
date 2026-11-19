import { describe, expect, it } from 'vitest'

import { checkAnswersPageHref } from './links'

describe('checkAnswersPageHref', () => {
  it('links to the page so saving returns to check your answers', () => {
    expect(checkAnswersPageHref(42, 'product-name')).toBe(
      '/portal/records/42/product-name?returnTo=check-your-answers',
    )
  })

  it('adds the anchor to scroll to', () => {
    expect(checkAnswersPageHref(42, 'product-name', { anchor: 'generic-name' })).toBe(
      '/portal/records/42/product-name?returnTo=check-your-answers#generic-name',
    )
  })

  it('asks for a new blank group item when adding one', () => {
    expect(
      checkAnswersPageHref(42, 'clinical-trial-details', {
        addGroupItem: true,
        anchor: 'record_clinical_trial-item-2',
      }),
    ).toBe(
      '/portal/records/42/clinical-trial-details?returnTo=check-your-answers&add=group-item#record_clinical_trial-item-2',
    )
  })
})
