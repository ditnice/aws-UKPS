import { useCallback, useEffect, useRef, useState } from 'react'

/** Where focus goes once the list has re-rendered after an item was added or removed. */
type FocusRequest = { index: number; part: 'field' | 'heading' } | { index: null; part: 'heading' }

const focusable = 'input, select, textarea'

/**
 * Moves focus after an add another list changes, as the MoJ add another pattern does: to the first
 * field of a newly added item, and to a heading after an item is removed, so keyboard and screen
 * reader users are not left on a button that has gone.
 *
 * Attach `headingRef` to the list's heading (it needs `tabIndex={-1}`) and `itemRef(index)` to each
 * item's wrapper. An item's own heading is marked with `data-add-another-heading`.
 */
export function useAddAnotherFocus() {
  const headingRef = useRef<HTMLElement>(null)
  const items = useRef<(HTMLElement | null)[]>([])
  // The request waits in a ref until the list has re-rendered; the counter re-runs the effect.
  const pending = useRef<FocusRequest | null>(null)
  const [requests, setRequests] = useState(0)
  const setRequest = useCallback((request: FocusRequest) => {
    pending.current = request
    setRequests((count) => count + 1)
  }, [])

  useEffect(() => {
    const request = pending.current
    if (!request) {
      return
    }
    pending.current = null
    const item = request.index === null ? null : items.current[request.index]
    const target =
      request.index === null
        ? headingRef.current
        : request.part === 'heading'
          ? (item?.querySelector<HTMLElement>('[data-add-another-heading]') ??
            item?.querySelector<HTMLElement>(focusable))
          : item?.querySelector<HTMLElement>(focusable)
    target?.focus()
  }, [requests])

  const itemRef = useCallback(
    (index: number) => (element: HTMLElement | null) => {
      items.current[index] = element
    },
    [],
  )

  return {
    headingRef,
    itemRef,
    /** Focus the first field of the item at `index`, such as one just added. */
    focusField: (index: number) => setRequest({ index, part: 'field' }),
    /**
     * Focus a heading after a removal: the list's own heading when it has one, otherwise the
     * heading (or first field) of the item at `index`.
     */
    focusHeading: (index: number) =>
      setRequest(
        headingRef.current ? { index: null, part: 'heading' } : { index, part: 'heading' },
      ),
  }
}
