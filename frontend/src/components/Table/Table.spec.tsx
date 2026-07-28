import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import { Table } from './Table'

afterEach(cleanup)

const rows = (
  <>
    <thead>
      <tr>
        <th scope="col">Ref</th>
        <th scope="col">Title</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td>ABC1</td>
        <td>Lorem ipsum dolor sit amet</td>
      </tr>
    </tbody>
  </>
)

describe('Table', () => {
  it('renders an unwrapped design system table by default', () => {
    const { container } = render(<Table>{rows}</Table>)

    const table = container.querySelector('table')
    expect(table).not.toBeNull()
    expect(table?.classList.contains('table')).toBe(true)
    expect(table?.parentElement).toBe(container)
    expect(screen.getByRole('columnheader', { name: 'Ref' })).toBeDefined()
    expect(screen.getByRole('cell', { name: 'ABC1' })).toBeDefined()
  })

  it('wraps the table in a scroll container for the content variant', () => {
    const { container } = render(<Table columnWidth="content">{rows}</Table>)

    const table = container.querySelector('table')
    expect(table).not.toBeNull()
    expect(table?.parentElement?.tagName).toBe('DIV')
    expect(table?.classList.contains('table')).toBe(true)
    expect(table?.classList.length).toBeGreaterThan(1)
  })

  it('adds an extra class for the equal variant', () => {
    const { container: contentContainer } = render(<Table columnWidth="content">{rows}</Table>)
    const { container: equalContainer } = render(<Table columnWidth="equal">{rows}</Table>)

    const contentClasses = Array.from(contentContainer.querySelector('table')?.classList ?? [])
    const equalClasses = Array.from(equalContainer.querySelector('table')?.classList ?? [])

    expect(equalClasses.length).toBe(contentClasses.length + 1)
    expect(contentClasses.every((name) => equalClasses.includes(name))).toBe(true)
  })

  it('forwards className and table attributes in both modes', () => {
    const { container } = render(
      <Table aria-label="Users" className="additional-class">
        {rows}
      </Table>,
    )
    const table = container.querySelector('table')
    expect(table?.classList.contains('additional-class')).toBe(true)
    expect(table?.getAttribute('aria-label')).toBe('Users')

    cleanup()

    const { container: wrappedContainer } = render(
      <Table aria-label="Users" className="additional-class" columnWidth="equal">
        {rows}
      </Table>,
    )
    const wrappedTable = wrappedContainer.querySelector('table')
    expect(wrappedTable?.classList.contains('additional-class')).toBe(true)
    expect(wrappedTable?.getAttribute('aria-label')).toBe('Users')
  })

  it('renders a caption child', () => {
    const { container } = render(
      <Table columnWidth="content">
        <caption>Organisation users</caption>
        {rows}
      </Table>,
    )

    expect(container.querySelector('caption')?.textContent).toBe('Organisation users')
  })
})
