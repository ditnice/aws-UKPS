import { cleanup, fireEvent, render, screen, within } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import { getComponentDefinition } from '../../_data/components'

import { Examples } from './Examples'

afterEach(cleanup)

describe('Multi-select examples', () => {
  it('is registered in the components catalogue', () => {
    expect(getComponentDefinition('multi-select').label).toBe('Multi-select')
  })

  it('renders independent interactive examples', () => {
    render(<Examples />)
    const input = screen.getByRole('combobox', { name: 'Choose locations' })
    fireEvent.click(input)
    fireEvent.click(screen.getByRole('option', { name: 'North West' }))
    expect(screen.getByRole('button', { name: 'Remove North West' })).toBeDefined()
    const limitInput = screen.getByRole('combobox', { name: 'Locations (optional)' })
    expect(limitInput.getAttribute('aria-expanded')).toBe('false')
    const descriptions = limitInput.getAttribute('aria-describedby')!.split(' ')
    const count = descriptions
      .map((id) => document.getElementById(id))
      .find((element) => element?.textContent?.includes('of 3 selected'))
    expect(count?.textContent).toBe('0 of 3 selected. ')
  })

  it('shows preselected options, validation errors and disabled controls', () => {
    render(<Examples />)
    const selections = screen.getByRole('list', { name: 'Selected options for Selected locations' })
    expect(within(selections).getByRole('button', { name: 'Remove London' })).toBeDefined()
    const errorInput = screen.getByRole('combobox', { name: 'Locations with an error' })
    expect(errorInput.getAttribute('aria-invalid')).toBe('true')
    expect(screen.getByText('Select no more than 3 locations')).toBeDefined()
    expect(
      (screen.getByRole('combobox', { name: 'Disabled locations' }) as HTMLInputElement).disabled,
    ).toBe(true)
  })

  it('exposes the unavailable option and preserves filtering', () => {
    render(<Examples />)
    const input = screen.getByRole('combobox', { name: 'Available locations' })
    fireEvent.change(input, { target: { value: 'London' } })
    const option = screen.getByRole('option', { name: 'London' })
    expect(option.getAttribute('aria-disabled')).toBe('true')
    fireEvent.click(option)
    const list = screen.queryByRole('list', { name: 'Selected options for Available locations' })
    expect(list).toBeNull()
  })
})
