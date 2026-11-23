import type {
  RecordPageConditionDto,
  RecordPageOptionDto,
  RecordPageQuestionDto,
  RecordPageRuleDto,
} from '@/client/generated'

import type { DateValue } from './dates'

export type Question = RecordPageQuestionDto
export type Rule = RecordPageRuleDto
export type Option = RecordPageOptionDto
export type Condition = RecordPageConditionDto

/** An answer on the wire: a string or null, or an array of strings for multi-value questions. */
export type Answer = string | null | string[]

/** One item of a group's answer on the wire, keyed by child question ID. */
export type GroupItemAnswer = Record<string, Answer>

/** Any answer on the wire, including a group's array of items. */
export type PayloadAnswer = Answer | GroupItemAnswer[]

/** One item of a group while it is being edited, keyed by child question ID. */
export type GroupItemValue = Record<string, string | string[]>

/** One parent and child choice of a hierarchical select. */
export type HierarchicalEntry = { parent: string; child: string }

/**
 * A question's value while the form is being edited:
 * - text, select and radio questions hold a string ('' when unanswered);
 * - checkbox questions hold the ticked values, and text lists their entries in order;
 * - hierarchical selects hold a parent and child pair, or a list of pairs when multi-value;
 * - date questions hold the chosen precision and the text of each part;
 * - groups hold their items, each keyed by child question ID.
 */
export type FormValue =
  string | string[] | HierarchicalEntry | HierarchicalEntry[] | DateValue | GroupItemValue[]

export type FormValues = Record<string, FormValue>
