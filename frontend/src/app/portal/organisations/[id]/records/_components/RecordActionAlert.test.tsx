import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { navigationState } from '@/test-utils/nextNavigation'

import { RecordActionAlert } from './RecordActionAlert'

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

beforeEach(() => {
  // The alert is shown once, only when the URL carries a banner parameter
  navigationState.searchParams = new URLSearchParams({ action: 'discarded' })
})

afterEach(cleanup)

describe('RecordActionAlert', () => {
  it('confirms that the record was discarded', () => {
    render(<RecordActionAlert action="discarded" />)

    expect(
      screen.getByRole('heading', { level: 3, name: 'Record successfully discarded' }),
    ).toBeTruthy()
    expect(
      screen.getByText(
        'You have discarded the record. This record will no longer be viewable under my records.',
      ),
    ).toBeTruthy()
  })

  it('confirms that the record was submitted for QA review', () => {
    render(<RecordActionAlert action="submitted" />)

    expect(
      screen.getByRole('heading', { level: 3, name: 'Record submitted for QA review' }),
    ).toBeTruthy()
    expect(
      screen.getByText(
        'You have submitted the record for review by the QA team. You cannot make any edits to the record whilst it is in review.',
      ),
    ).toBeTruthy()
  })

  it('confirms that the draft record was saved', () => {
    render(<RecordActionAlert action="draft-saved" />)

    expect(screen.getByRole('heading', { level: 3, name: 'Draft record saved' })).toBeTruthy()
    expect(
      screen.getByText(
        'This record has been saved in my records. This record has not been published and is not visible to horizon scanners.',
      ),
    ).toBeTruthy()
  })
})
