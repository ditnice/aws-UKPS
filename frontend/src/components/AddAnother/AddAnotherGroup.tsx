import { Button } from '@/components/Button/Button'

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
  /** The heading level of each item's heading, below the heading the group sits under. */
  itemHeadingLevel?: 2 | 3 | 4
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
  itemHeadingLevel = 2,
  maxItems = Infinity,
}: AddAnotherGroupProps) {
  const { itemRef, focusField, focusHeading } = useAddAnotherFocus()
  const canRemove = count > 1
  const ItemHeading = `h${itemHeadingLevel}` as const

  return (
    <div className={styles.root}>
      {Array.from({ length: count }, (_, index) => (
        // Items have no identity of their own; position is what the user edits.
        <fieldset className={styles.item} key={index} ref={itemRef(index)}>
          <legend className={styles.heading} data-add-another-heading tabIndex={-1}>
            <ItemHeading className={styles.headingText}>{itemHeading(index)}</ItemHeading>
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
