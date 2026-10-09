import type { FormQuestionDto } from '@/client/generated'

import styles from '../DynamicForm.module.scss'

import { hintId } from './types'

/** Renders a question's hint; blank lines separate paragraphs. */
export function QuestionHint({ question }: { question: FormQuestionDto }) {
  if (!question.hint) {
    return null
  }

  return (
    <div id={hintId(question)}>
      {question.hint.split(/\n\s*\n/).map((paragraph) => (
        <p className={styles.hint} key={paragraph}>
          {paragraph}
        </p>
      ))}
    </div>
  )
}
