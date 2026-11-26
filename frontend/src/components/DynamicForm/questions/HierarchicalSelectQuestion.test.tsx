import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import type { Question } from '@/lib/dynamic-form/types'

import { HierarchicalSelectQuestion } from './HierarchicalSelectQuestion'

const areas: Question = {
  id: 'areas',
  type: 'HierarchicalSelect',
  multiple: true,
  label: 'Select the most appropriate therapeutic area for this product (optional)',
  hint: null,
  display: null,
  placeholder: 'Choose a therapeutic area',
  childLabel: 'Select the disease or condition (optional)',
  childPlaceholder: 'Choose a disease or condition',
  addAnotherLabel: 'Add another therapy area',
  itemLabel: 'Therapeutic area',
  removeLabel: 'Remove therapeutic area',
  dateName: null,
  showWhen: null,
  rules: [{ kind: 'MaxItems', value: 3, message: 'Select up to 3 therapeutic areas' }],
  options: [
    {
      value: '1',
      label: '1: Addiction',
      hint: null,
      children: [{ value: '11', label: '1.1: Opioid use disorder', hint: null, children: null }],
    },
    { value: '2', label: '2: Allergy', hint: null, children: null },
  ],
}

describe('HierarchicalSelectQuestion', () => {
  it('describes an entry by both the question error and its own error', () => {
    render(
      <HierarchicalSelectQuestion
        clearError={vi.fn()}
        error="You have selected the same option more than once"
        errorItems={{ '0': 'Entry problem' }}
        labelStyle="heading"
        name="q0"
        onBlur={vi.fn()}
        onChange={vi.fn()}
        question={areas}
        value={[
          { parent: '1', child: '' },
          { parent: '2', child: '' },
        ]}
      />,
    )

    const groupMessage = screen.getByText('You have selected the same option more than once')
    const entryMessage = screen.getByText('Entry problem')

    const parent = screen.getByRole('combobox', { name: 'Therapeutic area 1' })
    expect(parent).toBeInvalid()
    expect(parent.getAttribute('aria-describedby')?.split(' ')).toEqual(
      expect.arrayContaining([groupMessage.id, entryMessage.id]),
    )

    const child = screen.getByRole('combobox', { name: /Select the disease or condition/ })
    expect(child).toBeInvalid()
    expect(child.getAttribute('aria-describedby')?.split(' ')).toEqual(
      expect.arrayContaining([groupMessage.id, entryMessage.id]),
    )

    const other = screen.getByRole('combobox', { name: 'Therapeutic area 2' })
    expect(other).toBeInvalid()
    expect(other.getAttribute('aria-describedby')?.split(' ')).toContain(groupMessage.id)
    expect(other.getAttribute('aria-describedby')?.split(' ')).not.toContain(entryMessage.id)
  })
})
