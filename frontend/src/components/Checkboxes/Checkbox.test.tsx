import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createRef, useState } from 'react'
import { describe, expect, it, vi } from 'vitest'

import { FormGroup } from '@/components/FormGroup/FormGroup'

import { Checkbox } from './Checkboxes'

describe('Checkbox', () => {
  it('associates custom IDs and composes group, option and consumer descriptions once', () => {
    const ref = createRef<HTMLInputElement>()
    render(
      <>
        <p id="extra">More context</p>
        <FormGroup identifier="contact" legend="Contact" hint="Group hint" error="Group error">
          <Checkbox
            aria-describedby="extra contact-hint"
            error
            errorMessage="Option error"
            hint="Option hint"
            id="custom"
            label="Email"
            name="contact"
            ref={ref}
            value="email"
          />
        </FormGroup>
      </>,
    )
    const input = screen.getByRole('checkbox', { name: 'Email' })
    expect(input).toBe(ref.current)
    expect(screen.getByLabelText('Email')).toBe(input)
    expect(input).toHaveAttribute('id', 'custom')
    expect(input).toHaveAttribute('aria-invalid', 'true')
    expect(input).toHaveAccessibleDescription(
      'More context Group hint Error: Group error Option hint Error: Option error',
    )
    expect(
      input
        .getAttribute('aria-describedby')
        ?.split(' ')
        .filter((id) => id === 'contact-hint'),
    ).toHaveLength(1)
  })

  it('supports native defaults, keyboard, blur, disabled, required, submission and reset', async () => {
    const user = userEvent.setup()
    const blur = vi.fn()
    render(
      <form aria-label="Preferences">
        <Checkbox
          defaultChecked
          label="Email"
          name="contact"
          onBlur={blur}
          required
          value="email"
        />
        <Checkbox defaultChecked disabled label="Post" name="contact" value="post" />
        <button type="reset">Reset</button>
      </form>,
    )
    const form = screen.getByRole('form') as HTMLFormElement
    const input = screen.getByRole('checkbox', { name: 'Email' })
    expect(input).toBeRequired()
    expect(new FormData(form).getAll('contact')).toEqual(['email'])
    await user.tab()
    expect(input).toHaveFocus()
    await user.keyboard(' ')
    expect(input).not.toBeChecked()
    await user.tab()
    expect(blur).toHaveBeenCalledOnce()
    await user.click(screen.getByRole('button', { name: 'Reset' }))
    expect(input).toBeChecked()
    expect(new FormData(form).getAll('contact')).toEqual(['email'])
  })

  it('supports parent-controlled state and never renders an empty error', async () => {
    function Controlled() {
      const [checked, setChecked] = useState(false)
      return (
        <Checkbox
          checked={checked}
          error
          label="Agree"
          name="agree"
          onChange={(event) => setChecked(event.target.checked)}
          value="yes"
        />
      )
    }
    const { container } = render(<Controlled />)
    await userEvent.setup().click(screen.getByRole('checkbox'))
    expect(screen.getByRole('checkbox')).toBeChecked()
    expect(container.querySelector('.checkbox__error-message')).toBeNull()
    expect(screen.getByRole('checkbox')).not.toHaveAttribute('aria-describedby')
  })

  it('generates unique label associations for standalone controls sharing a name', () => {
    render(
      <>
        <Checkbox name="contact" value="email" label="Email" />
        <Checkbox name="contact" value="phone" label="Phone" />
      </>,
    )
    expect(screen.getByLabelText('Email').id).not.toBe(screen.getByLabelText('Phone').id)
  })
})
