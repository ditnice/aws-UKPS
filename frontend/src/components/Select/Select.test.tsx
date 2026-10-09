import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createRef } from 'react'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { inputWidthCases } from '@/test-utils/inputWidthCases'

import { Select, SelectOption } from './Select'

let user: ReturnType<typeof userEvent.setup>

beforeEach(() => {
  user = userEvent.setup()
})

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
    expect(select).toHaveValue('updated')
    expect(screen.getByRole('option', { name: 'Recently published' })).toBeInTheDocument()
    expect(asFragment()).toMatchSnapshot()
  })

  it('defaults the id to the name', () => {
    render(
      <Select label="Sort by" name="sort">
        <SelectOption value="published">Recently published</SelectOption>
      </Select>,
    )

    expect(screen.getByLabelText('Sort by').getAttribute('id')).toBe('sort')
  })

  it('supports an explicit id', () => {
    render(
      <Select id="sort-select" label="Sort by" name="sort">
        <SelectOption value="published">Recently published</SelectOption>
      </Select>,
    )

    expect(screen.getByLabelText('Sort by').getAttribute('id')).toBe('sort-select')
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
    expect(screen.getByText('Select a location')).toBeInTheDocument()
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
    render(
      <Select disabled label="Sort by" name="sort" onChange={handleChange} required>
        <SelectOption value="published">Recently published</SelectOption>
        <SelectOption disabled value="updated">
          Recently updated
        </SelectOption>
      </Select>,
    )

    const select = screen.getByLabelText('Sort by') as HTMLSelectElement
    expect(select).toBeDisabled()
    expect(select.required).toBe(true)
    expect(screen.getByRole('option', { name: 'Recently updated' }).hasAttribute('disabled')).toBe(
      true,
    )
  })

  it('calls onChange when the selected option changes', async () => {
    const handleChange = vi.fn()
    render(
      <Select label="Sort by" name="sort" onChange={handleChange}>
        <SelectOption value="published">Recently published</SelectOption>
        <SelectOption value="updated">Recently updated</SelectOption>
      </Select>,
    )

    await user.selectOptions(screen.getByLabelText('Sort by'), 'updated')

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
    const { container } = render(
      <Select className="extra-class" label="Sort by" name="sort">
        <SelectOption value="published">Recently published</SelectOption>
      </Select>,
    )

    const root = container.querySelector('[data-component="select"]')
    expect(root?.classList.contains('extra-class')).toBe(true)
  })

  it('renders no label element when label is null', () => {
    render(
      <Select label={null} name="sort">
        <SelectOption value="published">Recently published</SelectOption>
      </Select>,
    )

    expect(screen.queryByText('Sort by')).toBeNull()
  })

  it('applies a max-width for a fixed width', () => {
    render(
      <Select label="Sort by" name="sort" width={10}>
        <SelectOption value="published">Recently published</SelectOption>
      </Select>,
    )

    const select = screen.getByLabelText('Sort by')
    expect(select.style.maxWidth).toBe('11.5em')
  })

  it('applies a width for a fluid width', () => {
    render(
      <Select label="Sort by" name="sort" width="one-half">
        <SelectOption value="published">Recently published</SelectOption>
      </Select>,
    )

    const select = screen.getByLabelText('Sort by')
    expect(select.style.width).toBe('50%')
  })

  it('merges width styles with an explicit style prop', () => {
    render(
      <Select label="Sort by" name="sort" style={{ color: 'red' }} width="full">
        <SelectOption value="published">Recently published</SelectOption>
      </Select>,
    )

    const select = screen.getByLabelText('Sort by')
    expect(select.style.width).toBe('100%')
    expect(select.style.color).toBe('red')
  })

  it.each(inputWidthCases)(
    'renders the $width width variant',
    ({ width, maxWidth, fluidWidth }) => {
      render(
        <Select label="Sort by" name="sort" width={width}>
          <SelectOption value="published">Recently published</SelectOption>
        </Select>,
      )

      const select = screen.getByLabelText('Sort by')
      expect(select.style.maxWidth).toBe(maxWidth)
      expect(select.style.width).toBe(fluidWidth)
    },
  )
})
