import { CSSProperties } from 'react'

import { Textarea as NdsTextarea } from '@nice-digital/nds-textarea'
import type { TextareaProps as NdsTextareaProps } from '@nice-digital/nds-textarea'

export type TextareaFixedWidth = 2 | 3 | 4 | 5 | 10 | 20 | 30

export type TextareaFluidWidth =
  'full' | 'three-quarters' | 'two-thirds' | 'one-half' | 'one-third' | 'one-quarter'

export type TextareaWidth = TextareaFixedWidth | TextareaFluidWidth

export type TextareaProps = NdsTextareaProps & {
  style?: CSSProperties
  /**
   * Fixed widths (2, 3, 4, 5, 10, 20, 30) constrain the text area to a character-based
   * max-width. Fluid widths resize the text area as a percentage of its container.
   * Matches the GOV.UK Design System text text area width scale.
   */
  width?: TextareaWidth
}

export const textareaWidthStyles: Record<TextareaWidth, CSSProperties> = {
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

export function Textarea({ style, width, ...rest }: TextareaProps) {
  const textareaStyle = width ? { ...style, ...textareaWidthStyles[width] } : style

  return <NdsTextarea style={textareaStyle} {...rest} />
}
