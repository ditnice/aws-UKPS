import type { RecordAnswersQuestionDto } from '@/client/generated'
import { SummaryCard } from '@/components/SummaryCard/SummaryCard'
import { SummaryList, SummaryListRow } from '@/components/SummaryList/SummaryList'
import { formatAnswer } from '@/lib/dynamic-form/answerDisplay'

import type { ReactNode } from 'react'

type GroupItemCardsProps = {
  /** A group question, whose `items` are shown one card each. */
  question: RecordAnswersQuestionDto
  /** Each card's title-line action, such as an Edit link to that item. */
  itemAction?: (index: number, title: string) => ReactNode
  /** Shown once above the items, such as a problem tag; errors are tagged on the group. */
  tag?: ReactNode
  titleHeadingLevel: 3 | 4 | 5 | 6
}

/** One card per item of a group question, such as each clinical trial. */
export function GroupItemCards({
  question,
  itemAction,
  tag,
  titleHeadingLevel,
}: GroupItemCardsProps) {
  const cards = (question.items ?? []).map((item, index) => {
    const title = `${question.itemLabel ?? 'Item'} ${index + 1}`
    return (
      // Items have no identity of their own; position is what the user edits.
      <SummaryCard
        actions={itemAction?.(index, title)}
        key={index}
        title={title}
        titleHeadingLevel={titleHeadingLevel}
      >
        <SummaryList variant="wide-value">
          {item.questions.map((child) => (
            <SummaryListRow key={child.id} label={child.label} value={formatAnswer(child)} />
          ))}
        </SummaryList>
      </SummaryCard>
    )
  })

  return (
    <>
      {tag && <p>{tag}</p>}
      {cards}
    </>
  )
}
