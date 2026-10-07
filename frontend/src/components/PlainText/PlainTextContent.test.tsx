import { render, screen, within } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import PlainTextContent from './PlainTextContent'

describe('PlainTextContent', () => {
  it.each(['', '   ', '\n\n \n'])('renders no content for blank input: %j', (text) => {
    const { container } = render(<PlainTextContent text={text} />)
    expect(container).toBeEmptyDOMElement()
  })

  it('separates paragraphs at blank lines and trims surrounding whitespace', () => {
    const { container } = render(
      <PlainTextContent text={'  First paragraph  \n \n Second paragraph  '} />,
    )
    const paragraphs = container.querySelectorAll('p')

    expect(paragraphs).toHaveLength(2)
    expect(paragraphs[0]).toHaveTextContent('First paragraph')
    expect(paragraphs[1]).toHaveTextContent('Second paragraph')
  })

  it('preserves single line breaks within a paragraph', () => {
    const { container } = render(<PlainTextContent text={'First line\nSecond line\nThird line'} />)

    expect(container.querySelectorAll('p')).toHaveLength(1)
    expect(container.querySelectorAll('br')).toHaveLength(2)
    expect(screen.getByText('First line')).toBeInTheDocument()
    expect(screen.getByText('Second line')).toBeInTheDocument()
    expect(screen.getByText('Third line')).toBeInTheDocument()
  })

  it('renders bullet-only blocks as lists alongside ordinary paragraphs', () => {
    const { container } = render(
      <PlainTextContent text={'Introduction\n\n - First item\n - Second item\n\nConclusion'} />,
    )
    const list = screen.getByRole('list')

    expect(
      within(list)
        .getAllByRole('listitem')
        .map((item) => item.textContent),
    ).toEqual(['First item', 'Second item'])
    expect(container.querySelectorAll('p')).toHaveLength(2)
    expect(screen.getByText('Introduction')).toBeInTheDocument()
    expect(screen.getByText('Conclusion')).toBeInTheDocument()
  })

  it('keeps mixed bullet and ordinary lines as a paragraph', () => {
    const { container } = render(<PlainTextContent text={'- Bullet\nOrdinary line'} />)

    expect(screen.queryByRole('list')).not.toBeInTheDocument()
    expect(container.querySelectorAll('p')).toHaveLength(1)
    expect(container.querySelectorAll('br')).toHaveLength(1)
  })

  it('renders HTML-looking input as text rather than executable markup', () => {
    const text = '<script>alert("unsafe")</script> <img src=x onerror=alert(1)>'
    const { container } = render(<PlainTextContent text={text} />)

    expect(screen.getByText(text)).toBeInTheDocument()
    expect(container.querySelector('script, img')).toBeNull()
  })
})
