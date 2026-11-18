import type { RecordCheckAnswersQuestionDto } from '@/client/generated'
import { SummaryCard } from '@/components/SummaryCard/SummaryCard'
import {
  SummaryList,
  SummaryListAction,
  SummaryListRow,
} from '@/components/SummaryList/SummaryList'
import { formatAnswer } from '@/lib/dynamic-form/answerDisplay'
import { checkAnswersReturnTo, groupItemAnchor, pageHref } from '@/lib/dynamic-form/links'

type GroupItemCardsProps = {
  recordId: number
  /** A group question, whose `items` are shown one card each. */
  question: RecordCheckAnswersQuestionDto
}

/** One card per item of a group question, such as each clinical trial, with an Edit link to that item. */
export function GroupItemCards({ recordId, question }: GroupItemCardsProps) {
  const pageLink = `${pageHref({ recordId }, question.pageId)}?returnTo=${checkAnswersReturnTo}`

  return (question.items ?? []).map((item, index) => {
    const title = `${question.itemLabel ?? 'Item'} ${index + 1}`
    return (
      // Items have no identity of their own; position is what the user edits.
      <SummaryCard
        actions={
          <SummaryListAction
            href={`${pageLink}#${groupItemAnchor(question.id, index)}`}
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
  })
}
