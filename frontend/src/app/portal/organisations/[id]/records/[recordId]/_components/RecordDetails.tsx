import type { RecordAnswersSectionDto } from '@/client/generated'
import { TaskAnswers } from '@/components/RecordAnswers/TaskAnswers'
import { SummaryCard } from '@/components/SummaryCard/SummaryCard'

import { Accordion, AccordionGroup } from './RecordAccordion'

type RecordDetailsProps = {
  sections: readonly RecordAnswersSectionDto[]
}

/**
 * Renders the questions that apply to a record with their answers, laid out as check your answers
 * lays them out but read only: an accordion per section and a card per task.
 */
export const RecordDetails = ({ sections }: RecordDetailsProps) => (
  <AccordionGroup>
    {sections.map((section) => (
      <Accordion key={section.id} title={section.title} displayTitleAsHeading headingLevel={3}>
        {section.tasks.map((task) => (
          <SummaryCard key={task.id} title={task.title} titleHeadingLevel={4}>
            <TaskAnswers questions={task.questions} groupItemHeadingLevel={5} />
          </SummaryCard>
        ))}
      </Accordion>
    ))}
  </AccordionGroup>
)
