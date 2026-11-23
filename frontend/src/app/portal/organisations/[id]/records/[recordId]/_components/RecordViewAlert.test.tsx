import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { parseRecordViewAlert, RecordViewAlert } from './RecordViewAlert'

describe('RecordViewAlert', () => {
  it.each([
    [
      'archived',
      'Record status changed to archived',
      'You have updated the record status to archived.',
    ],
    ['placed-on-hold', 'Record placed on hold', 'You have placed this record on hold.'],
    [
      'hold-removed',
      'Record status changed to published',
      'You have updated the record status to published.',
    ],
  ] as const)('confirms the record was %s', (action, heading, body) => {
    render(<RecordViewAlert action={action} />)

    expect(screen.getByRole('heading', { level: 2, name: heading })).toBeInTheDocument()
    expect(screen.getByText(new RegExp(body))).toBeInTheDocument()
  })
})

describe('parseRecordViewAlert', () => {
  it.each(['archived', 'placed-on-hold', 'hold-removed'])('accepts %s', (action) => {
    expect(parseRecordViewAlert(action)).toBe(action)
  })

  it.each([undefined, '', 'discarded', 'toString', ['archived']])('ignores %j', (action) => {
    expect(parseRecordViewAlert(action)).toBeUndefined()
  })
})
