import { act, cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { BackToTop } from './BackToTop'
import styles from './BackToTop.module.scss'

vi.mock('@nice-digital/nds-container', () => ({
  Container: ({ children }: { children: import('react').ReactNode }) => <div>{children}</div>,
}))

function setHeights({
  scrollHeight,
  innerHeight,
  footerHeight = 0,
}: {
  scrollHeight: number
  innerHeight: number
  footerHeight?: number
}) {
  Object.defineProperty(document.documentElement, 'scrollHeight', {
    configurable: true,
    value: scrollHeight,
  })
  Object.defineProperty(window, 'innerHeight', { configurable: true, value: innerHeight })

  if (footerHeight) {
    const footer = document.createElement('footer')
    footer.setAttribute('data-component', 'footer')
    Object.defineProperty(footer, 'offsetHeight', { configurable: true, value: footerHeight })
    document.body.append(footer)
  }
}

function isHidden(element: Element) {
  return element.className.split(' ').includes(styles.hidden)
}

beforeEach(() => {
  setHeights({ scrollHeight: 2000, innerHeight: 800 })
})

afterEach(() => {
  vi.unstubAllGlobals()
  document.body.replaceChildren()
  cleanup()
})

describe('BackToTop', () => {
  it('renders a back to top link', () => {
    render(<BackToTop />)

    expect(screen.getByRole('link', { name: 'Back to top' }).getAttribute('href')).toBe('#top')
  })

  it('focuses the top target when clicked', () => {
    const target = document.createElement('div')
    target.id = 'top'
    target.scrollIntoView = vi.fn()
    document.body.append(target)

    render(<BackToTop />)

    fireEvent.click(screen.getByRole('link', { name: 'Back to top' }))

    expect(document.activeElement).toBe(target)
    expect(target.tabIndex).toBe(-1)
    expect(target.scrollIntoView).toHaveBeenCalledWith({ block: 'start' })
  })

  it('does not change focus when the top target is missing', () => {
    render(<BackToTop />)

    fireEvent.click(screen.getByRole('link', { name: 'Back to top' }))

    expect(document.activeElement).toBe(document.body)
  })

  it('stays visible when the page content overflows the viewport', () => {
    setHeights({ scrollHeight: 2000, innerHeight: 800, footerHeight: 200 })

    const { container } = render(<BackToTop />)

    expect(isHidden(container.firstElementChild as Element)).toBe(false)
  })

  it('is visually hidden, but still reachable by keyboard, when content does not fill the viewport', () => {
    setHeights({ scrollHeight: 500, innerHeight: 800 })

    const { container } = render(<BackToTop />)

    expect(isHidden(container.firstElementChild as Element)).toBe(true)
    // still present and interactive for assistive tech / keyboard users
    expect(screen.getByRole('link', { name: 'Back to top' })).toBeTruthy()
  })

  it('rechecks visibility when the window is resized', () => {
    setHeights({ scrollHeight: 500, innerHeight: 1000 })
    const { container } = render(<BackToTop />)

    expect(isHidden(container.firstElementChild as Element)).toBe(true)

    Object.defineProperty(window, 'innerHeight', { configurable: true, value: 300 })
    fireEvent(window, new Event('resize'))

    expect(isHidden(container.firstElementChild as Element)).toBe(false)
  })

  it('rechecks visibility when observed content is resized', () => {
    let triggerResize = () => {}
    vi.stubGlobal(
      'ResizeObserver',
      vi.fn().mockImplementation(function (this: unknown, callback: () => void) {
        triggerResize = callback
        return { observe: vi.fn(), unobserve: vi.fn(), disconnect: vi.fn() }
      }),
    )

    setHeights({ scrollHeight: 500, innerHeight: 1000 })
    const { container } = render(<BackToTop />)

    expect(isHidden(container.firstElementChild as Element)).toBe(true)

    Object.defineProperty(document.documentElement, 'scrollHeight', {
      configurable: true,
      value: 3000,
    })
    act(() => triggerResize())

    expect(isHidden(container.firstElementChild as Element)).toBe(false)
  })
})
