import { Textarea as NdsTextarea } from '@nice-digital/nds-textarea'
import type { TextareaProps as NdsTextareaProps } from '@nice-digital/nds-textarea'

import { inputWidthStyles } from '@/components/Input/Input'
import type {
  InputFixedWidth,
  InputFluidWidth,
  InputWidth,
  LabelSize,
} from '@/components/Input/Input'

import styles from './Textarea.module.scss'

import type { CSSProperties } from 'react'

export type TextareaFixedWidth = InputFixedWidth
export type TextareaFluidWidth = InputFluidWidth
export type TextareaWidth = InputWidth

export type TextareaProps = NdsTextareaProps & {
  labelSize?: LabelSize
  style?: CSSProperties
  title?: string
  /**
   * Fixed widths (2, 3, 4, 5, 10, 20, 30) constrain the text area to a character-based
   * max-width. Fluid widths resize the text area as a percentage of its container.
   * Matches the GOV.UK Design System text text area width scale.
   */
  width?: TextareaWidth
}

export function Textarea({ labelSize = 'default', style, title, width, ...rest }: TextareaProps) {
  const textareaStyle = width ? { ...style, ...inputWidthStyles[width] } : style

  return (
    <>
      {title && <h2>{title}</h2>}
      {labelSize === 'lead' ? (
        // The NDS text area does not take a class for its label, so a wrapper styles it.
        <div className={styles.leadLabel}>
          <NdsTextarea {...rest} style={textareaStyle} />
        </div>
      ) : (
        <NdsTextarea {...rest} style={textareaStyle} />
      )}
    </>
  )
}
