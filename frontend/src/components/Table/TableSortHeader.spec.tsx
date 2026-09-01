import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import {
  TableSortHeader,
  type TableSortDirection,
  type TableSortHeaderProps,
} from './TableSortHeader'

afterEach(cleanup)

function renderHeader(
  direction: TableSortDirection,
  props?: Partial<Omit<TableSortHeaderProps, 'children' | 'direction'>>,
) {
  const onSort = props?.onSort ?? vi.fn()
  const result = render(
    <table>
      <thead>
        <tr>
          <TableSortHeader direction={direction} onSort={onSort} {...props}>
            Name
          </TableSortHeader>
        </tr>
      </thead>
    </table>,
  )

  return { ...result, onSort }
}

describe('TableSortHeader', () => {
  it.each([
    ['none', 2],
    ['ascending', 1],
    ['descending', 1],
  ] as const)('renders the %s state', (direction, pathCount) => {
    const { asFragment, container } = renderHeader(direction)
    const header = screen.getByRole('columnheader', { name: 'Name' })
    const indicator = container.querySelector('svg')

    expect(header.getAttribute('aria-sort')).toBe(direction)
    expect(header.getAttribute('scope')).toBe('col')
    expect(screen.getByRole('button', { name: 'Name' })).toBeDefined()
    expect(screen.queryByRole('img')).toBeNull()
    expect(indicator?.getAttribute('aria-hidden')).toBe('true')
    expect(indicator?.getAttribute('fill')).toBe('currentColor')
    expect(indicator?.querySelectorAll('path')).toHaveLength(pathCount)
    expect(asFragment()).toMatchSnapshot()
  })

  it.each([
    ['none', 'ascending'],
    ['ascending', 'descending'],
    ['descending', 'ascending'],
  ] as const)('requests %s → %s', (direction, nextDirection) => {
    const { onSort } = renderHeader(direction)

    fireEvent.click(screen.getByRole('button', { name: 'Name' }))

    expect(onSort).toHaveBeenCalledOnce()
    expect(onSort).toHaveBeenCalledWith(nextDirection)
  })

  it('forwards className and native header attributes', () => {
    const { container } = renderHeader('none', {
      className: 'additional-class',
      colSpan: 2,
      id: 'sort-header',
      scope: 'row',
    })

    const header = container.querySelector('#sort-header')
    if (!header) throw new Error('Expected the sort header to render')

    expect(header.classList.contains('additional-class')).toBe(true)
    expect(header.getAttribute('colspan')).toBe('2')
    expect(header.getAttribute('scope')).toBe('row')
  })
})
