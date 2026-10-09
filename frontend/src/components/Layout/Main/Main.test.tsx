import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { Main } from './Main'
import styles from './Main.module.scss'

vi.mock('./BackToTop/BackToTop', () => ({
  BackToTop: () => <div data-testid="back-to-top" />,
}))
describe('Main', () => {
  it('renders a main landmark with the main data-component attribute', () => {
    const { asFragment } = render(<Main />)

    const main = screen.getByRole('main')
    expect(main.getAttribute('data-component')).toBe('main')
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders children', () => {
    render(
      <Main>
        <p>Page content</p>
      </Main>,
    )

    expect(screen.getByText('Page content')).toBeInTheDocument()
  })

  it('applies padding by default', () => {
    render(<Main />)

    expect(screen.getByRole('main').classList.contains(styles.withPadding)).toBe(true)
  })

  it('omits padding when withPadding is false', () => {
    render(<Main withPadding={false} />)

    expect(screen.getByRole('main').classList.contains(styles.withPadding)).toBe(false)
  })

  it('preserves custom class names', () => {
    render(<Main className="additional-class" />)

    expect(screen.getByRole('main').classList.contains('additional-class')).toBe(true)
  })

  it('forwards native main attributes', () => {
    render(<Main id="page-main" />)

    expect(screen.getByRole('main').getAttribute('id')).toBe('page-main')
  })

  it('renders BackToTop as a child', () => {
    render(<Main />)

    expect(screen.getByTestId('back-to-top')).toBeInTheDocument()
  })
})
