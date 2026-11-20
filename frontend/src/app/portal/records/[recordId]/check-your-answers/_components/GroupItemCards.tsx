import type { RecordCheckAnswersQuestionDto } from '@/client/generated'
import { SummaryCard } from '@/components/SummaryCard/SummaryCard'
import {
  SummaryList,
  SummaryListAction,
  SummaryListRow,
} from '@/components/SummaryList/SummaryList'
import { Tag } from '@/components/Tag/Tag'
import { formatAnswer } from '@/lib/dynamic-form/answerDisplay'
import { checkAnswersPageHref, groupItemAnchor } from '@/lib/dynamic-form/links'

type GroupItemCardsProps = {
  recordId: number
  /** A group question, whose `items` are shown one card each. */
  question: RecordCheckAnswersQuestionDto
  /**
   * Set when the group has a submission error, which is tagged on the group rather than an item.
   * With no items there is nothing to tag: the error is in the summary at the top of the page.
   */
  hasError?: boolean
}

/** One card per item of a group question, such as each clinical trial, with an Edit link to that item. */
export function GroupItemCards({ recordId, question, hasError = false }: GroupItemCardsProps) {
  return (
    <>
      {hasError && (question.items?.length ?? 0) > 0 && (
        <p>
          <Tag colour="red">
            There is a problem<span className="visually-hidden"> with {question.label}</span>
          </Tag>
        </p>
      )}
      {(question.items ?? []).map((item, index) => {
        const title = `${question.itemLabel ?? 'Item'} ${index + 1}`
        return (
          // Items have no identity of their own; position is what the user edits.
          <SummaryCard
            actions={
              <SummaryListAction
                href={checkAnswersPageHref(recordId, question.pageId, {
                  anchor: groupItemAnchor(question.id, index),
                })}
                visuallyHiddenText={title}
              >
                Edit
              </SummaryListAction>
            }
            key={index}
            title={title}
            titleHeadingLevel={3}
          >
            <SummaryList variant="wide-value">
              {item.questions.map((child) => (
                <SummaryListRow key={child.id} label={child.label} value={formatAnswer(child)} />
              ))}
            </SummaryList>
          </SummaryCard>
        )
      })}
    </>
  )
}
