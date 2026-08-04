import { createElement } from 'react'

import type { AnchorHTMLAttributes, ImgHTMLAttributes } from 'react'

export function NextLinkMock({
  children,
  href,
  ...props
}: AnchorHTMLAttributes<HTMLAnchorElement>) {
  return (
    <a href={String(href)} {...props}>
      {children}
    </a>
  )
}

export function NextImageMock({
  priority: _priority,
  ...props
}: ImgHTMLAttributes<HTMLImageElement> & { priority?: boolean }) {
  return createElement('img', props)
}
