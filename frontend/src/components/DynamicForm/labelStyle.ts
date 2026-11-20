import type { LabelSize } from '@/components/Input/Input'

/**
 * How a question shows its own label or legend: as the page heading on a single-question page
 * (`heading`), as bold lead text for each top-level question on a page of several (`lead`), or at
 * the standard size inside another question, such as one revealed beneath an option or in a
 * group's items (`default`).
 */
export type LabelStyle = 'heading' | 'lead' | 'default'

/** The size of the shared field's label or legend for a question shown in this style. */
export const labelSizeFor = (labelStyle: LabelStyle): LabelSize =>
  labelStyle === 'lead' ? 'lead' : 'default'
