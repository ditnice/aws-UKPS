import { RuleKind } from '@/client/generated'
import { AddAnotherGroup } from '@/components/AddAnother/AddAnotherGroup'
import { FormGroup } from '@/components/FormGroup/FormGroup'
import { Select, SelectOption } from '@/components/Select/Select'
import type { HierarchicalEntry, Question } from '@/lib/dynamic-form/types'

import styles from '../DynamicForm.module.scss'

import type { LabelStyle } from '../labelStyle'
import type { QuestionProps } from '../types'

const emptyEntry: HierarchicalEntry = { parent: '', child: '' }

const describedBy = (ids: (string | undefined)[]) => ids.filter(Boolean).join(' ') || undefined

type EntryProps = {
  question: Question
  name: string
  index: number
  entry: HierarchicalEntry
  onChange: (entry: HierarchicalEntry) => void
  onBlur: () => void
  error?: string
  /** The ID of the question's hint, which every select is described by. */
  hintId?: string
  labelStyle: LabelStyle
}

/** One parent dropdown and, once a parent with children is chosen, its child dropdown. */
function HierarchicalEntrySelects({
  question,
  name,
  index,
  entry,
  onChange,
  onBlur,
  error,
  hintId,
  labelStyle,
}: EntryProps) {
  const parentId = `${name}-${index}-parent`
  const childId = `${name}-${index}-child`
  const children = question.options?.find((o) => o.value === entry.parent)?.children ?? []
  // A multi-value question's entries sit in a group whose legend, hint and error name and describe
  // them, and each entry's heading names the entry, so the parent label is hidden.
  const inGroup = Boolean(question.multiple)
  const showParentLabel = !inGroup && labelStyle !== 'heading'
  // On a page of several questions each question's label is lead text, and the child's label
  // matches it rather than standing out above it.
  const labelClass = labelStyle === 'lead' ? styles.leadLabel : styles.childLabel
  // Otherwise the question's error is shown above the select; every select is styled as invalid
  // and described by that one message (the select adds it itself).
  // Every select is also described by the question's hint.
  const ownsError = Boolean(error) && !inGroup
  const errorId = error && !inGroup ? `${name}-0-parent-error` : undefined

  return (
    <div className={styles.entry}>
      <label className={showParentLabel ? labelClass : 'visually-hidden'} htmlFor={parentId}>
        {question.label}
      </label>
      <Select
        aria-describedby={describedBy([hintId, ownsError ? undefined : errorId])}
        error={Boolean(error)}
        errorMessage={ownsError ? error : undefined}
        id={parentId}
        label={null}
        name={`${name}[${index}].parent`}
        onBlur={onBlur}
        onChange={(event) => {
          onChange({ parent: event.target.value, child: '' })
          // Revalidate straight away, so an error the new parent fixes clears before the child
          // select appears, rather than the child showing as invalid until the next blur.
          onBlur()
        }}
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
          <label className={labelClass} htmlFor={childId}>
            {question.childLabel}
          </label>
          <Select
            aria-describedby={describedBy([hintId, errorId])}
            error={Boolean(error)}
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
 * specific option chosen. Multi-value questions repeat the pair in the MoJ add another stacked
 * layout (as for clinical trials), up to the MaxItems rule.
 */
export function HierarchicalSelectQuestion({
  question,
  name,
  value,
  onChange,
  onBlur,
  error,
  clearError,
  labelStyle,
}: QuestionProps<HierarchicalEntry | HierarchicalEntry[]>) {
  const entries = Array.isArray(value) ? value : [value]
  const maxItems = question.rules.find((rule) => rule.kind === RuleKind.MAX_ITEMS)?.value

  const update = (next: HierarchicalEntry[]) => onChange(question.multiple ? next : next[0])
  const replace = (index: number, entry: HierarchicalEntry) =>
    update(entries.map((current, i) => (i === index ? entry : current)))
  const renderEntry = (index: number, hintId?: string) => (
    <HierarchicalEntrySelects
      entry={entries[index]}
      error={error}
      hintId={hintId}
      index={index}
      labelStyle={labelStyle}
      name={name}
      onBlur={onBlur}
      onChange={(next) => replace(index, next)}
      question={question}
    />
  )

  if (!question.multiple) {
    const hintId = question.hint ? `${name}-hint` : undefined
    return (
      <div>
        {question.hint && (
          <p className="input__hint" id={hintId}>
            {question.hint}
          </p>
        )}
        {renderEntry(0, hintId)}
      </div>
    )
  }

  // On a single-question page the label is the page heading, so the legend is hidden visually
  // (see DynamicForm.module.scss) and is not a heading itself; the items are then h2s. Otherwise
  // the legend is an h2 of its own and the items are h3s.
  const labelAsHeading = labelStyle === 'heading'

  return (
    <FormGroup
      error={error || undefined}
      headingLevel={labelAsHeading ? undefined : 2}
      hint={question.hint ?? undefined}
      identifier={name}
      legend={question.label}
    >
      <AddAnotherGroup
        addItemLabel={question.addAnotherLabel ?? 'Add another'}
        count={entries.length}
        itemHeading={(index) => `${question.itemLabel ?? 'Item'} ${index + 1}`}
        itemHeadingLevel={labelAsHeading ? 2 : 3}
        maxItems={maxItems ?? undefined}
        onAdd={() => update([...entries, emptyEntry])}
        onRemove={(index) => {
          update(entries.filter((_, i) => i !== index))
          // Positions have moved, so an error shown against them no longer lines up.
          clearError()
        }}
        removeItemLabel={question.removeLabel ?? 'Remove'}
        renderItem={(index) => renderEntry(index)}
      />
    </FormGroup>
  )
}
