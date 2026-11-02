import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { Radio, Radios } from './Radios'

let user: ReturnType<typeof userEvent.setup>

beforeEach(() => {
  user = userEvent.setup()
})

function ContactRadios(props: Partial<Parameters<typeof Radios>[0]>) {
  return (
    <Radios legend="How would you like to be contacted?" name="contact" {...props}>
      <Radio
        conditional={
          <>
            <label htmlFor="contact-email">Email address</label>
            <input id="contact-email" name="contactEmail" />
          </>
        }
        label="Email"
        value="email"
      />
      <Radio
        conditional={
          <>
            <label htmlFor="contact-phone">Phone number</label>
            <input id="contact-phone" name="contactPhone" />
          </>
        }
        label="Phone"
        value="phone"
      />
      <Radio label="Post" value="post" />
    </Radios>
  )
}

const conditionalFor = (container: HTMLElement, value: string) =>
  container.querySelector(`#conditional-contact_${value}`)

describe('Radios', () => {
  it('renders a legend, hint and options sharing the group name', () => {
    const { asFragment } = render(<ContactRadios hint="Select one option" />)

    expect(screen.getByRole('group', { name: 'How would you like to be contacted?' })).toBeVisible()
    expect(screen.getByText('Select one option')).toBeInTheDocument()
    for (const radio of screen.getAllByRole('radio')) {
      expect(radio.getAttribute('name')).toBe('contact')
    }
    expect(asFragment()).toMatchSnapshot()
  })

  it('reveals the conditional content of the default selection', () => {
    const { container } = render(<ContactRadios defaultValue="email" />)

    expect(screen.getByLabelText('Email')).toBeChecked()
    expect(conditionalFor(container, 'email')?.className).not.toMatch(/conditionalHidden/)
    expect(conditionalFor(container, 'phone')?.className).toMatch(/conditionalHidden/)
  })

  it('reveals the conditional content of the selected option only', async () => {
    const { container } = render(<ContactRadios defaultValue="email" />)

    await user.click(screen.getByLabelText('Phone'))

    expect(screen.getByLabelText('Phone')).toBeChecked()
    expect(conditionalFor(container, 'email')?.className).toMatch(/conditionalHidden/)
    expect(conditionalFor(container, 'phone')?.className).not.toMatch(/conditionalHidden/)
  })

  it('links options to their conditional content with aria-controls and aria-expanded', async () => {
    render(<ContactRadios />)

    const email = screen.getByLabelText('Email')
    expect(email.getAttribute('aria-controls')).toBe('conditional-contact_email')
    expect(email.getAttribute('aria-expanded')).toBe('false')

    await user.click(email)

    expect(email.getAttribute('aria-expanded')).toBe('true')
    expect(screen.getByLabelText('Phone').getAttribute('aria-expanded')).toBe('false')
  })

  it('omits aria-controls and aria-expanded on options without conditional content', () => {
    render(<ContactRadios />)

    const post = screen.getByLabelText('Post')
    expect(post.hasAttribute('aria-controls')).toBe(false)
    expect(post.hasAttribute('aria-expanded')).toBe(false)
  })

  it('keeps hidden conditional content mounted so entered values are kept', async () => {
    render(<ContactRadios />)

    await user.click(screen.getByLabelText('Email'))
    await user.type(screen.getByLabelText('Email address'), 'jo@example.com')
    await user.click(screen.getByLabelText('Post'))

    expect(screen.getByLabelText('Email address')).toBeInTheDocument()

    await user.click(screen.getByLabelText('Email'))

    expect(screen.getByLabelText('Email address')).toHaveValue('jo@example.com')
  })

  it('calls onChange with the selected value and follows the value prop when controlled', async () => {
    const handleChange = vi.fn()
    const { container, rerender } = render(<ContactRadios onChange={handleChange} value="email" />)

    await user.click(screen.getByLabelText('Phone'))

    expect(handleChange).toHaveBeenCalledWith('phone')
    expect(screen.getByLabelText('Email')).toBeChecked()
    expect(conditionalFor(container, 'phone')?.className).toMatch(/conditionalHidden/)

    rerender(<ContactRadios onChange={handleChange} value="phone" />)

    expect(screen.getByLabelText('Phone')).toBeChecked()
    expect(conditionalFor(container, 'phone')?.className).not.toMatch(/conditionalHidden/)
  })

  it('selects nothing when controlled with a null value', () => {
    render(<ContactRadios onChange={vi.fn()} value={null} />)

    for (const radio of screen.getAllByRole('radio')) {
      expect(radio).not.toBeChecked()
    }
  })

  it('calls onBlur when an option loses focus', async () => {
    const handleBlur = vi.fn()
    render(<ContactRadios onBlur={handleBlur} />)

    await user.click(screen.getByLabelText('Post'))
    await user.tab()

    expect(handleBlur).toHaveBeenCalled()
  })

  it('renders the error message and styles every option as invalid', () => {
    const { asFragment, container } = render(
      <ContactRadios error="Select how you would like to be contacted" />,
    )

    expect(screen.getByText('Select how you would like to be contacted')).toBeInTheDocument()
    expect(container.querySelectorAll('.radio--error')).toHaveLength(3)
    expect(asFragment()).toMatchSnapshot()
  })

  it('does not render conditional content for inline radios', () => {
    const { container } = render(<ContactRadios defaultValue="email" inline />)

    expect(container.querySelector('[data-component="radios-conditional"]')).toBeNull()
    expect(screen.getByLabelText('Email').hasAttribute('aria-controls')).toBe(false)
  })

  it('forwards native input props to the option', () => {
    render(
      <Radios legend="Pick one" name="pick">
        <Radio disabled label="Unavailable" value="unavailable" />
      </Radios>,
    )

    expect(screen.getByLabelText('Unavailable')).toBeDisabled()
  })

  it('merges a consumer className onto the root wrapper', () => {
    const { container } = render(<ContactRadios className="extra-class" />)

    const root = container.querySelector('[data-component="radios"]')
    expect(root?.classList.contains('extra-class')).toBe(true)
  })

  it('throws when Radio is rendered outside Radios', () => {
    vi.spyOn(console, 'error').mockImplementation(() => {})

    expect(() => render(<Radio label="Orphan" value="orphan" />)).toThrow(
      'Radio must be rendered inside Radios',
    )
  })
})
