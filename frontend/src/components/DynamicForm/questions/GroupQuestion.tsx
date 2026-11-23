import { RuleKind } from '@/client/generated'
import { AddAnotherGroup } from '@/components/AddAnother/AddAnotherGroup'
import { FormGroup } from '@/components/FormGroup/FormGroup'
import { emptyGroupItem } from '@/lib/dynamic-form/answers'
import { groupItemAnchor } from '@/lib/dynamic-form/links'
import type { FormValue, GroupItemValue } from '@/lib/dynamic-form/types'

import { questionRegistry } from '../questionRegistry'

import type { QuestionProps } from '../types'

/** The item errors that belong to one child of one item, with that prefix removed. */
const childErrors = (errorItems: Record<string, string> | undefined, prefix: string) => {
  const entries = Object.entries(errorItems ?? {}).filter(([key]) => key.startsWith(prefix))
  return entries.length > 0
    ? Object.fromEntries(entries.map(([key, message]) => [key.slice(prefix.length), message]))
    : undefined
}

/**
 * A repeating group of questions in the MoJ add another stacked layout, such as clinical trials.
 * The whole group is a fieldset whose legend is the question label, with the hint and any error
 * about the group as a whole linked to it. Each item's questions are rendered by their own
 * controls, with errors from `errorItems`.
 */
export function GroupQuestion({
  question,
  name,
  value,
  onChange,
  onBlur,
  error,
  errorItems,
  clearError,
  labelStyle,
}: QuestionProps<GroupItemValue[]>) {
  const labelAsHeading = labelStyle === 'heading'
  const items = value.length > 0 ? value : [emptyGroupItem(question)]
  const children = question.children ?? []
  const maxItems = question.rules.find((rule) => rule.kind === RuleKind.MAX_ITEMS)?.value

  const update = (index: number, childId: string, next: FormValue) =>
    onChange(
      items.map((item, i) =>
        i === index ? { ...item, [childId]: next as string | string[] } : item,
      ),
    )

  return (
    // On a single-question page the label is the page heading, so the legend is hidden visually
    // (see DynamicForm.module.scss) and is not a heading itself; the items are then h2s. Otherwise
    // the legend is an h2 of its own, so it keeps its heading style rather than taking lead text.
    <FormGroup
      error={error || undefined}
      headingLevel={labelAsHeading ? undefined : 2}
      hint={question.hint ?? undefined}
      identifier={name}
      legend={question.label}
    >
      <AddAnotherGroup
        addItemLabel={question.addAnotherLabel ?? 'Add another'}
        count={items.length}
        itemHeading={(index) => `${question.itemLabel ?? 'Item'} ${index + 1}`}
        itemHeadingLevel={labelAsHeading ? 2 : 3}
        itemId={(index) => groupItemAnchor(question.id, index)}
        maxItems={maxItems ?? undefined}
        onAdd={() => onChange([...items, emptyGroupItem(question)])}
        onRemove={(index) => {
          onChange(items.filter((_, i) => i !== index))
          // Positions have moved, so errors shown against them no longer line up.
          clearError()
        }}
        removeItemLabel={question.removeLabel ?? 'Remove'}
        renderItem={(index) =>
          children.map((child) => {
            const Component = questionRegistry[child.type]
            const childName = `${name}-${index}-${child.id}`
            return (
              <div data-question-id={child.id} key={child.id}>
                <Component
                  clearError={clearError}
                  error={errorItems?.[`${index}.${child.id}`]}
                  errorItems={childErrors(errorItems, `${index}.${child.id}.`)}
                  labelStyle="default"
                  name={childName}
                  onBlur={onBlur}
                  onChange={(next: FormValue) => update(index, child.id, next)}
                  question={child}
                  value={items[index][child.id]}
                />
              </div>
            )
          })
        }
      />
    </FormGroup>
  )
}
