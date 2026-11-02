import type {
  RecordPageOptionDto,
  RecordPageQuestionDto,
  RecordPageRuleDto,
} from '@/client/generated'

export type Question = RecordPageQuestionDto
export type Rule = RecordPageRuleDto
export type Option = RecordPageOptionDto

/** An answer on the wire: a string or null, or an array of strings for multi-value questions. */
export type Answer = string | null | string[]

/** One parent and child choice of a hierarchical select. */
export type HierarchicalEntry = { parent: string; child: string }

/**
 * A question's value while the form is being edited:
 * - text, select and radio questions hold a string ('' when unanswered);
 * - checkbox questions hold the ticked values;
 * - hierarchical selects hold a parent and child pair, or a list of pairs when multi-value.
 */
export type FormValue = string | string[] | HierarchicalEntry | HierarchicalEntry[]

export type FormValues = Record<string, FormValue>
