import { ChangeEvent } from 'react'

import { FormGroup } from '@nice-digital/nds-form-group'

import { Button } from '@/components/Button/Button'
import { Input, InputWidth } from '@/components/Input/Input'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { getFieldErrorMessage } from '@/lib/form/getFieldErrorMessage'

import styles from './ArrayInput.module.scss'

export type ArrayField<T extends string> = {
  name: T
  state: { value: string[]; meta: { errors: unknown[] } }
  removeValue: (i: number) => void
  pushValue: (newValue: string) => void
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
  labelPrefix: string
  hint: string
  field: ArrayField<T>
  width?: InputWidth | undefined
  getSubfield: (
    value: `${T}[${number}]`,
    renderer: (subfield: Field) => React.ReactElement,
  ) => React.ReactElement
}

/**
 * Renders an array-backed form field as a list of editable input items.
 *
 * Each value in the array is rendered as a subfield using `getSubfield`, allowing
 * the parent form library to wire up the correct field metadata and handlers.
 * The first item is treated as the primary input while additional items get a
 * remove button. Users can also append new empty values via the add button.
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
  labelPrefix,
  hint,
  width,
  field,
  getSubfield,
}: ArrayInputProps<T>) => {
  const errorMessage = getFieldErrorMessage(field.state.meta.errors)
  return (
    <FormGroup>
      <ErrorState>{errorMessage}</ErrorState>
      {field.state.value.map((_, i) => {
        const subfieldName = `${field.name}[${i}]` as const
        return (
          <div key={i}>
            {getSubfield(subfieldName, (subfield) => {
              const errorMessage = getFieldErrorMessage(subfield.state.meta.errors)
              return (
                <Input
                  error={Boolean(errorMessage)}
                  errorMessage={errorMessage}
                  label={`${labelPrefix} ${i > 0 ? i + 1 : ''}`}
                  className={styles.multiValuesInput}
                  name={subfield.name}
                  onBlur={subfield.handleBlur}
                  value={subfield.state.value}
                  hint={i == 0 ? hint : undefined}
                  onChange={(event: ChangeEvent<HTMLInputElement>) =>
                    subfield.handleChange(event.target.value)
                  }
                  width={width}
                />
              )
            })}
            {i > 0 ? (
              <Button
                data-testid={`remove-button-${i}`}
                type="button"
                variant="secondary"
                onClick={() => field.removeValue(i)}
              >
                {removeItemLabel}
              </Button>
            ) : (
              <></>
            )}
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
