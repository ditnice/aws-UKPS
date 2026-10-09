import { cleanup, render, screen, within } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import { ErrorSummary } from './ErrorSummary'

afterEach(() => {
  cleanup()
})

describe('ErrorSummary', () => {
  it('renders nothing when there are no errors', () => {
    const { container } = render(<ErrorSummary errors={[]} />)

    expect(container.innerHTML).toBe('')
  })

  it('links errors to their fields and lists other errors as text', () => {
    render(
      <ErrorSummary
        errors={[{ targetId: 'field-one', message: 'Enter a value' }, { message: 'Other problem' }]}
      />,
    )

    const summary = screen.getByRole('alert')
    expect(within(summary).getByRole('heading', { name: 'There is a problem' })).toBeTruthy()
    expect(within(summary).getByRole('link', { name: 'Enter a value' }).getAttribute('href')).toBe(
      '#field-one',
    )
    expect(within(summary).getByText('Other problem')).toBeTruthy()
  })

  it('takes focus when shown', () => {
    render(<ErrorSummary errors={[{ message: 'Enter a value' }]} />)

    expect(document.activeElement).toBe(screen.getByRole('alert'))
  })
})
