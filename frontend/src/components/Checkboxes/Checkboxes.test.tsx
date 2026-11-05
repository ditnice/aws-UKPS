import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { useState } from 'react'
import { describe, expect, it, vi } from 'vitest'

import { Checkbox, Checkboxes } from './Checkboxes'

import type { CheckboxesProps } from './Checkboxes'

function Options(props: Partial<CheckboxesProps>) {
  return (
    <Checkboxes legend="Contact methods" name="contact" {...props}>
      <Checkbox label="Email" value="email" />
      <Checkbox label="Phone" value="phone" />
      <Checkbox label="None" value="none" />
      <Checkbox disabled label="Post" value="post" />
    </Checkboxes>
  )
}

describe('Checkboxes', () => {
  it('generates distinct IDs for groups that share a name', () => {
    render(
      <>
        <Checkboxes error="First error" hint="First hint" legend="First" name="contact">
          <Checkbox hint="First option hint" label="First option" value="same" />
        </Checkboxes>
        <Checkboxes error="Second error" hint="Second hint" legend="Second" name="contact">
          <Checkbox hint="Second option hint" label="Second option" value="same" />
        </Checkboxes>
      </>,
    )
    const first = screen.getByRole('checkbox', { name: 'First option' })
    const second = screen.getByRole('checkbox', { name: 'Second option' })

    expect(first.id).not.toBe(second.id)
    expect(first).toHaveAccessibleDescription('First hint Error: First error First option hint')
    expect(second).toHaveAccessibleDescription('Second hint Error: Second error Second option hint')
  })

  it('owns option names and checked state rather than leaking standalone defaults', () => {
    render(
      <Checkboxes legend="Contact" name="contact" value={['email']}>
        <Checkbox
          checked={false}
          defaultChecked={false}
          label="Email"
          name="incorrect"
          value="email"
        />
        <Checkbox checked defaultChecked label="Phone" name="incorrect" value="phone" />
      </Checkboxes>,
    )
    expect(screen.getByLabelText('Email')).toBeChecked()
    expect(screen.getByLabelText('Phone')).not.toBeChecked()
    for (const input of screen.getAllByRole('checkbox'))
      expect(input).toHaveAttribute('name', 'contact')
  })

  it('keeps multiple selections in insertion order with immutable duplicate-free arrays', async () => {
    const user = userEvent.setup()
    const initial = ['phone', 'phone']
    const change = vi.fn()
    render(<Options defaultValue={initial} onChange={change} />)
    await user.click(screen.getByLabelText('Email'))
    expect(change).toHaveBeenLastCalledWith(['phone', 'email'])
    await user.click(screen.getByLabelText('None'))
    expect(change).toHaveBeenLastCalledWith(['phone', 'email', 'none'])
    await user.click(screen.getByLabelText('Phone'))
    expect(change).toHaveBeenLastCalledWith(['email', 'none'])
    await user.click(screen.getByLabelText('Phone'))
    expect(change).toHaveBeenLastCalledWith(['email', 'none', 'phone'])
    expect(initial).toEqual(['phone', 'phone'])
    await user.click(screen.getByLabelText('Post'))
    expect(change).toHaveBeenCalledTimes(4)
  })

  it('composes metadata, blur and native submission without making every option required', async () => {
    const blur = vi.fn()
    render(
      <form aria-label="Preferences">
        <Options
          defaultValue={['phone', 'email', 'post']}
          error="Select an option"
          hint="Select all"
          inline
          onBlur={blur}
        />
      </form>,
    )
    expect(screen.getByRole('group')).toHaveAccessibleDescription(
      'Select all Error: Select an option',
    )
    for (const checkbox of screen.getAllByRole('checkbox')) {
      expect(checkbox).toHaveAttribute('name', 'contact')
      expect(checkbox).toHaveAccessibleDescription('Select all Error: Select an option')
      expect(checkbox).not.toHaveAttribute('aria-invalid')
      expect(checkbox).not.toBeRequired()
      expect(checkbox.parentElement).toHaveClass('checkbox--inline', 'checkbox--error')
    }
    expect(new FormData(screen.getByRole('form') as HTMLFormElement).getAll('contact')).toEqual([
      'email',
      'phone',
    ])
    const user = userEvent.setup()
    await user.tab()
    await user.tab()
    expect(blur).toHaveBeenCalledOnce()
  })

  it('supports controlled updates and parent ownership', async () => {
    function Controlled() {
      const [value, setValue] = useState(['phone'])
      return <Options value={value} onChange={setValue} />
    }
    const { rerender } = render(<Controlled />)
    await userEvent.setup().click(screen.getByLabelText('Email'))
    expect(screen.getByLabelText('Phone')).toBeChecked()
    expect(screen.getByLabelText('Email')).toBeChecked()
    const change = vi.fn()
    rerender(<Options value={[]} onChange={change} />)
    await userEvent.setup().click(screen.getByLabelText('Phone'))
    expect(change).toHaveBeenCalledWith(['phone'])
    expect(screen.getByLabelText('Phone')).not.toBeChecked()
  })
})
