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
    <Radios
      identifier="contact"
      legend="How would you like to be contacted?"
      name="contact"
      {...props}
    >
      <Radio
        id="contact_email"
        conditional={
          <>
            <label htmlFor="contact-email">Email address</label>
            <input id="contact-email" name="contactEmail" required />
          </>
        }
        label="Email"
        value="email"
      />
      <Radio
        id="contact_phone"
        conditional={
          <>
            <label htmlFor="contact-phone">Phone number</label>
            <input id="contact-phone" name="contactPhone" />
          </>
        }
        label="Phone"
        value="phone"
      />
      <Radio id="contact_post" label="Post" value="post" />
    </Radios>
  )
}

const conditionalFor = (container: HTMLElement, value: string) =>
  container.querySelector(`#conditional-contact_${value}`)

describe('Radios', () => {
  it('generates distinct IDs for groups that share a name', async () => {
    render(
      <>
        <Radios error="First error" hint="First hint" legend="First" name="action">
          <Radio
            conditional={<input aria-label="First detail" />}
            hint="First option hint"
            label="First option"
            value="same"
          />
        </Radios>
        <Radios error="Second error" hint="Second hint" legend="Second" name="action">
          <Radio
            conditional={<input aria-label="Second detail" />}
            hint="Second option hint"
            label="Second option"
            value="same"
          />
        </Radios>
      </>,
    )
    const first = screen.getByRole('radio', { name: 'First option' })
    const second = screen.getByRole('radio', { name: 'Second option' })

    expect(first.id).not.toBe(second.id)
    expect(first.getAttribute('aria-controls')).not.toBe(second.getAttribute('aria-controls'))
    expect(screen.getByRole('group', { name: 'First' })).toHaveAccessibleDescription(
      'First hint Error: First error',
    )
    expect(screen.getByRole('group', { name: 'Second' })).toHaveAccessibleDescription(
      'Second hint Error: Second error',
    )
    expect(first).toHaveAccessibleDescription('First hint Error: First error First option hint')
    expect(second).toHaveAccessibleDescription('Second hint Error: Second error Second option hint')
    await user.click(second)
    expect(screen.getByLabelText('Second detail')).toBeVisible()
    expect(screen.getByLabelText('First detail')).not.toBeVisible()
  })

  it('composes shared and option descriptions without duplicate ID references', () => {
    render(
      <Radios
        aria-describedby="outside contact-hint"
        hint="Group hint"
        identifier="contact"
        legend="Contact"
        name="contact"
      >
        <Radio
          aria-describedby="outside contact-hint"
          hint="Option hint"
          id="contact_email"
          label="Email"
          value="email"
        />
      </Radios>,
    )
    expect(screen.getByRole('radio')).toHaveAttribute(
      'aria-describedby',
      'outside contact-hint contact_email-hint',
    )
    expect(screen.getByRole('group')).toHaveAttribute('aria-describedby', 'outside contact-hint')
  })

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
    expect(conditionalFor(container, 'email')).toBeVisible()
    expect(conditionalFor(container, 'phone')).not.toBeVisible()
  })

  it('reveals the conditional content of the selected option only', async () => {
    const { container } = render(<ContactRadios defaultValue="email" />)

    await user.click(screen.getByLabelText('Phone'))

    expect(screen.getByLabelText('Phone')).toBeChecked()
    expect(conditionalFor(container, 'email')).not.toBeVisible()
    expect(conditionalFor(container, 'phone')).toBeVisible()
  })

  it('links conditional content without unsupported aria-expanded on radios', async () => {
    render(<ContactRadios />)

    const email = screen.getByLabelText('Email')
    expect(email.getAttribute('aria-controls')).toBe('conditional-contact_email')
    expect(email).not.toHaveAttribute('aria-expanded')

    await user.click(email)

    expect(screen.getByLabelText('Email address')).toBeVisible()
    for (const radio of screen.getAllByRole('radio'))
      expect(radio).not.toHaveAttribute('aria-expanded')
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
    expect(conditionalFor(container, 'phone')).not.toBeVisible()

    rerender(<ContactRadios onChange={handleChange} value="phone" />)

    expect(screen.getByLabelText('Phone')).toBeChecked()
    expect(conditionalFor(container, 'phone')).toBeVisible()
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

  it('associates custom IDs with labels and composes group and option descriptions', async () => {
    const groupBlur = vi.fn()
    const optionBlur = vi.fn()
    const { container } = render(
      <>
        <p id="consumer">Consumer description</p>
        <Radios
          aria-describedby="consumer"
          error="Group error"
          hint="Group hint"
          legend="Choose"
          name="choose"
          onBlur={groupBlur}
        >
          <Radio
            aria-describedby="consumer"
            conditional={<input aria-label="Details" />}
            error
            errorMessage="Option error"
            hint="Option hint"
            id="custom-id"
            label="Custom label"
            onBlur={optionBlur}
            value="custom"
          />
          <Radio error label="Other" value="other" />
        </Radios>
      </>,
    )
    const radio = screen.getByRole('radio', { name: 'Custom label' })
    expect(radio).toHaveAttribute('id', 'custom-id')
    expect(radio).toHaveAttribute('aria-controls', 'conditional-custom-id')
    expect(radio).toHaveAccessibleDescription(
      /Consumer description.*Group hint.*Group error.*Option hint.*Option error/,
    )
    expect(screen.getByRole('group', { name: 'Choose' })).toHaveAccessibleDescription(
      /Consumer description.*Group hint.*Group error/,
    )
    expect(screen.getByRole('group', { name: 'Choose' })).not.toHaveAttribute('aria-invalid')
    for (const option of screen.getAllByRole('radio')) {
      expect(option).not.toHaveAttribute('aria-invalid')
      expect(option).not.toHaveAttribute('aria-expanded')
      expect(option).toHaveAccessibleDescription(/Group error/)
    }
    expect(container.querySelectorAll('.radio__error-message')).toHaveLength(1)
    await user.click(screen.getByText('Custom label'))
    expect(radio).toBeChecked()
    expect(screen.getByLabelText('Details')).toBeVisible()
    await user.tab()
    expect(groupBlur).toHaveBeenCalledTimes(1)
    expect(optionBlur).toHaveBeenCalledTimes(1)
  })

  it('excludes inactive required controls from validation and submission while retaining values', async () => {
    const { container } = render(
      <form>
        <ContactRadios defaultValue="post" />
      </form>,
    )
    const form = container.querySelector('form')!
    const email = screen.getByLabelText('Email address')
    expect(email).toBeDisabled()
    expect(form.checkValidity()).toBe(true)
    expect(new FormData(form).has('contactEmail')).toBe(false)
    await user.click(screen.getByLabelText('Email'))
    expect(email).toBeEnabled()
    expect(form.checkValidity()).toBe(false)
    await user.type(email, 'jo@example.com')
    expect(form.checkValidity()).toBe(true)
    expect(new FormData(form).get('contactEmail')).toBe('jo@example.com')
    await user.click(screen.getByLabelText('Post'))
    expect(new FormData(form).has('contactEmail')).toBe(false)
    await user.click(screen.getByLabelText('Email'))
    expect(email).toHaveValue('jo@example.com')
  })

  it('disables arbitrary nested native controls without overriding consumer disabled state', async () => {
    const { container } = render(
      <form>
        <Radios legend="Choose" name="nested">
          <Radio
            label="Details"
            value="details"
            conditional={
              <div>
                <select aria-label="Choice" name="choice" required>
                  <option value="">Choose</option>
                  <option value="yes">Yes</option>
                </select>
                <fieldset>
                  <textarea aria-label="Explanation" name="explanation" required />
                </fieldset>
                <input aria-label="Unavailable detail" disabled name="unavailable" />
                <button type="submit">Submit detail</button>
              </div>
            }
          />
          <Radio label="Skip" value="skip" />
        </Radios>
      </form>,
    )
    const form = container.querySelector('form')!
    expect(screen.getByLabelText('Choice')).toBeDisabled()
    expect(screen.getByLabelText('Explanation')).toBeDisabled()
    expect(screen.getByText('Submit detail')).toBeDisabled()
    expect(form.checkValidity()).toBe(true)
    expect(Array.from(new FormData(form).keys())).toEqual([])
    await user.click(screen.getByLabelText('Details'))
    expect(screen.getByLabelText('Choice')).toBeEnabled()
    expect(screen.getByLabelText('Explanation')).toBeEnabled()
    expect(screen.getByText('Submit detail')).toBeEnabled()
    expect(screen.getByLabelText('Unavailable detail')).toBeDisabled()
    expect(form.checkValidity()).toBe(false)
  })
})
