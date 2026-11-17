import { AddAnother } from '@/components/AddAnother/AddAnother'

import type { QuestionProps } from '../types'

/**
 * Text inputs in the MoJ add another inline layout: one per entry, each with a remove button once
 * there are several, and an add another button. The question label is the list's legend.
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
        removeValue: (index) => onChange(entries.filter((_, i) => i !== index)),
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
      labelPrefix={question.label}
      legend={question.label}
      removeItemLabel={question.removeLabel ?? 'Remove'}
    />
  )
}
