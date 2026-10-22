import type { FormControl } from '@/client/generated'
import type { FormValue, HierarchicalEntry } from '@/lib/dynamic-form/types'

import { CheckboxQuestion } from './questions/CheckboxQuestion'
import { HierarchicalSelectQuestion } from './questions/HierarchicalSelectQuestion'
import { RadioQuestion } from './questions/RadioQuestion'
import { SelectQuestion } from './questions/SelectQuestion'
import { TextareaQuestion } from './questions/TextareaQuestion'
import { TextQuestion } from './questions/TextQuestion'

import type { QuestionProps } from './types'
import type { ComponentType, ReactElement } from 'react'

/**
 * Narrows the form value to the shape a control holds. `toFormValue` in
 * lib/dynamic-form/answers.ts guarantees each control receives its own shape.
 */
const control = <TValue extends FormValue>(Component: ComponentType<QuestionProps<TValue>>) => {
  const Control = (props: QuestionProps): ReactElement => (
    <Component {...(props as unknown as QuestionProps<TValue>)} />
  )
  Control.displayName = `Control(${Component.displayName ?? Component.name})`
  return Control
}

/** The component that renders each control. Add new controls here. */
export const questionRegistry: Record<FormControl, (props: QuestionProps) => ReactElement> = {
  Text: control<string>(TextQuestion),
  Textarea: control<string>(TextareaQuestion),
  Select: control<string>(SelectQuestion),
  Radio: control<string>(RadioQuestion),
  Checkbox: control<string[]>(CheckboxQuestion),
  HierarchicalSelect: control<HierarchicalEntry | HierarchicalEntry[]>(HierarchicalSelectQuestion),
}
