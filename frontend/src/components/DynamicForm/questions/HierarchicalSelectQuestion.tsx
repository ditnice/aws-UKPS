import { RuleKind } from '@/client/generated'
import { Button } from '@/components/Button/Button'
import { Select, SelectOption } from '@/components/Select/Select'
import type { HierarchicalEntry, Question } from '@/lib/dynamic-form/types'

import styles from '../DynamicForm.module.scss'

import type { QuestionProps } from '../types'

const emptyEntry: HierarchicalEntry = { parent: '', child: '' }

type EntryProps = {
  question: Question
  name: string
  index: number
  entry: HierarchicalEntry
  onChange: (entry: HierarchicalEntry) => void
  onBlur: () => void
  hasError: boolean
  disabled: boolean
  labelAsHeading: boolean
}

/** One parent dropdown and, once a parent with children is chosen, its child dropdown. */
function HierarchicalEntrySelects({
  question,
  name,
  index,
  entry,
  onChange,
  onBlur,
  hasError,
  disabled,
  labelAsHeading,
}: EntryProps) {
  const parentId = `${name}-${index}-parent`
  const childId = `${name}-${index}-child`
  const children = question.options?.find((o) => o.value === entry.parent)?.children ?? []
  // Repeated entries need distinct accessible names.
  const entrySuffix = question.multiple && index > 0 ? ` (${index + 1})` : ''
  const showParentLabel = !labelAsHeading || entrySuffix !== ''

  return (
    <div className={styles.entry}>
      <label className={showParentLabel ? styles.childLabel : 'visually-hidden'} htmlFor={parentId}>
        {question.label}
        {entrySuffix}
      </label>
      <Select
        disabled={disabled}
        id={parentId}
        label={null}
        name={`${name}[${index}].parent`}
        onBlur={onBlur}
        onChange={(event) => onChange({ parent: event.target.value, child: '' })}
        aria-invalid={hasError || undefined}
        value={entry.parent}
        width="one-third"
      >
        <SelectOption value="">{question.placeholder ?? 'Select an option'}</SelectOption>
        {question.options?.map((option) => (
          <SelectOption key={option.value} value={option.value}>
            {option.label}
          </SelectOption>
        ))}
      </Select>

      {children.length > 0 && (
        <>
          <label className={styles.childLabel} htmlFor={childId}>
            {question.childLabel}
            {entrySuffix && <span className="visually-hidden">{entrySuffix}</span>}
          </label>
          <Select
            disabled={disabled}
            id={childId}
            label={null}
            name={`${name}[${index}].child`}
            onBlur={onBlur}
            onChange={(event) => onChange({ ...entry, child: event.target.value })}
            value={entry.child}
            width="one-third"
          >
            <SelectOption value="">{question.childPlaceholder ?? 'Select an option'}</SelectOption>
            {children.map((option) => (
              <SelectOption key={option.value} value={option.value}>
                {option.label}
              </SelectOption>
            ))}
          </Select>
        </>
      )}
    </div>
  )
}

/**
 * A two-level choice: a parent dropdown, then an optional child dropdown. The answer is the most
 * specific option chosen. Multi-value questions repeat the pair, up to the MaxItems rule.
 */
export function HierarchicalSelectQuestion({
  question,
  name,
  value,
  onChange,
  onBlur,
  error,
  disabled,
  labelAsHeading,
}: QuestionProps<HierarchicalEntry | HierarchicalEntry[]>) {
  const entries = Array.isArray(value) ? value : [value]
  const maxItems =
    question.rules.find((rule) => rule.kind === RuleKind.MAX_ITEMS)?.value ?? Infinity

  const update = (next: HierarchicalEntry[]) => onChange(question.multiple ? next : next[0])
  const replace = (index: number, entry: HierarchicalEntry) =>
    update(entries.map((current, i) => (i === index ? entry : current)))

  return (
    <div>
      {question.hint && (
        <p className="input__hint" id={`${name}-hint`}>
          {question.hint}
        </p>
      )}
      {error && (
        <p className={styles.error} id={`${name}-error`} role="alert">
          <span className="visually-hidden">Error:</span> {error}
        </p>
      )}
      {entries.map((entry, index) => (
        // Entries have no stable identity of their own; position is what the user edits.

        <div key={index}>
          <HierarchicalEntrySelects
            disabled={disabled}
            entry={entry}
            hasError={Boolean(error)}
            index={index}
            labelAsHeading={labelAsHeading}
            name={name}
            onBlur={onBlur}
            onChange={(next) => replace(index, next)}
            question={question}
          />
          {question.multiple && index > 0 && !disabled && (
            <Button onClick={() => update(entries.filter((_, i) => i !== index))} variant="link">
              Remove
              <span className="visually-hidden"> entry {index + 1}</span>
            </Button>
          )}
        </div>
      ))}
      {question.multiple && !disabled && entries.length < maxItems && (
        <Button onClick={() => update([...entries, emptyEntry])} variant="primary">
          {question.addAnotherLabel ?? 'Add another'}
        </Button>
      )}
    </div>
  )
}
