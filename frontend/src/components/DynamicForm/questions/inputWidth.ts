import type { FormInputWidth } from '@/client/generated'
import type { InputFluidWidth, InputWidth } from '@/components/Input/Input'

// Exhaustive, so a width added to the server's FormInputWidth fails to compile until it is mapped.
const inputWidths: Record<Exclude<FormInputWidth, 'Default'>, InputWidth> = {
  Full: 'full',
  ThreeQuarters: 'three-quarters',
  TwoThirds: 'two-thirds',
  OneHalf: 'one-half',
  OneThird: 'one-third',
  OneQuarter: 'one-quarter',
  Chars2: 2,
  Chars3: 3,
  Chars4: 4,
  Chars5: 5,
  Chars10: 10,
  Chars20: 20,
  Chars30: 30,
}

/** The input width the server set for a question, or undefined for the control's default. */
export function inputWidthOf(width?: FormInputWidth | null): InputWidth | undefined {
  return width && width !== 'Default' ? inputWidths[width] : undefined
}

/**
 * The share of the container the server set for a question, or undefined for the default. A
 * character count is ignored: the server rejects one on a text list, whose entries need a share.
 */
export function fluidInputWidthOf(width?: FormInputWidth | null): InputFluidWidth | undefined {
  const inputWidth = inputWidthOf(width)
  return typeof inputWidth === 'string' ? inputWidth : undefined
}
