import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { createRef, useState } from 'react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { MultiSelect } from './MultiSelect'

import type { MultiSelectProps } from './MultiSelect'

const options = [
  { value: 'apple', label: 'Apple' },
  { value: 'pear', label: 'Pear' },
  { value: 'plum', label: 'Plum', disabled: true },
]

function Controlled(props: Partial<MultiSelectProps> = {}) {
  const [value, setValue] = useState<readonly string[]>(props.value ?? [])
  return (
    <MultiSelect
      label="Fruit"
      name="fruit"
      options={options}
      {...props}
      onChange={(next) => {
        setValue(next)
        props.onChange?.(next)
      }}
      value={value}
    />
  )
}

afterEach(cleanup)

describe('MultiSelect', () => {
  it('labels the combobox and connects hints, errors, and consumer descriptions', () => {
    render(
      <Controlled
        aria-describedby="extra"
        error
        errorMessage="Choose fewer items"
        hint="Optional"
        id="fruit"
      />,
    )
    const input = screen.getByRole('combobox', { name: 'Fruit' })
    expect(input.getAttribute('aria-describedby')).toBe(
      'extra fruit-hint fruit-error fruit-instructions',
    )
    expect(input.getAttribute('aria-invalid')).toBe('true')
    expect(input.getAttribute('aria-expanded')).toBe('false')
  })

  it('generates distinct IDs for multiple components sharing a name', () => {
    render(
      <>
        <Controlled />
        <Controlled />
      </>,
    )
    const inputs = screen.getAllByRole('combobox')
    expect(inputs[0].id).not.toBe(inputs[1].id)
  })

  it('opens on click and filters case-insensitively without losing selections', () => {
    render(<Controlled value={['apple']} />)
    const input = screen.getByRole('combobox')
    fireEvent.click(input)
    expect(screen.getAllByRole('option')).toHaveLength(3)
    expect(screen.getByRole('listbox').getAttribute('aria-multiselectable')).toBe('true')
    fireEvent.change(input, { target: { value: ' PEAR ' } })
    expect(screen.getAllByRole('option')).toHaveLength(1)
    expect(screen.getByRole('button', { name: 'Remove Apple' })).toBeDefined()
    fireEvent.change(input, { target: { value: 'unknown' } })
    expect(screen.getByText('No options found.')).toBeDefined()
    expect(input.hasAttribute('aria-activedescendant')).toBe(false)
  })

  it('toggles selected values, clears search, and keeps the panel open', () => {
    const onChange = vi.fn()
    render(<Controlled onChange={onChange} />)
    const input = screen.getByRole('combobox') as HTMLInputElement
    fireEvent.change(input, { target: { value: 'app' } })
    fireEvent.click(screen.getByRole('option', { name: 'Apple' }))
    expect(onChange).toHaveBeenLastCalledWith(['apple'])
    expect(input.value).toBe('')
    expect(input.getAttribute('aria-expanded')).toBe('true')
    expect(screen.getByRole('option', { name: 'Apple' }).getAttribute('aria-selected')).toBe('true')
    fireEvent.click(screen.getByRole('option', { name: 'Apple' }))
    expect(onChange).toHaveBeenLastCalledWith([])
  })

  it('supports arrow keys and Enter while skipping disabled options', () => {
    render(<Controlled />)
    const input = screen.getByRole('combobox')
    input.focus()
    fireEvent.keyDown(input, { key: 'ArrowUp' })
    expect(input.getAttribute('aria-activedescendant')).toBe(
      screen.getByRole('option', { name: 'Pear' }).id,
    )
    fireEvent.keyDown(input, { key: 'Enter' })
    expect(screen.getByRole('button', { name: 'Remove Pear' })).toBeDefined()
    expect(document.activeElement).toBe(input)
    fireEvent.keyDown(input, { key: 'ArrowUp' })
    fireEvent.keyDown(input, { key: 'Enter' })
    expect(screen.getByRole('button', { name: 'Remove Apple' })).toBeDefined()
    fireEvent.keyDown(input, { key: 'Escape' })
    expect(input.getAttribute('aria-expanded')).toBe('false')
    expect(input.hasAttribute('aria-activedescendant')).toBe(false)
    expect(document.activeElement).toBe(input)
  })

  it('does not hijack text-editing keys or composition events', () => {
    render(<Controlled />)
    const input = screen.getByRole('combobox')
    fireEvent.click(input)
    for (const key of [' ', 'Home', 'End', 'ArrowLeft', 'ArrowRight', 'Backspace']) {
      expect(fireEvent.keyDown(input, { key })).toBe(true)
    }
    expect(fireEvent.keyDown(input, { key: 'Enter', isComposing: true })).toBe(true)
    expect(screen.queryByRole('button', { name: /Remove/ })).toBeNull()
  })

  it('prevents additions at the limit but allows deselection', () => {
    const onChange = vi.fn()
    render(<Controlled maxSelections={1} onChange={onChange} value={['apple']} />)
    fireEvent.click(screen.getByRole('combobox'))
    const pear = screen.getByRole('option', { name: 'Pear' })
    expect(pear.getAttribute('aria-disabled')).toBe('true')
    fireEvent.click(pear)
    expect(onChange).not.toHaveBeenCalled()
    expect(screen.getByRole('status').textContent).toContain('Selection limit reached')
    fireEvent.click(screen.getByRole('option', { name: 'Apple' }))
    fireEvent.click(pear)
    expect(onChange).toHaveBeenLastCalledWith(['pear'])
  })

  it('supports zero and unlimited selections and ignores disabled options', () => {
    const onChange = vi.fn()
    const { rerender } = render(<Controlled maxSelections={0} onChange={onChange} />)
    fireEvent.click(screen.getByRole('combobox'))
    fireEvent.click(screen.getByRole('option', { name: 'Apple' }))
    expect(onChange).not.toHaveBeenCalled()
    rerender(<Controlled onChange={onChange} />)
    fireEvent.click(screen.getByRole('option', { name: 'Apple' }))
    fireEvent.click(screen.getByRole('option', { name: 'Pear' }))
    fireEvent.click(screen.getByRole('option', { name: 'Plum' }))
    expect(onChange).toHaveBeenLastCalledWith(['apple', 'pear'])
  })

  it('recovers focus when removing a selection, including disabled options', () => {
    render(<Controlled value={['apple', 'pear', 'plum']} />)
    fireEvent.click(screen.getByRole('button', { name: 'Remove Pear' }))
    expect(document.activeElement).toBe(screen.getByRole('button', { name: 'Remove Plum' }))
    fireEvent.click(screen.getByRole('button', { name: 'Remove Plum' }))
    expect(document.activeElement).toBe(screen.getByRole('button', { name: 'Remove Apple' }))
    fireEvent.click(screen.getByRole('button', { name: 'Remove Apple' }))
    expect(document.activeElement).toBe(screen.getByRole('combobox'))
  })

  it('closes on Tab, outside pointer interaction, and external blur', () => {
    const onBlur = vi.fn()
    render(<Controlled onBlur={onBlur} value={['apple']} />)
    const input = screen.getByRole('combobox')
    fireEvent.click(input)
    fireEvent.keyDown(input, { key: 'Tab' })
    expect(input.getAttribute('aria-expanded')).toBe('false')
    fireEvent.click(input)
    fireEvent.pointerDown(document.body)
    expect(input.getAttribute('aria-expanded')).toBe('false')
    fireEvent.blur(input, { relatedTarget: screen.getByRole('button', { name: 'Remove Apple' }) })
    expect(onBlur).not.toHaveBeenCalled()
    fireEvent.blur(input, { relatedTarget: document.body })
    expect(onBlur).toHaveBeenCalledOnce()
  })

  it('supports object and callback input refs and width styling', () => {
    const ref = createRef<HTMLInputElement>()
    const { rerender, container } = render(<Controlled inputRef={ref} width={20} />)
    expect(ref.current).toBe(screen.getByRole('combobox'))
    expect(container.querySelector<HTMLElement>('[style]')?.style.maxWidth).toBe('20.5em')
    const callback = vi.fn()
    rerender(<Controlled inputRef={callback} />)
    expect(callback).toHaveBeenCalledWith(screen.getByRole('combobox'))
  })

  it('deduplicates controlled selections and submits values, never the search query', () => {
    const { container } = render(
      <form>
        <Controlled value={['apple', 'apple', 'pear']} />
      </form>,
    )
    fireEvent.change(screen.getByRole('combobox'), { target: { value: 'search text' } })
    expect(new FormData(container.querySelector('form')!).getAll('fruit')).toEqual([
      'apple',
      'pear',
    ])
  })

  it('disables all controls and omits disabled values from native form submission', () => {
    const { container } = render(
      <form>
        <Controlled disabled value={['apple']} />
      </form>,
    )
    expect((screen.getByRole('combobox') as HTMLInputElement).disabled).toBe(true)
    for (const button of screen.getAllByRole('button')) {
      expect((button as HTMLButtonElement).disabled).toBe(true)
    }
    expect(new FormData(container.querySelector('form')!).getAll('fruit')).toEqual([])
  })

  it('removes the active descendant when options change or the component is disabled', () => {
    const { rerender } = render(<Controlled />)
    const input = screen.getByRole('combobox')
    fireEvent.keyDown(input, { key: 'ArrowDown' })
    rerender(<Controlled options={[]} />)
    expect(input.hasAttribute('aria-activedescendant')).toBe(false)
    rerender(<Controlled disabled />)
    expect(input.getAttribute('aria-expanded')).toBe('false')
  })
})
