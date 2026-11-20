import { AddAnother } from '@/components/AddAnother/AddAnother'

import { labelSizeFor } from '../labelStyle'

import type { QuestionProps } from '../types'

/**
 * Text inputs in the MoJ add another inline layout: one per entry, each with a remove button once
 * there are several, and an add another button. The question label is the list's legend. When the
 * question has an item label, each entry shows it as its own label ("Other registry number" while there is
 * one entry, "Other registry number 1", "Other registry number 2" once there are several); otherwise entries are
 * labelled with the question label for screen readers only.
 */
export function TextListQuestion({
  question,
  name,
  value,
  onChange,
  onBlur,
  error,
  errorItems,
  clearError,
  labelStyle,
}: QuestionProps<string[]>) {
  const entries = value.length > 0 ? value : ['']
  const indexOf = (subfieldName: string) => Number(/\[(\d+)\]$/.exec(subfieldName)?.[1] ?? 0)

  return (
    <AddAnother
      addItemLabel={question.addAnotherLabel ?? 'Add another'}
      field={{
        name,
        state: { value: entries, meta: { errors: error ? [error] : [] } },
        pushValue: (next) => onChange([...entries, next]),
        removeValue: (index) => {
          onChange(entries.filter((_, i) => i !== index))
          // Positions have moved, so errors shown against them no longer line up.
          clearError()
        },
        // Typing in an entry clears the errors shown from the last save until it is next checked.
        setMeta: () => clearError(),
      }}
      getSubfield={(subfieldName, render) => {
        const index = indexOf(subfieldName)
        const entryError = errorItems?.[String(index)]
        return render({
          name: subfieldName,
          handleBlur: onBlur,
          handleChange: (next) => onChange(entries.map((entry, i) => (i === index ? next : entry))),
          state: { value: entries[index] ?? '', meta: { errors: entryError ? [entryError] : [] } },
        })
      }}
      hint={question.hint ?? undefined}
      itemLabel={question.itemLabel ?? undefined}
      labelPrefix={question.label}
      legend={question.label}
      legendSize={labelSizeFor(labelStyle)}
      removeItemLabel={question.removeLabel ?? 'Remove'}
    />
  )
}
