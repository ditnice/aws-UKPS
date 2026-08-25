'use client'

import clsx from 'clsx'
import { useLayoutEffect, useState } from 'react'

import ChevronUp from '@nice-digital/icons/lib/ChevronUp'
import { Container } from '@nice-digital/nds-container'

import styles from './BackToTop.module.scss'

import type { MouseEvent } from 'react'

export function BackToTop() {
  // Default to hidden: this is server-rendered before any measurement is possible,
  // so starting visible would flash the link on short pages before the effect below
  // (which only runs after hydration) can hide it again.
  const [isVisible, setIsVisible] = useState(false)

  useLayoutEffect(() => {
    function checkVisibility() {
      const footer = document.querySelector('[data-component="footer"]')
      const footerHeight = footer instanceof HTMLElement ? footer.offsetHeight : 0
      const scrollableHeight = document.documentElement.scrollHeight - footerHeight

      setIsVisible(scrollableHeight > window.innerHeight)
    }

    checkVisibility()

    window.addEventListener('resize', checkVisibility)

    const resizeObserver = new ResizeObserver(checkVisibility)
    const resizeObserverTarget = document.querySelector('[data-component="main"]') ?? document.body
    resizeObserver.observe(resizeObserverTarget)

    return () => {
      window.removeEventListener('resize', checkVisibility)
      resizeObserver.disconnect()
    }
  }, [])

  function handleClick(event: MouseEvent<HTMLAnchorElement>) {
    const id = event.currentTarget.hash.slice(1)
    if (!id) return

    const element = document.getElementById(id)
    if (!element) return

    event.preventDefault()
    element.tabIndex = -1
    element.focus({ preventScroll: true })
    element.scrollIntoView({ block: 'start' })
  }

  return (
    <div className={clsx(styles.wrapper, !isVisible && styles.hidden)}>
      <nav aria-labelledby="back-to-top-link" className={styles.nav}>
        <a className={styles.anchor} id="back-to-top-link" href="#top" onClick={handleClick}>
          <Container className={styles.container}>
            <ChevronUp /> Back to top
          </Container>
        </a>
      </nav>
    </div>
  )
}
