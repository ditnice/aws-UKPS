import { AnyFieldLikeMetaBase, Updater } from '@tanstack/react-form'
import clsx from 'clsx'
import { ChangeEvent, CSSProperties } from 'react'

import { Button } from '@/components/Button/Button'
import { Input, inputWidthStyles } from '@/components/Input/Input'
import type { InputFluidWidth, LabelSize } from '@/components/Input/Input'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { clearFormApiErrors } from '@/lib/form/formErrorHandling'
import { getFieldErrorMessage } from '@/lib/form/getFieldErrorMessage'

import styles from './AddAnother.module.scss'
import { useAddAnotherFocus } from './useAddAnotherFocus'

export type ArrayField<T extends string> = {
  name: T
  state: { value: string[]; meta: { errors: unknown[] } }
  removeValue: (i: number) => void
  pushValue: (newValue: string) => void
  setMeta: (updater: Updater<AnyFieldLikeMetaBase>) => void
}
export type Field = {
  name: string
  handleBlur: () => void
  state: { value: string; meta: { errors: unknown[] } }
  handleChange: (value: string) => void
}

export type GetSubfield<T extends string> = (
  name: `${T}[${number}]`,
  renderer: (subfield: Field) => React.ReactElement,
) => React.ReactElement

export type AddAnotherProps<T extends string> = {
  addItemLabel: string
  removeItemLabel: string
  title?: string
  /**
   * When set, the list is a fieldset with this legend as its visible label, and each input's own
   * label is visually hidden. Use it where the list answers one question, instead of `title`.
   */
  legend?: string
  /** The size of the legend; the items' own labels keep the standard size. */
  legendSize?: LabelSize
  /**
   * A label of each item's own, for example "Trial number", which is shown even when `legend` is
   * set and is used in place of `labelPrefix`. Blank values are ignored.
   */
  itemLabel?: string
  firstItemClassName?: string
  /** Optional share of the row each input's box takes; defaults to two-thirds. */
  inputWidth?: InputFluidWidth
  labelPrefix: string
  hint?: string
  field: ArrayField<T>
  getSubfield: GetSubfield<T>
}

/**
 * Renders an array-backed form field as a list of editable input items.
 *
 * Each value in the array is rendered as a subfield using `getSubfield`, allowing
 * the parent form library to wire up the correct field metadata and handlers.
 * The first item is treated as the primary input while additional items get a
 * remove button. Users can also append new empty values via the add button.
 * Changing any item clears API errors on the array field, as these are attached
 * to the array rather than its items and would otherwise block resubmission.
 *
 * Focus follows the MoJ add another pattern: adding an item focuses its input, and removing one
 * focuses the list's heading (the title or legend), or the previous item when there is none.
 *
 * @template T - The field name key used for the array entries.
 * @param props.addItemLabel - Label shown on the button used to append a new item.
 * @param props.removeItemLabel - Label shown on the button used to remove a list item.
 * @param props.title - Optional h2 shown once above the list.
 * @param props.legend - Optional legend that labels the list as one question.
 * @param props.inputWidth - Optional width of each input's box, overriding the default two-thirds.
 * @param props.itemLabel - Optional visible label of each item, used instead of `labelPrefix`.
 * @param props.firstItemClassName - Optional class applied to the first item's row only.
 * @param props.labelPrefix - Prefix used for each item's visible label.
 * @param props.hint - Optional helper text shown above the first item.
 * @param props.field - Array field state and actions from the form library.
 * @param props.getSubfield - Callback used to render a specific field instance for one array index.
 * @returns The rendered array input component.
 */
export const AddAnother = <T extends string>({
  addItemLabel,
  removeItemLabel,
  title,
  inputWidth,
  legend,
  legendSize = 'default',
  itemLabel,
  firstItemClassName,
  labelPrefix,
  hint,
  field,
  getSubfield,
}: AddAnotherProps<T>) => {
  const errorMessage = getFieldErrorMessage(field.state.meta.errors)
  const canRemoveItem = field.state.value.length > 1
  const hintId = hint ? `${field.name}-hint` : undefined
  const { headingRef, itemRef, focusField, focusHeading } = useAddAnotherFocus()
  const visibleItemLabel = itemLabel?.trim() || undefined
  const prefix = visibleItemLabel ?? labelPrefix
  // Singular item keeps plain label; several are numbered from 1.
  const getItemLabel = (index: number) => (canRemoveItem ? `${prefix} ${index + 1}` : prefix)
  const Root = legend ? 'fieldset' : 'div'
  return (
    <Root className={clsx(styles.root, legend && styles.fieldset)}>
      {legend && (
        <legend
          className={clsx(
            'input__label',
            styles.legend,
            legendSize === 'lead' && styles.leadLegend,
          )}
          ref={headingRef}
          tabIndex={-1}
        >
          {legend}
        </legend>
      )}
      {title && (
        <h2 ref={headingRef} tabIndex={-1}>
          {title}
        </h2>
      )}
      {hint && (
        <p className="input__hint" id={hintId}>
          {hint}
        </p>
      )}
      {errorMessage && <ErrorState>{errorMessage}</ErrorState>}
      {field.state.value.map((_, i) => (
        <div
          className={
            i === 0 && firstItemClassName ? `${styles.row} ${firstItemClassName}` : styles.row
          }
          key={i}
          ref={itemRef(i)}
        >
          <div
            className={clsx(styles.input, legend && !visibleItemLabel && styles.hiddenLabel)}
            style={
              inputWidth
                ? ({ '--input-basis': inputWidthStyles[inputWidth].width } as CSSProperties)
                : undefined
            }
          >
            {getSubfield(`${field.name}[${i}]`, (subfield) => {
              const errorMessage = getFieldErrorMessage(subfield.state.meta.errors)
              return (
                <Input
                  error={Boolean(errorMessage)}
                  errorMessage={errorMessage}
                  label={getItemLabel(i)}
                  name={subfield.name}
                  onBlur={subfield.handleBlur}
                  value={subfield.state.value}
                  aria-describedby={i === 0 ? hintId : undefined}
                  onChange={(event: ChangeEvent<HTMLInputElement>) => {
                    subfield.handleChange(event.target.value)
                    field.setMeta(clearFormApiErrors())
                  }}
                  width="full"
                />
              )
            })}
          </div>
          {canRemoveItem && (
            <Button
              className={styles.removeButton}
              data-testid={`remove-button-${i}`}
              type="button"
              variant="secondary"
              onClick={() => {
                field.removeValue(i)
                focusHeading(Math.max(i - 1, 0))
              }}
            >
              {removeItemLabel} <span className="visually-hidden">{getItemLabel(i)}</span>
            </Button>
          )}
        </div>
      ))}
      <Button
        data-testid="add-item-button"
        type="button"
        variant="primary"
        onClick={() => {
          const index = field.state.value.length
          field.pushValue('')
          focusField(index)
        }}
      >
        {addItemLabel}
      </Button>
    </Root>
  )
}
