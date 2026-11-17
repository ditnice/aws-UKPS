import { Button } from '@/components/Button/Button'
import { ErrorState } from '@/components/Placeholder/ErrorState'

import styles from './AddAnotherGroup.module.scss'
import { useAddAnotherFocus } from './useAddAnotherFocus'

import type { ReactNode } from 'react'

export type AddAnotherGroupProps = {
  /** How many items there are; always at least one is shown by the caller. */
  count: number
  /** The heading of the item at `index`, such as "Clinical trial 1". */
  itemHeading: (index: number) => string
  /** The fields of the item at `index`. */
  renderItem: (index: number) => ReactNode
  addItemLabel: string
  removeItemLabel: string
  onAdd: () => void
  onRemove: (index: number) => void
  /** An error about the group as a whole, such as having no items. */
  error?: string
  /** No more items may be added once there are this many. */
  maxItems?: number
}

/**
 * A repeating group of fields in the MoJ add another stacked layout: each item is a fieldset
 * headed by its number, with a remove button once there are several, followed by a divider, and an
 * add another button after the last item.
 *
 * Adding an item focuses its first field; removing one focuses the heading of the item before it
 * (or the new first item), so focus is never lost with the removed button.
 */
export function AddAnotherGroup({
  count,
  itemHeading,
  renderItem,
  addItemLabel,
  removeItemLabel,
  onAdd,
  onRemove,
  error,
  maxItems = Infinity,
}: AddAnotherGroupProps) {
  const { itemRef, focusField, focusHeading } = useAddAnotherFocus()
  const canRemove = count > 1

  return (
    <div className={styles.root}>
      {error && <ErrorState>{error}</ErrorState>}
      {Array.from({ length: count }, (_, index) => (
        // Items have no identity of their own; position is what the user edits.
        <fieldset className={styles.item} key={index} ref={itemRef(index)}>
          <legend className={styles.heading} data-add-another-heading tabIndex={-1}>
            <h2 className={styles.headingText}>{itemHeading(index)}</h2>
          </legend>
          {renderItem(index)}
          {canRemove && (
            <Button
              className={styles.removeButton}
              data-testid={`remove-item-${index}`}
              onClick={() => {
                onRemove(index)
                focusHeading(Math.max(index - 1, 0))
              }}
              type="button"
              variant="secondary"
            >
              {removeItemLabel} <span className="visually-hidden">{itemHeading(index)}</span>
            </Button>
          )}
        </fieldset>
      ))}
      {count < maxItems && (
        <Button
          data-testid="add-item"
          onClick={() => {
            onAdd()
            focusField(count)
          }}
          type="button"
          variant="primary"
        >
          {addItemLabel}
        </Button>
      )}
    </div>
  )
}
