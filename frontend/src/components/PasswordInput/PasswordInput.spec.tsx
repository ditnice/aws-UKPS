import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { createRef } from 'react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import type { InputWidth } from '@/components/Input/Input'

import { PasswordInput } from './PasswordInput'

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

describe('PasswordInput', () => {
  it('renders masked by default with a "Show" toggle', () => {
    const { asFragment } = render(<PasswordInput label="Password" name="password" />)

    const input = screen.getByLabelText('Password')
    expect(input.getAttribute('type')).toBe('password')

    const toggle = screen.getByRole('button', { name: 'Show password' })
    expect(toggle.textContent).toBe('Show')
    expect(toggle.getAttribute('type')).toBe('button')
    expect(toggle.getAttribute('aria-controls')).toBe('password')

    expect(screen.getByText('Your password is hidden')).toBeDefined()
    expect(asFragment()).toMatchSnapshot()
  })

  it('reveals the password when the toggle is clicked', () => {
    const { asFragment } = render(<PasswordInput label="Password" name="password" />)

    fireEvent.click(screen.getByRole('button', { name: 'Show password' }))

    const input = screen.getByLabelText('Password')
    expect(input.getAttribute('type')).toBe('text')

    const toggle = screen.getByRole('button', { name: 'Hide password' })
    expect(toggle.textContent).toBe('Hide')

    expect(screen.getByText('Your password is visible')).toBeDefined()
    expect(asFragment()).toMatchSnapshot()
  })

  it('masks the password again when the toggle is clicked a second time', () => {
    const { asFragment } = render(<PasswordInput label="Password" name="password" />)

    fireEvent.click(screen.getByRole('button', { name: 'Show password' }))
    fireEvent.click(screen.getByRole('button', { name: 'Hide password' }))

    expect(screen.getByLabelText('Password').getAttribute('type')).toBe('password')
    expect(screen.getByText('Your password is hidden')).toBeDefined()
    expect(asFragment()).toMatchSnapshot()
  })

  it('always sets spellcheck and autocapitalize off, regardless of other props', () => {
    const { asFragment } = render(<PasswordInput label="Password" name="password" required />)

    const input = screen.getByLabelText('Password')
    expect(input.getAttribute('spellcheck')).toBe('false')
    expect(input.getAttribute('autocapitalize')).toBe('none')
    expect(input.hasAttribute('required')).toBe(true)
    expect(asFragment()).toMatchSnapshot()
  })

  it('defaults autocomplete to current-password but allows an override', () => {
    const { asFragment, rerender } = render(<PasswordInput label="Password" name="password" />)
    expect(screen.getByLabelText('Password').getAttribute('autocomplete')).toBe('current-password')
    expect(asFragment()).toMatchSnapshot()

    rerender(<PasswordInput autoComplete="new-password" label="Password" name="password" />)
    expect(screen.getByLabelText('Password').getAttribute('autocomplete')).toBe('new-password')
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders a hint', () => {
    const { asFragment } = render(
      <PasswordInput hint="At least 8 characters" label="Password" name="password" />,
    )

    expect(screen.getByText('At least 8 characters')).toBeDefined()
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders an error message', () => {
    const { asFragment } = render(
      <PasswordInput error errorMessage="Enter your password" label="Password" name="password" />,
    )

    expect(screen.getByText('Enter your password')).toBeDefined()
    expect(asFragment()).toMatchSnapshot()
  })

  it('uses the NDS inverse button variant for the toggle', () => {
    const { asFragment } = render(<PasswordInput label="Password" name="password" />)

    const toggle = screen.getByRole('button', { name: 'Show password' })
    expect(toggle.className).toMatch(/\bbtn\b/)
    expect(toggle.className).toMatch(/\bbtn--inverse\b/)
    expect(asFragment()).toMatchSnapshot()
  })

  it('applies a max-width for a fixed width', () => {
    const { asFragment } = render(<PasswordInput label="Password" name="password" width={10} />)

    const input = screen.getByLabelText('Password')
    expect(input.style.maxWidth).toBe('11.5em')
    expect(asFragment()).toMatchSnapshot()
  })

  it('applies a width for a fluid width', () => {
    const { asFragment } = render(
      <PasswordInput label="Password" name="password" width="one-half" />,
    )

    const input = screen.getByLabelText('Password')
    expect(input.style.width).toBe('50%')
    expect(asFragment()).toMatchSnapshot()
  })

  it('merges width styles with an explicit style prop', () => {
    const { asFragment } = render(
      <PasswordInput label="Password" name="password" style={{ color: 'red' }} width="full" />,
    )

    const input = screen.getByLabelText('Password')
    expect(input.style.width).toBe('100%')
    expect(input.style.color).toBe('red')
    expect(asFragment()).toMatchSnapshot()
  })

  it('forwards an object inputRef to the underlying input element', () => {
    const ref = createRef<HTMLInputElement>()
    render(<PasswordInput inputRef={ref} label="Password" name="password" />)

    expect(ref.current).toBe(screen.getByLabelText('Password'))
  })

  it('forwards a callback inputRef to the underlying input element', () => {
    const inputRef = vi.fn()
    render(<PasswordInput inputRef={inputRef} label="Password" name="password" />)

    expect(inputRef).toHaveBeenCalledWith(screen.getByLabelText('Password'))
  })

  it('merges a consumer className onto the root wrapper', () => {
    const { asFragment, container } = render(
      <PasswordInput className="extra-class" label="Password" name="password" />,
    )

    const root = container.querySelector('[data-component="input"]')
    expect(root?.classList.contains('extra-class')).toBe(true)
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders no label element when label is null', () => {
    const { asFragment } = render(<PasswordInput label={null} name="password" />)

    expect(screen.queryByText('Password')).toBeNull()
    expect(asFragment()).toMatchSnapshot()
  })

  it('marks the root wrapper with data-component="input"', () => {
    const { asFragment, container } = render(<PasswordInput label="Password" name="password" />)

    expect(container.querySelector('[data-component="input"]')).not.toBeNull()
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders an empty error message when error is set without errorMessage', () => {
    const { asFragment, container } = render(
      <PasswordInput error label="Password" name="password" />,
    )

    const errorParagraph = container.querySelector('p')
    expect(errorParagraph?.textContent).toBe('')
    expect(asFragment()).toMatchSnapshot()
  })

  it('forwards arbitrary passthrough props onto the input', () => {
    const handleChange = vi.fn()
    const { asFragment } = render(
      <PasswordInput
        disabled
        label="Password"
        name="password"
        onChange={handleChange}
        placeholder="Enter password"
      />,
    )

    const input = screen.getByLabelText('Password') as HTMLInputElement
    expect(input.placeholder).toBe('Enter password')
    expect(input.disabled).toBe(true)
    expect(asFragment()).toMatchSnapshot()
  })

  it.each(widths)('renders the %s width variant', (width) => {
    const { asFragment } = render(<PasswordInput label="Password" name="password" width={width} />)

    expect(screen.getByLabelText('Password')).toBeDefined()
    expect(asFragment()).toMatchSnapshot()
  })
})
