import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import { ManageRecordActionBanner } from './ManageRecordActionBanner'

afterEach(() => {
  cleanup()
})

describe('ManageRecordActionBanner', () => {
  it('links Update record to the record content form', () => {
    render(<ManageRecordActionBanner updateRecordHref="/portal/records/5/revisions/7/indication" />)

    expect(screen.getByRole('link', { name: 'Update record' }).getAttribute('href')).toBe(
      '/portal/records/5/revisions/7/indication',
    )
  })

  it('hides Update record when the record type has no form', () => {
    render(<ManageRecordActionBanner />)

    expect(screen.queryByText('Update record')).toBeNull()
  })
})
