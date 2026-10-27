import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import { RecordActionAlert } from './RecordActionAlert'

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
})
