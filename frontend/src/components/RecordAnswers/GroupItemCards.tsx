import type { RecordAnswersQuestionDto } from '@/client/generated'
import { SummaryCard } from '@/components/SummaryCard/SummaryCard'
import { SummaryList, SummaryListRow } from '@/components/SummaryList/SummaryList'
import { formatAnswer } from '@/lib/dynamic-form/answerDisplay'

import type { ComponentProps, ReactNode } from 'react'

type GroupItemCardsProps = {
  /** A group question, whose `items` are shown one card each. */
  question: RecordAnswersQuestionDto
  /** Each card's title-line action, such as an Edit link to that item. */
  itemAction?: (index: number, title: string) => ReactNode
  /**
   * Shown once, such as a problem tag; errors are tagged on the group. It goes above the items, or
   * on the title line of the empty card.
   */
  tag?: ReactNode
  /** With no items, show one "Not yet provided" card in their place. */
  showEmptyCard?: boolean
  titleHeadingLevel: 3 | 4 | 5 | 6
}

/** One card per item of a group question, such as each clinical trial. */
export function GroupItemCards({
  question,
  itemAction,
  tag,
  showEmptyCard = false,
  titleHeadingLevel,
}: GroupItemCardsProps) {
  const items = question.items ?? []
  const isEmptyCard = showEmptyCard && items.length === 0
  const renderCard = (index: number, rows: ComponentProps<typeof SummaryList>['children']) => {
    const title = `${question.itemLabel ?? 'Item'} ${index + 1}`
    return (
      // Items have no identity of their own; position is what the user edits.
      <SummaryCard
        actions={itemAction?.(index, title)}
        key={index}
        tag={isEmptyCard && tag}
        title={title}
        titleHeadingLevel={titleHeadingLevel}
      >
        <SummaryList variant="wide-value">{rows}</SummaryList>
      </SummaryCard>
    )
  }

  return (
    <>
      {tag && !isEmptyCard && <p>{tag}</p>}
      {isEmptyCard
        ? renderCard(0, <SummaryListRow label="Not yet provided" value="" />)
        : items.map((item, index) =>
            renderCard(
              index,
              item.questions.map((child) => (
                <SummaryListRow key={child.id} label={child.label} value={formatAnswer(child)} />
              )),
            ),
          )}
    </>
  )
}
