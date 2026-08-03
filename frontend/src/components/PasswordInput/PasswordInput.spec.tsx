import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { createRef } from 'react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { PasswordInput } from './PasswordInput'

afterEach(cleanup)

describe('PasswordInput', () => {
  it('renders masked by default with a "Show" toggle', () => {
    render(<PasswordInput label="Password" name="password" />)

    const input = screen.getByLabelText('Password')
    expect(input.getAttribute('type')).toBe('password')

    const toggle = screen.getByRole('button', { name: 'Show password' })
    expect(toggle.textContent).toBe('Show')
    expect(toggle.getAttribute('type')).toBe('button')
    expect(toggle.getAttribute('aria-controls')).toBe('password')

    expect(screen.getByText('Your password is hidden')).toBeDefined()
  })

  it('reveals the password when the toggle is clicked', () => {
    render(<PasswordInput label="Password" name="password" />)

    fireEvent.click(screen.getByRole('button', { name: 'Show password' }))

    const input = screen.getByLabelText('Password')
    expect(input.getAttribute('type')).toBe('text')

    const toggle = screen.getByRole('button', { name: 'Hide password' })
    expect(toggle.textContent).toBe('Hide')

    expect(screen.getByText('Your password is visible')).toBeDefined()
  })

  it('masks the password again when the toggle is clicked a second time', () => {
    render(<PasswordInput label="Password" name="password" />)

    fireEvent.click(screen.getByRole('button', { name: 'Show password' }))
    fireEvent.click(screen.getByRole('button', { name: 'Hide password' }))

    expect(screen.getByLabelText('Password').getAttribute('type')).toBe('password')
    expect(screen.getByText('Your password is hidden')).toBeDefined()
  })

  it('always sets spellcheck and autocapitalize off, regardless of other props', () => {
    render(<PasswordInput label="Password" name="password" required />)

    const input = screen.getByLabelText('Password')
    expect(input.getAttribute('spellcheck')).toBe('false')
    expect(input.getAttribute('autocapitalize')).toBe('none')
    expect(input.hasAttribute('required')).toBe(true)
  })

  it('defaults autocomplete to current-password but allows an override', () => {
    const { rerender } = render(<PasswordInput label="Password" name="password" />)
    expect(screen.getByLabelText('Password').getAttribute('autocomplete')).toBe('current-password')

    rerender(<PasswordInput autoComplete="new-password" label="Password" name="password" />)
    expect(screen.getByLabelText('Password').getAttribute('autocomplete')).toBe('new-password')
  })

  it('renders a hint', () => {
    render(<PasswordInput hint="At least 8 characters" label="Password" name="password" />)

    expect(screen.getByText('At least 8 characters')).toBeDefined()
  })

  it('renders an error message', () => {
    render(
      <PasswordInput error errorMessage="Enter your password" label="Password" name="password" />,
    )

    expect(screen.getByText('Enter your password')).toBeDefined()
  })

  it('uses the NDS inverse button variant for the toggle', () => {
    render(<PasswordInput label="Password" name="password" />)

    const toggle = screen.getByRole('button', { name: 'Show password' })
    expect(toggle.className).toMatch(/\bbtn\b/)
    expect(toggle.className).toMatch(/\bbtn--inverse\b/)
  })

  it('applies a max-width for a fixed width', () => {
    render(<PasswordInput label="Password" name="password" width={10} />)

    const input = screen.getByLabelText('Password')
    expect(input.style.maxWidth).toBe('11.5em')
  })

  it('applies a width for a fluid width', () => {
    render(<PasswordInput label="Password" name="password" width="one-half" />)

    const input = screen.getByLabelText('Password')
    expect(input.style.width).toBe('50%')
  })

  it('merges width styles with an explicit style prop', () => {
    render(<PasswordInput label="Password" name="password" style={{ color: 'red' }} width="full" />)

    const input = screen.getByLabelText('Password')
    expect(input.style.width).toBe('100%')
    expect(input.style.color).toBe('red')
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
    const { container } = render(
      <PasswordInput className="extra-class" label="Password" name="password" />,
    )

    const root = container.querySelector('[data-component="input"]')
    expect(root?.classList.contains('extra-class')).toBe(true)
  })

  it('renders no label element when label is null', () => {
    render(<PasswordInput label={null} name="password" />)

    expect(screen.queryByText('Password')).toBeNull()
  })

  it('marks the root wrapper with data-component="input"', () => {
    const { container } = render(<PasswordInput label="Password" name="password" />)

    expect(container.querySelector('[data-component="input"]')).not.toBeNull()
  })

  it('renders an empty error message when error is set without errorMessage', () => {
    const { container } = render(<PasswordInput error label="Password" name="password" />)

    const errorParagraph = container.querySelector('p')
    expect(errorParagraph?.textContent).toBe('')
  })

  it('forwards arbitrary passthrough props onto the input', () => {
    const handleChange = vi.fn()
    render(
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
  })
})
