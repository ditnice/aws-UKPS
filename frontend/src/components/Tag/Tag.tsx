import { Tag as NdsTag } from '@nice-digital/nds-tag'

import type { CSSProperties, HTMLAttributes, ReactElement, ReactNode } from 'react'

export type TagColour =
  'grey' | 'green' | 'teal' | 'blue' | 'purple' | 'magenta' | 'red' | 'orange' | 'yellow'

export type TagProps = Omit<HTMLAttributes<HTMLSpanElement>, 'color'> & {
  children: ReactNode
  colour?: TagColour
  flush?: boolean
  impact?: boolean
  outline?: boolean
  remove?: ReactElement
}

const tagColourStyles: Record<TagColour, Pick<CSSProperties, 'backgroundColor' | 'color'>> = {
  grey: { backgroundColor: '#cecece', color: '#0b0c0c' },
  green: { backgroundColor: '#cfe4dc', color: '#083d29' },
  teal: { backgroundColor: '#d0e6e7', color: '#0b4144' },
  blue: { backgroundColor: '#d2e2f1', color: '#0f385c' },
  purple: { backgroundColor: '#ddd6ec', color: '#2a1950' },
  magenta: { backgroundColor: '#f4d7e5', color: '#651b3e' },
  red: { backgroundColor: '#f4d7d7', color: '#651b1b' },
  orange: { backgroundColor: '#fde4d7', color: '#7a3c1c' },
  yellow: { backgroundColor: '#ffee80', color: '#7a3c1c' },
}

export function Tag({ children, className, colour = 'grey', style, ...rest }: TagProps) {
  const tagStyle = { ...style, ...tagColourStyles[colour] }
  const tag = (
    <NdsTag style={tagStyle} {...rest}>
      {children}
    </NdsTag>
  )

  return className ? <span className={className}>{tag}</span> : tag
}
