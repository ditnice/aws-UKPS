import { act, render, screen } from '@testing-library/react'
import { createRef } from 'react'
import { hydrateRoot } from 'react-dom/client'
import { renderToString } from 'react-dom/server'
import { describe, expect, it, vi } from 'vitest'

import { firstOptionValue, FormGroup } from './FormGroup'

import type { ReactNode } from 'react'

function Option(_: { value: string }) {
  return null
}

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
    expect(group).not.toHaveAttribute('aria-invalid')
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

  it('uses React IDs on the server and renders a plain legend without a heading level or no legend at all', () => {
    const html = renderToString(<FormGroup legend="Server question" hint="Server hint" />)
    expect(html).toContain('Server hint')
    expect(html).toMatch(/aria-describedby="[^"]+-hint"/)
    const { rerender } = render(<FormGroup legend="Question" />)
    expect(screen.queryByRole('heading')).not.toBeInTheDocument()
    expect(screen.getByRole('group')).not.toHaveAttribute('aria-describedby')
    rerender(
      <FormGroup>
        <span>Child only</span>
      </FormGroup>,
    )
    expect(screen.getByRole('group').querySelector('legend')).toBeNull()
  })

  it('renders the hint before the error message, following the NDS and GOV.UK patterns', () => {
    const { container } = render(
      <FormGroup error="Choose something" hint="Choose all that apply" legend="Contact" />,
    )
    const hint = container.querySelector('.form-group__hint')
    const error = container.querySelector('.form-group__error-message')
    expect(hint).not.toBeNull()
    expect(error).not.toBeNull()
    expect(hint!.compareDocumentPosition(error!) & Node.DOCUMENT_POSITION_FOLLOWING).toBeTruthy()
  })

  describe('firstOptionValue', () => {
    const cases: [string, ReactNode, string | undefined][] = [
      ['direct children', [<Option key="a" value="a" />, <Option key="b" value="b" />], 'a'],
      [
        'skipping other elements and text',
        ['Or', <p key="p">Divider</p>, <Option key="b" value="b" />],
        'b',
      ],
      [
        'inside Fragments',
        <>
          <></>
          <>
            <Option value="nested" />
          </>
          <Option value="after" />
        </>,
        'nested',
      ],
      ['no options', <p key="p">Nothing</p>, undefined],
      ['no children', null, undefined],
    ]
    it.each(cases)('finds the first option among %s', (_, children, expected) => {
      expect(firstOptionValue(children, Option)).toBe(expected)
    })
  })
})
