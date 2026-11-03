import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { useState } from 'react'
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
            <input id="contact-email" name="contactEmail" required />
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
  it('composes shared and option descriptions without duplicate ID references', () => {
    render(
      <Radios
        aria-describedby="outside contact-hint"
        hint="Group hint"
        legend="Contact"
        name="contact"
      >
        <Radio
          aria-describedby="outside contact-hint"
          hint="Option hint"
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
    expect(screen.getByRole('group', { name: 'Choose' })).toHaveAttribute('aria-invalid', 'true')
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

  it.each([undefined, 'email'])(
    'synchronises native reset with the uncontrolled default %s',
    async (defaultValue) => {
      const onChange = vi.fn()
      const { container } = render(
        <form>
          <ContactRadios defaultValue={defaultValue} onChange={onChange} />
          <button type="reset">Reset</button>
        </form>,
      )
      await user.click(screen.getByLabelText('Phone'))
      await user.click(screen.getByRole('button', { name: 'Reset' }))
      await waitFor(() => expect(conditionalFor(container, 'phone')).not.toBeVisible())
      expect(screen.getByLabelText('Email')).toHaveProperty('checked', defaultValue === 'email')
      expect(screen.getByLabelText('Phone')).not.toBeChecked()
      expect(screen.getByLabelText('Email address')).toHaveProperty(
        'willValidate',
        defaultValue === 'email',
      )
      expect(onChange).toHaveBeenCalledTimes(1)
    },
  )

  it('keeps controlled selection parent-owned across native reset', async () => {
    const onChange = vi.fn()
    const { container, rerender } = render(
      <form>
        <ContactRadios value="email" onChange={onChange} />
        <button type="reset">Reset</button>
      </form>,
    )
    rerender(
      <form>
        <ContactRadios value="phone" onChange={onChange} />
        <button type="reset">Reset</button>
      </form>,
    )
    await user.click(screen.getByRole('button', { name: 'Reset' }))
    expect(screen.getByLabelText('Phone')).toBeChecked()
    expect(conditionalFor(container, 'phone')).toBeVisible()
    expect(onChange).not.toHaveBeenCalled()
  })

  it('respects cancelled native resets', async () => {
    render(
      <form onReset={(event) => event.preventDefault()}>
        <ContactRadios defaultValue="email" />
        <button type="reset">Reset</button>
      </form>,
    )
    await user.click(screen.getByLabelText('Phone'))
    await user.click(screen.getByRole('button', { name: 'Reset' }))
    expect(screen.getByLabelText('Phone')).toBeChecked()
    expect(screen.getByLabelText('Phone number')).toBeVisible()
  })

  it('follows a controlled parent that changes its value in onReset', async () => {
    function ControlledForm() {
      const [value, setValue] = useState('email')
      return (
        <form onReset={() => setValue('phone')}>
          <ContactRadios value={value} onChange={setValue} />
          <button type="reset">Reset</button>
        </form>
      )
    }
    render(<ControlledForm />)
    await user.click(screen.getByRole('button', { name: 'Reset' }))
    expect(screen.getByLabelText('Phone')).toBeChecked()
    expect(screen.getByLabelText('Email')).not.toBeChecked()
    expect(screen.getByLabelText('Phone number')).toBeVisible()
  })

  it('listens to the owning form of externally associated inputs', async () => {
    render(
      <>
        <form id="external-form">
          <button type="reset">Reset external</button>
        </form>
        <Radios defaultValue="first" legend="External choice" name="external-choice">
          <Radio form="external-form" label="First" value="first" />
          <Radio
            conditional={
              <input aria-label="External detail" form="external-form" name="detail" required />
            }
            form="external-form"
            label="Second"
            value="second"
          />
        </Radios>
      </>,
    )
    await user.click(screen.getByLabelText('Second'))
    expect(screen.getByLabelText('External detail')).toBeVisible()
    await user.click(screen.getByRole('button', { name: 'Reset external' }))
    expect(screen.getByLabelText('First')).toBeChecked()
    expect(screen.getByLabelText('Second')).not.toBeChecked()
    expect(screen.getByLabelText('External detail')).not.toBeVisible()
    expect(screen.getByLabelText('External detail')).toBeDisabled()
  })

  it('subscribes to reset when options arrive after the first render', async () => {
    function LateOptions({ loaded }: { loaded: boolean }) {
      return (
        <form>
          <Radios defaultValue="first" legend="Late choice" name="late-choice">
            {loaded && <Radio label="First" value="first" />}
            {loaded && (
              <Radio
                label="Second"
                value="second"
                conditional={<input aria-label="Late detail" />}
              />
            )}
          </Radios>
          <button type="reset">Reset late choice</button>
        </form>
      )
    }
    const { rerender } = render(<LateOptions loaded={false} />)
    expect(screen.queryByRole('radio')).not.toBeInTheDocument()
    rerender(<LateOptions loaded />)
    await user.click(screen.getByLabelText('Second'))
    expect(screen.getByLabelText('Late detail')).toBeVisible()
    await user.click(screen.getByRole('button', { name: 'Reset late choice' }))
    expect(screen.getByLabelText('First')).toBeChecked()
    expect(screen.getByLabelText('Second')).not.toBeChecked()
    expect(screen.getByLabelText('Late detail')).not.toBeVisible()
    expect(screen.getByLabelText('Late detail')).toBeDisabled()
  })

  it('refreshes reset subscriptions when form ownership changes', async () => {
    function ReassociatedOptions({ form }: { form: string }) {
      return (
        <>
          <form id="old-form">
            <button type="reset">Reset old form</button>
          </form>
          <form id="new-form">
            <button type="reset">Reset new form</button>
          </form>
          <Radios defaultValue="first" legend="Moving choice" name="moving-choice">
            <Radio form={form} label="First" value="first" />
            <Radio
              form={form}
              label="Second"
              value="second"
              conditional={<input aria-label="Moving detail" form={form} />}
            />
          </Radios>
        </>
      )
    }
    const { rerender } = render(<ReassociatedOptions form="old-form" />)
    await user.click(screen.getByLabelText('Second'))
    rerender(<ReassociatedOptions form="new-form" />)
    await user.click(screen.getByRole('button', { name: 'Reset old form' }))
    expect(screen.getByLabelText('Second')).toBeChecked()
    expect(screen.getByLabelText('Moving detail')).toBeVisible()
    await user.click(screen.getByRole('button', { name: 'Reset new form' }))
    expect(screen.getByLabelText('First')).toBeChecked()
    expect(screen.getByLabelText('Second')).not.toBeChecked()
    expect(screen.getByLabelText('Moving detail')).not.toBeVisible()
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
