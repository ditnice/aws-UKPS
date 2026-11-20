import { useMemo } from 'react'

import { Autocomplete } from '@/components/Autocomplete/Autocomplete'
import { Input } from '@/components/Input/Input'
import { getSuggestionSource } from '@/lib/dynamic-form/suggestionSources'

import { useFormPageContext } from '../FormPageContext'
import { labelSizeFor } from '../labelStyle'

import { inputWidthOf } from './inputWidth'

import type { QuestionProps } from '../types'

export function TextQuestion({
  question,
  name,
  value,
  onChange,
  onBlur,
  error,
  labelStyle,
}: QuestionProps<string>) {
  const labelAsHeading = labelStyle === 'heading'
  const { organisationId } = useFormPageContext()
  // A text question whose display hint names an autocomplete source suggests values from it.
  const getSuggestions = useMemo(
    () =>
      organisationId === undefined
        ? undefined
        : getSuggestionSource(question.display, organisationId),
    [question.display, organisationId],
  )

  const fieldProps = {
    error: Boolean(error),
    errorMessage: error,
    hint: question.hint ?? undefined,
    label: labelAsHeading ? null : question.label,
    labelSize: labelSizeFor(labelStyle),
    name,
    onBlur,
    value,
    width: inputWidthOf(question.width) ?? 'two-thirds',
  } as const

  return (
    <>
      {labelAsHeading && (
        <label className="visually-hidden" htmlFor={name}>
          {question.label}
        </label>
      )}
      {getSuggestions ? (
        <Autocomplete {...fieldProps} getSuggestions={getSuggestions} onChange={onChange} />
      ) : (
        <Input {...fieldProps} onChange={(event) => onChange(event.target.value)} />
      )}
    </>
  )
}
