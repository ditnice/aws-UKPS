import { act, fireEvent, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { useState } from 'react'
import { describe, expect, it, vi } from 'vitest'

import { Autocomplete } from './Autocomplete'

import type { AutocompleteProps, GetSuggestions } from './Autocomplete'

const codes = ['ABC-123', 'ABC-456', 'XABC-9', 'DEF-1']

const filterCodes: GetSuggestions = (query) =>
  Promise.resolve(codes.filter((code) => code.toLowerCase().includes(query.toLowerCase())))

type HarnessProps = Partial<AutocompleteProps> & { onSubmit?: (value: string) => void }

function Harness({ onSubmit, getSuggestions = filterCodes, ...props }: HarnessProps) {
  const [value, setValue] = useState('')
  return (
    <form
      onSubmit={(event) => {
        event.preventDefault()
        onSubmit?.(value)
      }}
    >
      <Autocomplete
        debounceMs={0}
        getSuggestions={getSuggestions}
        hint="Enter the company code"
        label="Company code"
        name="companyCode"
        onChange={setValue}
        value={value}
        {...props}
      />
      <button type="submit">Save</button>
    </form>
  )
}

const combobox = () => screen.getByRole('combobox', { name: 'Company code' })

describe('Autocomplete', () => {
  it('renders a collapsed combobox described by its hint and usage instructions', () => {
    render(<Harness />)

    const input = combobox()
    expect(input).toHaveAttribute('aria-autocomplete', 'list')
    expect(input).toHaveAttribute('aria-expanded', 'false')
    expect(input).toHaveAttribute('aria-controls', 'companyCode-listbox')
    expect(input).toHaveAttribute('autocomplete', 'off')
    expect(input).not.toHaveAttribute('aria-activedescendant')
    expect(input).toHaveAccessibleDescription(
      expect.stringContaining('Enter the company code When suggestions are available'),
    )
    expect(screen.queryByRole('listbox')).not.toBeInTheDocument()
  })

  it('shows matching suggestions as the user types and announces how many', async () => {
    const user = userEvent.setup()
    render(<Harness />)

    await user.type(combobox(), 'abc')

    const options = await screen.findAllByRole('option')
    expect(options.map((option) => option.textContent)).toEqual(['ABC-123', 'ABC-456', 'XABC-9'])
    expect(combobox()).toHaveAttribute('aria-expanded', 'true')
    expect(screen.getByRole('listbox', { name: 'Company code suggestions' })).toBeVisible()
    expect(screen.getByRole('status')).toHaveTextContent('3 suggestions available.')
  })

  it('moves through suggestions with the arrow keys, returning to the input after the last', async () => {
    const user = userEvent.setup()
    render(<Harness />)

    await user.type(combobox(), 'def')
    await screen.findByRole('option', { name: 'DEF-1' })

    await user.keyboard('{ArrowDown}')
    expect(combobox()).toHaveAttribute('aria-activedescendant', 'companyCode-option-0')
    expect(screen.getByRole('option', { name: 'DEF-1' })).toHaveAttribute('aria-selected', 'true')

    await user.keyboard('{ArrowDown}')
    expect(combobox()).not.toHaveAttribute('aria-activedescendant')

    await user.keyboard('{ArrowUp}')
    expect(combobox()).toHaveAttribute('aria-activedescendant', 'companyCode-option-0')
  })

  it('selects the active suggestion with Enter without submitting the form', async () => {
    const onSubmit = vi.fn()
    const user = userEvent.setup()
    render(<Harness onSubmit={onSubmit} />)

    await user.type(combobox(), 'abc')
    await screen.findAllByRole('option')
    await user.keyboard('{ArrowDown}{ArrowDown}{Enter}')

    expect(combobox()).toHaveValue('ABC-456')
    expect(combobox()).toHaveFocus()
    expect(combobox()).toHaveAttribute('aria-expanded', 'false')
    expect(onSubmit).not.toHaveBeenCalled()
  })

  it('submits the typed text with Enter when no suggestion is active', async () => {
    const onSubmit = vi.fn()
    const user = userEvent.setup()
    render(<Harness onSubmit={onSubmit} />)

    await user.type(combobox(), 'abc')
    await screen.findAllByRole('option')
    await user.keyboard('{Enter}')

    expect(onSubmit).toHaveBeenCalledWith('abc')
  })

  it('drops old suggestions and the active selection as soon as the query changes', async () => {
    const onSubmit = vi.fn()
    const getSuggestions: GetSuggestions = (query, signal) =>
      query.startsWith('abc-') ? new Promise(() => undefined) : filterCodes(query, signal)
    const user = userEvent.setup()
    render(<Harness getSuggestions={getSuggestions} onSubmit={onSubmit} />)

    await user.type(combobox(), 'abc')
    await screen.findAllByRole('option')
    await user.keyboard('{ArrowDown}')
    expect(combobox()).toHaveAttribute('aria-activedescendant', 'companyCode-option-0')

    await user.type(combobox(), '-9')

    expect(screen.queryByRole('listbox')).not.toBeInTheDocument()
    expect(combobox()).not.toHaveAttribute('aria-activedescendant')
    expect(screen.getByRole('status')).toBeEmptyDOMElement()
    await user.keyboard('{ArrowDown}{Enter}')

    expect(combobox()).toHaveValue('abc-9')
    expect(onSubmit).toHaveBeenCalledWith('abc-9')
  })

  it('keeps free text that matches no suggestion', async () => {
    const user = userEvent.setup()
    render(<Harness />)

    await user.type(combobox(), 'NEW-CODE')

    await waitFor(() =>
      expect(screen.getByRole('status')).toHaveTextContent('No suggestions available.'),
    )
    expect(screen.queryByRole('listbox')).not.toBeInTheDocument()
    expect(combobox()).toHaveValue('NEW-CODE')
  })

  it('closes the suggestions with Escape, keeping the typed text', async () => {
    const user = userEvent.setup()
    render(<Harness />)

    await user.type(combobox(), 'abc')
    await screen.findAllByRole('option')
    await user.keyboard('{ArrowDown}{Escape}')

    expect(combobox()).toHaveAttribute('aria-expanded', 'false')
    expect(combobox()).not.toHaveAttribute('aria-activedescendant')
    expect(combobox()).toHaveValue('abc')

    await user.keyboard('{ArrowDown}')
    expect(combobox()).toHaveAttribute('aria-expanded', 'true')
  })

  it('cancels a running fetch with Escape before the list has opened', async () => {
    let resolve: (value: string[]) => void = () => undefined
    let signal: AbortSignal | undefined
    const getSuggestions = vi.fn<GetSuggestions>((_query, requestSignal) => {
      signal = requestSignal
      return new Promise((resolver) => (resolve = resolver))
    })
    const user = userEvent.setup()
    render(<Harness getSuggestions={getSuggestions} />)

    await user.type(combobox(), 'abc')
    await waitFor(() =>
      expect(getSuggestions).toHaveBeenLastCalledWith('abc', expect.any(AbortSignal)),
    )
    await user.keyboard('{Escape}')

    expect(signal?.aborted).toBe(true)
    await act(async () => resolve(['ABC-123']))

    expect(screen.queryByRole('listbox')).not.toBeInTheDocument()
    expect(combobox()).toHaveAttribute('aria-expanded', 'false')
    expect(combobox()).toHaveValue('abc')
  })

  it('cancels the debounce with Escape before a fetch starts', () => {
    vi.useFakeTimers()
    try {
      const getSuggestions = vi.fn(filterCodes)
      render(<Harness debounceMs={250} getSuggestions={getSuggestions} />)

      fireEvent.change(combobox(), { target: { value: 'abc' } })
      fireEvent.keyDown(combobox(), { key: 'Escape' })
      act(() => vi.advanceTimersByTime(250))

      expect(getSuggestions).not.toHaveBeenCalled()
      expect(combobox()).toHaveAttribute('aria-expanded', 'false')
      expect(combobox()).toHaveValue('abc')
    } finally {
      vi.useRealTimers()
    }
  })

  it('opens without choosing a suggestion with Alt+Down', async () => {
    const user = userEvent.setup()
    render(<Harness />)

    await user.type(combobox(), 'abc')
    await screen.findAllByRole('option')
    await user.keyboard('{Escape}{Alt>}{ArrowDown}{/Alt}')

    expect(combobox()).toHaveAttribute('aria-expanded', 'true')
    expect(combobox()).not.toHaveAttribute('aria-activedescendant')
  })

  it('closes without selecting when the user tabs away', async () => {
    const onBlur = vi.fn()
    const user = userEvent.setup()
    render(<Harness onBlur={onBlur} />)

    await user.type(combobox(), 'abc')
    await screen.findAllByRole('option')
    await user.keyboard('{ArrowDown}')
    await user.tab()

    expect(combobox()).toHaveValue('abc')
    expect(screen.queryByRole('listbox')).not.toBeInTheDocument()
    expect(onBlur).toHaveBeenCalled()
  })

  it('does not open the suggestions when a fetch finishes after the user has left the field', async () => {
    let resolve: (value: string[]) => void = () => undefined
    let signal: AbortSignal | undefined
    const getSuggestions = vi.fn<GetSuggestions>((_query, requestSignal) => {
      signal = requestSignal
      return new Promise((resolver) => (resolve = resolver))
    })
    const user = userEvent.setup()
    render(<Harness getSuggestions={getSuggestions} />)

    await user.type(combobox(), 'abc')
    await waitFor(() => expect(getSuggestions).toHaveBeenCalled())
    await user.tab()
    resolve(['ABC-123'])
    await Promise.resolve()

    expect(signal?.aborted).toBe(true)
    expect(screen.queryByRole('listbox')).not.toBeInTheDocument()
    expect(combobox()).toHaveAttribute('aria-expanded', 'false')
  })

  it('does not fetch when the user leaves the field before typing pauses', async () => {
    const getSuggestions = vi.fn(filterCodes)
    const user = userEvent.setup()
    render(<Harness debounceMs={50} getSuggestions={getSuggestions} />)

    await user.type(combobox(), 'abc')
    await user.tab()
    await new Promise((resolve) => setTimeout(resolve, 100))

    expect(getSuggestions).not.toHaveBeenCalled()
    expect(screen.queryByRole('listbox')).not.toBeInTheDocument()
  })

  it('does not offer the old suggestions again after one is chosen', async () => {
    const user = userEvent.setup()
    render(<Harness />)

    await user.type(combobox(), 'abc')
    await user.click(await screen.findByRole('option', { name: 'ABC-123' }))
    await user.keyboard('{ArrowDown}')

    expect(combobox()).toHaveAttribute('aria-expanded', 'false')
    expect(screen.queryByRole('listbox')).not.toBeInTheDocument()
  })

  it('changes the status text when two results in a row have the same count', async () => {
    const user = userEvent.setup()
    render(<Harness />)

    await user.type(combobox(), 'abc-1')
    await waitFor(() => expect(screen.getByRole('status')).toHaveTextContent('1 suggestion'))
    const first = screen.getByRole('status').textContent

    await user.type(combobox(), '2')
    await waitFor(() =>
      expect(screen.getByRole('status')).toHaveTextContent('1 suggestion available.'),
    )
    expect(screen.getByRole('status').textContent).not.toBe(first)
  })

  it('selects a suggestion with a click and keeps focus on the input', async () => {
    const user = userEvent.setup()
    render(<Harness />)

    await user.type(combobox(), 'abc')
    await user.click(await screen.findByRole('option', { name: 'XABC-9' }))

    expect(combobox()).toHaveValue('XABC-9')
    expect(combobox()).toHaveFocus()
    expect(screen.queryByRole('listbox')).not.toBeInTheDocument()
  })

  it('emphasises the part of each suggestion that matches the query', async () => {
    const user = userEvent.setup()
    render(<Harness />)

    await user.type(combobox(), 'abc')

    const option = await screen.findByRole('option', { name: 'XABC-9' })
    expect(option.querySelector('strong')).toHaveTextContent('ABC')
  })

  it('does not fetch until the query is long enough', async () => {
    const getSuggestions = vi.fn(filterCodes)
    const user = userEvent.setup()
    render(<Harness getSuggestions={getSuggestions} minQueryLength={2} />)

    await user.type(combobox(), ' a ')
    expect(getSuggestions).not.toHaveBeenCalled()

    await user.type(combobox(), 'b')
    await waitFor(() => expect(getSuggestions).toHaveBeenCalledWith('a b', expect.any(AbortSignal)))
  })

  it('fetches once typing pauses, aborting a request the user has typed past', async () => {
    let resolveFirst: (value: string[]) => void = () => undefined
    const signals: AbortSignal[] = []
    const getSuggestions = vi.fn<GetSuggestions>((query, signal) => {
      signals.push(signal)
      return query === 'a'
        ? new Promise((resolve) => (resolveFirst = resolve))
        : filterCodes(query, signal)
    })
    const user = userEvent.setup()
    render(<Harness getSuggestions={getSuggestions} />)

    await user.type(combobox(), 'a')
    await waitFor(() => expect(getSuggestions).toHaveBeenCalledTimes(1))
    await user.type(combobox(), 'bc-4')
    await screen.findByRole('option', { name: 'ABC-456' })

    expect(signals[0].aborted).toBe(true)
    resolveFirst(['STALE'])
    await Promise.resolve()
    expect(screen.queryByRole('option', { name: 'STALE' })).not.toBeInTheDocument()
  })

  it('stays a plain text input when suggestions cannot be fetched', async () => {
    const getSuggestions = vi.fn<GetSuggestions>(() => Promise.reject(new Error('Network')))
    const user = userEvent.setup()
    render(<Harness getSuggestions={getSuggestions} />)

    await user.type(combobox(), 'abc')

    await waitFor(() => expect(getSuggestions).toHaveBeenCalled())
    expect(screen.queryByRole('listbox')).not.toBeInTheDocument()
    expect(combobox()).toHaveValue('abc')
  })

  it('marks the input as invalid and associates its error message', () => {
    render(<Harness error errorMessage="Enter company code" />)

    expect(combobox()).toBeInvalid()
    expect(combobox()).toHaveAccessibleDescription(
      expect.stringContaining('Error: Enter company code'),
    )
  })

  it('applies the width to the field and its suggestions together', () => {
    render(<Harness width="two-thirds" />)

    expect(combobox().parentElement?.style.width).toBe('66.6667%')
  })
})
