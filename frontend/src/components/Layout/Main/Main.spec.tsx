import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { Main } from './Main'
import styles from './Main.module.scss'

vi.mock('./BackToTop/BackToTop', () => ({
  BackToTop: () => <div data-testid="back-to-top" />,
}))

afterEach(cleanup)

describe('Main', () => {
  it('renders a main landmark with the main data-component attribute', () => {
    const { asFragment } = render(<Main />)

    const main = screen.getByRole('main')
    expect(main.getAttribute('data-component')).toBe('main')
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders children', () => {
    const { asFragment } = render(
      <Main>
        <p>Page content</p>
      </Main>,
    )

    expect(screen.getByText('Page content')).toBeDefined()
    expect(asFragment()).toMatchSnapshot()
  })

  it('applies padding by default', () => {
    const { asFragment } = render(<Main />)

    expect(screen.getByRole('main').classList.contains(styles.withPadding)).toBe(true)
    expect(asFragment()).toMatchSnapshot()
  })

  it('omits padding when withPadding is false', () => {
    const { asFragment } = render(<Main withPadding={false} />)

    expect(screen.getByRole('main').classList.contains(styles.withPadding)).toBe(false)
    expect(asFragment()).toMatchSnapshot()
  })

  it('preserves custom class names', () => {
    const { asFragment } = render(<Main className="additional-class" />)

    expect(screen.getByRole('main').classList.contains('additional-class')).toBe(true)
    expect(asFragment()).toMatchSnapshot()
  })

  it('forwards native main attributes', () => {
    const { asFragment } = render(<Main id="page-main" />)

    expect(screen.getByRole('main').getAttribute('id')).toBe('page-main')
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders BackToTop as a child', () => {
    const { asFragment } = render(<Main />)

    expect(screen.getByTestId('back-to-top')).toBeDefined()
    expect(asFragment()).toMatchSnapshot()
  })
})
