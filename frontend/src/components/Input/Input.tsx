import { Input as NdsInput } from '@nice-digital/nds-input'
import type { InputProps as NdsInputProps } from '@nice-digital/nds-input'

import type { CSSProperties } from 'react'

export type InputFixedWidth = 2 | 3 | 4 | 5 | 10 | 20 | 30

export type InputFluidWidth =
  'full' | 'three-quarters' | 'two-thirds' | 'one-half' | 'one-third' | 'one-quarter'

export type InputWidth = InputFixedWidth | InputFluidWidth

export type InputProps = NdsInputProps & {
  style?: CSSProperties
  /**
   * Fixed widths (2, 3, 4, 5, 10, 20, 30) constrain the input to a character-based
   * max-width. Fluid widths resize the input as a percentage of its container.
   * Matches the GOV.UK Design System text input width scale.
   */
  width?: InputWidth
}

export const inputWidthStyles: Record<InputWidth, CSSProperties> = {
  2: { maxWidth: '2.75em' },
  3: { maxWidth: '3.75em' },
  4: { maxWidth: '4.5em' },
  5: { maxWidth: '5.5em' },
  10: { maxWidth: '11.5em' },
  20: { maxWidth: '20.5em' },
  30: { maxWidth: '29.5em' },
  full: { width: '100%' },
  'three-quarters': { width: '75%' },
  'two-thirds': { width: '66.6667%' },
  'one-half': { width: '50%' },
  'one-third': { width: '33.3333%' },
  'one-quarter': { width: '25%' },
}

export function Input({ style, width, ...rest }: InputProps) {
  const inputStyle = width ? { ...style, ...inputWidthStyles[width] } : style

  return <NdsInput style={inputStyle} {...rest} />
}
