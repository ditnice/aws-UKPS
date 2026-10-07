import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { createRef } from 'react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import type { InputWidth } from '@/components/Input/Input'

import { Select, SelectOption } from './Select'

const widths: InputWidth[] = [
  2,
  3,
  4,
  5,
  10,
  20,
  30,
  'full',
  'three-quarters',
  'two-thirds',
  'one-half',
  'one-third',
  'one-quarter',
]

afterEach(cleanup)

describe('Select', () => {
  it('renders a label and option children', () => {
    const { asFragment } = render(
      <Select defaultValue="updated" label="Sort by" name="sort">
        <SelectOption value="published">Recently published</SelectOption>
        <SelectOption value="updated">Recently updated</SelectOption>
        <SelectOption value="views">Most views</SelectOption>
      </Select>,
    )

    const select = screen.getByLabelText('Sort by') as HTMLSelectElement
    expect(select.value).toBe('updated')
    expect(screen.getByRole('option', { name: 'Recently published' })).toBeDefined()
    expect(asFragment()).toMatchSnapshot()
  })

  it('defaults the id to the name', () => {
    const { asFragment } = render(
      <Select label="Sort by" name="sort">
        <SelectOption value="published">Recently published</SelectOption>
      </Select>,
    )

    expect(screen.getByLabelText('Sort by').getAttribute('id')).toBe('sort')
    expect(asFragment()).toMatchSnapshot()
  })

  it('supports an explicit id', () => {
    const { asFragment } = render(
      <Select id="sort-select" label="Sort by" name="sort">
        <SelectOption value="published">Recently published</SelectOption>
      </Select>,
    )

    expect(screen.getByLabelText('Sort by').getAttribute('id')).toBe('sort-select')
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders a hint and describes the select with it', () => {
    const { asFragment } = render(
      <Select hint="Choose the most relevant option" label="Sort by" name="sort">
        <SelectOption value="published">Recently published</SelectOption>
      </Select>,
    )

    const select = screen.getByLabelText('Sort by')
    expect(screen.getByText('Choose the most relevant option').getAttribute('id')).toBe('sort-hint')
    expect(select.getAttribute('aria-describedby')).toBe('sort-hint')
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders an error and describes the select with it', () => {
    const { asFragment } = render(
      <Select error errorMessage="Select a location" label="Choose location" name="location">
        <SelectOption value="choose">Choose location</SelectOption>
      </Select>,
    )

    const select = screen.getByLabelText('Choose location')
    expect(screen.getByText('Select a location')).toBeDefined()
    expect(select.getAttribute('aria-describedby')).toBe('location-error')
    expect(select.className).toMatch(/fieldError/)
    expect(asFragment()).toMatchSnapshot()
  })

  it('merges existing describedby with hint and error ids', () => {
    const { asFragment } = render(
      <Select
        aria-describedby="existing-description"
        error
        errorMessage="Select a location"
        hint="This can be different to where you went before"
        label="Choose location"
        name="location"
      >
        <SelectOption value="choose">Choose location</SelectOption>
      </Select>,
    )

    expect(screen.getByLabelText('Choose location').getAttribute('aria-describedby')).toBe(
      'existing-description location-hint location-error',
    )
    expect(asFragment()).toMatchSnapshot()
  })

  it('forwards native select props', () => {
    const handleChange = vi.fn()
    const { asFragment } = render(
      <Select disabled label="Sort by" name="sort" onChange={handleChange} required>
        <SelectOption value="published">Recently published</SelectOption>
        <SelectOption disabled value="updated">
          Recently updated
        </SelectOption>
      </Select>,
    )

    const select = screen.getByLabelText('Sort by') as HTMLSelectElement
    expect(select.disabled).toBe(true)
    expect(select.required).toBe(true)
    expect(screen.getByRole('option', { name: 'Recently updated' }).hasAttribute('disabled')).toBe(
      true,
    )
    expect(asFragment()).toMatchSnapshot()
  })

  it('calls onChange when the selected option changes', () => {
    const handleChange = vi.fn()
    render(
      <Select label="Sort by" name="sort" onChange={handleChange}>
        <SelectOption value="published">Recently published</SelectOption>
        <SelectOption value="updated">Recently updated</SelectOption>
      </Select>,
    )

    fireEvent.change(screen.getByLabelText('Sort by'), { target: { value: 'updated' } })

    expect(handleChange).toHaveBeenCalledTimes(1)
  })

  it('forwards an object selectRef to the underlying select element', () => {
    const ref = createRef<HTMLSelectElement>()
    render(
      <Select label="Sort by" name="sort" selectRef={ref}>
        <SelectOption value="published">Recently published</SelectOption>
      </Select>,
    )

    expect(ref.current).toBe(screen.getByLabelText('Sort by'))
  })

  it('forwards a callback selectRef to the underlying select element', () => {
    const selectRef = vi.fn()
    render(
      <Select label="Sort by" name="sort" selectRef={selectRef}>
        <SelectOption value="published">Recently published</SelectOption>
      </Select>,
    )

    expect(selectRef).toHaveBeenCalledWith(screen.getByLabelText('Sort by'))
  })

  it('merges a consumer className onto the root wrapper', () => {
    const { asFragment, container } = render(
      <Select className="extra-class" label="Sort by" name="sort">
        <SelectOption value="published">Recently published</SelectOption>
      </Select>,
    )

    const root = container.querySelector('[data-component="select"]')
    expect(root?.classList.contains('extra-class')).toBe(true)
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders no label element when label is null', () => {
    const { asFragment } = render(
      <Select label={null} name="sort">
        <SelectOption value="published">Recently published</SelectOption>
      </Select>,
    )

    expect(screen.queryByText('Sort by')).toBeNull()
    expect(asFragment()).toMatchSnapshot()
  })

  it('applies a max-width for a fixed width', () => {
    const { asFragment } = render(
      <Select label="Sort by" name="sort" width={10}>
        <SelectOption value="published">Recently published</SelectOption>
      </Select>,
    )

    const select = screen.getByLabelText('Sort by')
    expect(select.style.maxWidth).toBe('11.5em')
    expect(asFragment()).toMatchSnapshot()
  })

  it('applies a width for a fluid width', () => {
    const { asFragment } = render(
      <Select label="Sort by" name="sort" width="one-half">
        <SelectOption value="published">Recently published</SelectOption>
      </Select>,
    )

    const select = screen.getByLabelText('Sort by')
    expect(select.style.width).toBe('50%')
    expect(asFragment()).toMatchSnapshot()
  })

  it('merges width styles with an explicit style prop', () => {
    const { asFragment } = render(
      <Select label="Sort by" name="sort" style={{ color: 'red' }} width="full">
        <SelectOption value="published">Recently published</SelectOption>
      </Select>,
    )

    const select = screen.getByLabelText('Sort by')
    expect(select.style.width).toBe('100%')
    expect(select.style.color).toBe('red')
    expect(asFragment()).toMatchSnapshot()
  })

  it.each(widths)('renders the %s width variant', (width) => {
    const { asFragment } = render(
      <Select label="Sort by" name="sort" width={width}>
        <SelectOption value="published">Recently published</SelectOption>
      </Select>,
    )

    expect(screen.getByLabelText('Sort by')).toBeDefined()
    expect(asFragment()).toMatchSnapshot()
  })
})
