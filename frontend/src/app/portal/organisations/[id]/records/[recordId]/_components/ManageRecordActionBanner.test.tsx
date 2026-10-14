import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { NextLinkMock } from '@/test-utils/nextMocks'

import { ManageRecordActionBanner } from './ManageRecordActionBanner'

vi.mock('next/link', () => ({ default: NextLinkMock }))

afterEach(cleanup)

describe('ManageRecordActionBanner', () => {
  it('links Update record to the form when there is one', () => {
    render(<ManageRecordActionBanner updateHref="/portal/records/42/revisions/3/indication" />)

    expect(screen.getByRole('link', { name: 'Update record' }).getAttribute('href')).toBe(
      '/portal/records/42/revisions/3/indication',
    )
  })

  it('shows Update record as a button when there is no form', () => {
    render(<ManageRecordActionBanner />)

    expect(screen.getByRole('button', { name: 'Update record' })).toBeDefined()
    expect(screen.queryByRole('link', { name: 'Update record' })).toBeNull()
  })
})
