import { act, render, screen } from '@testing-library/react'
import { createRef } from 'react'
import { hydrateRoot } from 'react-dom/client'
import { renderToString } from 'react-dom/server'
import { describe, expect, it, vi } from 'vitest'

import { FormGroup } from './FormGroup'

describe('FormGroup', () => {
  it('preserves generated description IDs across server rendering and hydration', async () => {
    const tree = (
      <>
        <FormGroup legend="First" hint="First hint" />
        <FormGroup legend="Second" hint="Second hint" />
      </>
    )
    const container = document.createElement('div')
    container.innerHTML = renderToString(tree)
    document.body.append(container)
    const ids = Array.from(container.querySelectorAll('fieldset'), (group) =>
      group.getAttribute('aria-describedby'),
    )
    const recoverableError = vi.fn()
    let root: ReturnType<typeof hydrateRoot> | undefined
    try {
      await act(async () => {
        root = hydrateRoot(container, tree, { onRecoverableError: recoverableError })
      })
      expect(
        Array.from(container.querySelectorAll('fieldset'), (group) =>
          group.getAttribute('aria-describedby'),
        ),
      ).toEqual(ids)
      expect(recoverableError).not.toHaveBeenCalled()
      expect(screen.getByRole('group', { name: 'First' })).toHaveAccessibleDescription('First hint')
    } finally {
      await act(async () => root?.unmount())
      container.remove()
    }
  })

  it('exposes native attributes, ref, heading, name and composed descriptions', () => {
    const ref = createRef<HTMLFieldSetElement>()
    render(
      <>
        <p id="extra">Additional context</p>
        <FormGroup
          aria-describedby="extra extra"
          className="custom"
          disabled
          error="Choose something"
          form="owner"
          headingLevel={2}
          hint="Choose all that apply"
          identifier="question"
          legend="Contact"
          name="question"
          ref={ref}
        >
          <input aria-label="Unchanged child" />
        </FormGroup>
      </>,
    )
    const group = screen.getByRole('group', { name: 'Contact' })
    expect(group).toBe(ref.current)
    expect(group).toHaveClass('form-group', 'custom')
    expect(group).toHaveAttribute('name', 'question')
    expect(group).toHaveAttribute('form', 'owner')
    expect(group).toHaveAttribute('aria-invalid', 'true')
    expect(group).toHaveAttribute('aria-describedby', 'extra question-hint question-error')
    expect(group).toHaveAccessibleDescription(
      'Additional context Choose all that apply Error: Choose something',
    )
    expect(screen.getByRole('heading', { level: 2 })).toHaveTextContent('Contact')
    const child = screen.getByRole('textbox')
    expect(child).toBeDisabled()
    expect(child).not.toHaveAttribute('aria-describedby')
    expect(child).not.toHaveAttribute('error')
  })

  it('generates distinct IDs without requiring an identifier', () => {
    render(
      <>
        <FormGroup legend="First" hint="First hint" />
        <FormGroup legend="Second" hint="Second hint" />
      </>,
    )
    const first = screen.getByRole('group', { name: 'First' })
    const second = screen.getByRole('group', { name: 'Second' })
    expect(first).toHaveAccessibleDescription('First hint')
    expect(second).toHaveAccessibleDescription('Second hint')
    expect(first.getAttribute('aria-describedby')).not.toBe(second.getAttribute('aria-describedby'))
  })

  it('uses React IDs on the server and permits no legend or invalid heading level', () => {
    const html = renderToString(<FormGroup legend="Server question" hint="Server hint" />)
    expect(html).toContain('Server hint')
    expect(html).toMatch(/aria-describedby="[^"]+-hint"/)
    const { rerender } = render(<FormGroup legend="Question" headingLevel={7} />)
    expect(screen.queryByRole('heading')).not.toBeInTheDocument()
    expect(screen.getByRole('group')).not.toHaveAttribute('aria-describedby')
    rerender(
      <FormGroup>
        <span>Child only</span>
      </FormGroup>,
    )
    expect(screen.getByRole('group').querySelector('legend')).toBeNull()
  })
})
