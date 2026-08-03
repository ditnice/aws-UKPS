'use client'

import styles from './SkipLink.module.scss'

import type { MouseEvent, ReactNode } from 'react'

export type SkipLinkProps = {
  to: string
  children: ReactNode
}

export function SkipLink({ to, children }: SkipLinkProps) {
  function handleClick(event: MouseEvent<HTMLAnchorElement>) {
    if (!to.startsWith('#')) return

    const id = to.slice(1)
    if (!id) return

    const element = document.getElementById(id)
    if (!element) return

    event.preventDefault()
    element.tabIndex = -1
    element.focus({ preventScroll: true })
    element.scrollIntoView({ block: 'start' })
  }

  return (
    <a href={to} className={styles.link} onClick={handleClick}>
      {children}
    </a>
  )
}
