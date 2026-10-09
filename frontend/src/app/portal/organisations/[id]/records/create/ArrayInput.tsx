import { AnyFieldLikeMetaBase, Updater } from '@tanstack/react-form'
import { ChangeEvent } from 'react'

import { FormGroup } from '@nice-digital/nds-form-group'

import { Button } from '@/components/Button/Button'
import { Input, InputWidth } from '@/components/Input/Input'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { clearFormApiErrors } from '@/lib/form/formErrorHandling'
import { getFieldErrorMessage } from '@/lib/form/getFieldErrorMessage'

import styles from './ArrayInput.module.scss'

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

export type ArrayInputProps<T extends string> = {
  addItemLabel: string
  removeItemLabel: string
  allowRemoveFirstItem?: boolean
  labelPrefix: string
  hint: string
  field: ArrayField<T>
  width?: InputWidth | undefined
  getSubfield: (
    value: `${T}[${number}]`,
    renderer: (subfield: Field) => React.ReactElement,
  ) => React.ReactElement
}

const OptionalErrorState = ({ children }: React.PropsWithChildren) =>
  children ? <ErrorState>{children}</ErrorState> : null

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
 * @param props.labelPrefix - Prefix used for each item's visible label.
 * @param props.hint - Helper text shown beneath the first item.
 * @param props.width - Optional width for the rendered inputs.
 * @param props.field - Array field state and actions from the form library.
 * @param props.getSubfield - Callback used to render a specific field instance for one array index.
 * @returns The rendered array input component.
 */
const ArrayInput = <T extends string>({
  addItemLabel,
  removeItemLabel,
  allowRemoveFirstItem = false,
  labelPrefix,
  hint,
  width,
  field,
  getSubfield,
}: ArrayInputProps<T>) => {
  const errorMessage = getFieldErrorMessage(field.state.meta.errors)
  return (
    <FormGroup>
      {/* FormGroup clones each child, so empty slots must be elements rather than null */}
      {errorMessage ? <ErrorState>{errorMessage}</ErrorState> : <></>}
      {field.state.value.map((_, i) => {
        const subfieldName = `${field.name}[${i}]` as const
        const canRemoveItem = i > 0 || (allowRemoveFirstItem && field.state.value.length > 1)
        return (
          <div className={styles.item} key={i}>
            <div className={canRemoveItem ? styles.inputRowWithRemove : styles.inputRow}>
              {getSubfield(subfieldName, (subfield) => {
                const errorMessage = getFieldErrorMessage(subfield.state.meta.errors)
                return (
                  <Input
                    error={Boolean(errorMessage)}
                    errorMessage={errorMessage}
                    label={i > 0 ? `${labelPrefix} ${i + 1}` : labelPrefix}
                    className={styles.multiValuesInput}
                    name={subfield.name}
                    onBlur={subfield.handleBlur}
                    value={subfield.state.value}
                    hint={i === 0 ? hint : undefined}
                    onChange={(event: ChangeEvent<HTMLInputElement>) => {
                      subfield.handleChange(event.target.value)
                      field.setMeta(clearFormApiErrors())
                    }}
                    width={canRemoveItem ? undefined : width}
                  />
                )
              })}
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
          </div>
        )
      })}
      <Button
        data-testid="add-item-button"
        type="button"
        variant="secondary"
        onClick={() => field.pushValue('')}
      >
        {addItemLabel}
      </Button>
    </FormGroup>
  )
}

export default ArrayInput
