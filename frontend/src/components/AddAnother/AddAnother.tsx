import { AnyFieldLikeMetaBase, Updater } from '@tanstack/react-form'
import { ChangeEvent } from 'react'

import { Button } from '@/components/Button/Button'
import { Input } from '@/components/Input/Input'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { clearFormApiErrors } from '@/lib/form/formErrorHandling'
import { getFieldErrorMessage } from '@/lib/form/getFieldErrorMessage'

import styles from './AddAnother.module.scss'

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
  heading?: string
  firstItemClassName?: string
  labelPrefix: string
  hint: string
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
 * @template T - The field name key used for the array entries.
 * @param props.addItemLabel - Label shown on the button used to append a new item.
 * @param props.removeItemLabel - Label shown on the button used to remove a list item.
 * @param props.heading - Optional h2 shown once above the list.
 * @param props.firstItemClassName - Optional class applied to the first item's row only.
 * @param props.labelPrefix - Prefix used for each item's visible label.
 * @param props.hint - Helper text shown beneath the first item.
 * @param props.field - Array field state and actions from the form library.
 * @param props.getSubfield - Callback used to render a specific field instance for one array index.
 * @returns The rendered array input component.
 */
export const AddAnother = <T extends string>({
  addItemLabel,
  removeItemLabel,
  heading,
  firstItemClassName,
  labelPrefix,
  hint,
  field,
  getSubfield,
}: AddAnotherProps<T>) => {
  const errorMessage = getFieldErrorMessage(field.state.meta.errors)
  const canRemoveItem = field.state.value.length > 1
  return (
    <div className={styles.root}>
      {heading && <h2 className={styles.heading}>{heading}</h2>}
      {errorMessage && <ErrorState>{errorMessage}</ErrorState>}
      {field.state.value.map((_, i) => (
        <div
          className={
            i === 0 && firstItemClassName ? `${styles.row} ${firstItemClassName}` : styles.row
          }
          key={i}
        >
          <div className={styles.input}>
            {getSubfield(`${field.name}[${i}]`, (subfield) => {
              const errorMessage = getFieldErrorMessage(subfield.state.meta.errors)
              return (
                <Input
                  error={Boolean(errorMessage)}
                  errorMessage={errorMessage}
                  label={i > 0 ? `${labelPrefix} ${i + 1}` : labelPrefix}
                  name={subfield.name}
                  onBlur={subfield.handleBlur}
                  value={subfield.state.value}
                  hint={i === 0 ? hint : undefined}
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
              onClick={() => field.removeValue(i)}
            >
              {removeItemLabel} <span className="visually-hidden">{i + 1}</span>
            </Button>
          )}
        </div>
      ))}
      <Button
        data-testid="add-item-button"
        type="button"
        variant="primary"
        onClick={() => field.pushValue('')}
      >
        {addItemLabel}
      </Button>
    </div>
  )
}
