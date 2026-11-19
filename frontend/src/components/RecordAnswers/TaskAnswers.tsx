import type { RecordAnswersQuestionDto } from '@/client/generated'
import { SummaryList, SummaryListRow } from '@/components/SummaryList/SummaryList'
import type { SummaryListActionProps } from '@/components/SummaryList/SummaryList'
import { formatAnswer } from '@/lib/dynamic-form/answerDisplay'

import { GroupItemCards } from './GroupItemCards'

import type { ReactElement, ReactNode } from 'react'

type TaskBlock =
  | { kind: 'rows'; questions: RecordAnswersQuestionDto[] }
  | { kind: 'group'; question: RecordAnswersQuestionDto }

/** Splits a task's questions, in form order, into runs of plain rows and the groups between them. */
const splitTaskQuestions = (questions: readonly RecordAnswersQuestionDto[]): TaskBlock[] => {
  const blocks: TaskBlock[] = []
  for (const question of questions) {
    const last = blocks.at(-1)
    if (question.items) {
      blocks.push({ kind: 'group', question })
    } else if (last?.kind === 'rows') {
      last.questions.push(question)
    } else {
      blocks.push({ kind: 'rows', questions: [question] })
    }
  }
  return blocks
}

type TaskAnswersProps = {
  /** A task's questions that apply, in form order. */
  questions: readonly RecordAnswersQuestionDto[]
  /** Heading level of each group item's card, one below the task's. */
  groupItemHeadingLevel: 3 | 4 | 5 | 6
  /** A row's action, such as a Change link. */
  rowAction?: (question: RecordAnswersQuestionDto) => ReactElement<SummaryListActionProps>
  /** A row's tag, such as a problem tag. */
  rowTag?: (question: RecordAnswersQuestionDto) => ReactNode
  /** A group's tag, shown once above its items. */
  groupTag?: (question: RecordAnswersQuestionDto) => ReactNode
  /** A group item card's title-line action, such as an Edit link to that item. */
  groupItemAction?: (question: RecordAnswersQuestionDto, index: number, title: string) => ReactNode
}

/**
 * A task's answers in form order: a summary row per question, and a card per item of a group
 * such as clinical trials.
 */
export function TaskAnswers({
  questions,
  groupItemHeadingLevel,
  rowAction,
  rowTag,
  groupTag,
  groupItemAction,
}: TaskAnswersProps) {
  return splitTaskQuestions(questions).map((block, index) =>
    block.kind === 'group' ? (
      <GroupItemCards
        key={block.question.id}
        question={block.question}
        tag={groupTag?.(block.question)}
        itemAction={
          groupItemAction &&
          ((itemIndex, title) => groupItemAction(block.question, itemIndex, title))
        }
        titleHeadingLevel={groupItemHeadingLevel}
      />
    ) : (
      <SummaryList key={`rows-${index}`} variant="wide-value">
        {block.questions.map((question) => (
          <SummaryListRow
            key={question.id}
            label={question.label}
            value={formatAnswer(question)}
            tag={rowTag?.(question)}
          >
            {rowAction?.(question)}
          </SummaryListRow>
        ))}
      </SummaryList>
    ),
  )
}
