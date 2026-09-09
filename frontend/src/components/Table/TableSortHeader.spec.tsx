import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import {
  TableSortHeaderButton,
  TableSortHeaderButtonProps,
  TableSortHeaderLink,
  type TableSortDirection,
} from './TableSortHeader'

afterEach(cleanup)

type Variant = 'button' | 'link'
const variants: Variant[] = ['button', 'link']

function renderButtonHeader(
  direction: TableSortDirection,
  props?: Partial<Omit<TableSortHeaderButtonProps, 'children' | 'direction'>>,
) {
  const onSort = props?.onSort ?? vi.fn()
  const result = render(
    <table>
      <thead>
        <tr>
          <TableSortHeaderButton direction={direction} onSort={onSort} {...props}>
            Name
          </TableSortHeaderButton>
        </tr>
      </thead>
    </table>,
  )

  return { ...result, onSort }
}

function renderLinkHeader(
  direction: TableSortDirection,
  props?: Partial<Omit<TableSortHeaderButtonProps, 'children' | 'direction'>>,
) {
  const result = render(
    <table>
      <thead>
        <tr>
          <TableSortHeaderLink direction={direction} createHref={(next) => `#${next}`} {...props}>
            Name
          </TableSortHeaderLink>
        </tr>
      </thead>
    </table>,
  )

  return { ...result }
}

const renderVariantHeader = (direction: TableSortDirection, variant: Variant) => {
  return variant === 'button' ? renderButtonHeader(direction) : renderLinkHeader(direction)
}

describe('TableSortHeader', () => {
  const dataWithVariant: [TableSortDirection, number, Variant][] = (
    [
      ['none', 2],
      ['ascending', 1],
      ['descending', 1],
    ] as [TableSortDirection, number][]
  ).flatMap(([direction, pathCount]) =>
    variants.map((variant): [TableSortDirection, number, Variant] => [
      direction,
      pathCount,
      variant,
    ]),
  )
  it.each(dataWithVariant)(
    'renders the %s state',
    (direction: TableSortDirection, pathCount: number, variant: Variant) => {
      const { asFragment, container } = renderVariantHeader(direction, variant)
      const header = screen.getByRole('columnheader', { name: 'Name' })
      const indicator = container.querySelector('svg')

      expect(header.getAttribute('aria-sort')).toBe(direction)
      expect(header.getAttribute('scope')).toBe('col')
      expect(screen.getByRole(variant, { name: 'Name' })).toBeDefined()
      expect(screen.queryByRole('img')).toBeNull()
      expect(indicator?.getAttribute('aria-hidden')).toBe('true')
      expect(indicator?.getAttribute('fill')).toBe('currentColor')
      expect(indicator?.querySelectorAll('path')).toHaveLength(pathCount)
      expect(asFragment()).toMatchSnapshot()
    },
  )

  const nextDirectionData: [TableSortDirection, TableSortDirection][] = [
    ['none', 'ascending'],
    ['ascending', 'descending'],
    ['descending', 'ascending'],
  ]
  it.each(nextDirectionData)('button variant - requests %s → %s', (direction, nextDirection) => {
    const { onSort } = renderButtonHeader(direction)

    fireEvent.click(screen.getByRole('button', { name: 'Name' }))

    expect(onSort).toHaveBeenCalledOnce()
    expect(onSort).toHaveBeenCalledWith(nextDirection)
  })

  it.each(nextDirectionData)('button variant - requests %s → %s', (direction, nextDirection) => {
    const {} = renderLinkHeader(direction)

    const link = screen.getByRole('link', { name: 'Name' })
    const href = link.getAttribute('href')
    expect(href).toBe(`#${nextDirection}`)
  })

  it('forwards className and native header attributes', () => {
    const { container } = renderButtonHeader('none', {
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
