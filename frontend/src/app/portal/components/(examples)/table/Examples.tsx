'use client'

import { useState, useSyncExternalStore } from 'react'

import { Table } from '@/components/Table/Table'
import {
  TableSortHeaderButton,
  TableSortHeaderLink,
  type TableSortDirection,
} from '@/components/Table/TableSortHeader'

import { Example } from '../../_components/Example'

const subscribe = () => () => undefined

const sortableRows = [
  { date: '2022-08-27', dateLabel: '27/08/2022', ref: 'ABC1', title: 'Lorem ipsum dolor sit amet' },
  {
    date: '2023-12-25',
    dateLabel: '25/12/2023',
    ref: 'XYZ2',
    title: 'Aliquam consectetur posuere nibh dapibus consequat',
  },
  { date: '2023-04-12', dateLabel: '12/04/2023', ref: 'DEF3', title: 'Dolor sit amet' },
] as const

type SortColumn = 'ref' | 'title' | 'date'
type ActiveSortDirection = Exclude<TableSortDirection, 'none'>
type SortState = { column: SortColumn; direction: ActiveSortDirection } | null

const collator = new Intl.Collator('en-GB', { numeric: true, sensitivity: 'base' })

export function Examples() {
  const [sort, setSort] = useState<SortState>(null)
  const isClient = useSyncExternalStore(
    subscribe,
    () => true,
    () => false,
  )

  if (!isClient) return null

  const rows = sort
    ? [...sortableRows].sort((rowA, rowB) => {
        const comparison = collator.compare(rowA[sort.column], rowB[sort.column])
        return sort.direction === 'ascending' ? comparison : -comparison
      })
    : sortableRows

  const directionFor = (column: SortColumn): TableSortDirection =>
    sort?.column === column ? sort.direction : 'none'

  return (
    <>
      <Example title="Default">
        <Table>
          <thead>
            <tr>
              <th scope="col">Ref</th>
              <th scope="col">Title</th>
              <th scope="col">Date</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td>ABC1</td>
              <td>Lorem ipsum dolor sit amet</td>
              <td>27/08/2022</td>
            </tr>
            <tr>
              <td>XYZ2</td>
              <td>Aliquam consectetur posuere nibh dapibus consequat</td>
              <td>25/12/2023</td>
            </tr>
          </tbody>
        </Table>
      </Example>
      <Example title="Table captions">
        <Table>
          <caption>Here is a caption!</caption>
          <thead>
            <tr>
              <th scope="col">Ref</th>
              <th scope="col">Title</th>
              <th scope="col">Date</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td>ABC1</td>
              <td>Lorem ipsum dolor sit amet</td>
              <td>27/08/2022</td>
            </tr>
            <tr>
              <td>XYZ2</td>
              <td>Aliquam consectetur posuere nibh dapibus consequat</td>
              <td>25/12/2023</td>
            </tr>
          </tbody>
        </Table>
      </Example>
      <Example title="Full width, columns sized by content">
        <Table columnWidth="content">
          <thead>
            <tr>
              <th scope="col">Ref</th>
              <th scope="col">Title</th>
              <th scope="col">Date</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td>ABC1</td>
              <td>Lorem ipsum dolor sit amet</td>
              <td>27/08/2022</td>
            </tr>
            <tr>
              <td>XYZ2</td>
              <td>Aliquam consectetur posuere nibh dapibus consequat</td>
              <td>25/12/2023</td>
            </tr>
          </tbody>
        </Table>
      </Example>
      <Example title="Full width, equal columns">
        <Table columnWidth="equal">
          <thead>
            <tr>
              <th scope="col">Ref</th>
              <th scope="col">Title</th>
              <th scope="col">Date</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td>ABC1</td>
              <td>Lorem ipsum dolor sit amet</td>
              <td>27/08/2022</td>
            </tr>
            <tr>
              <td>XYZ2</td>
              <td>Aliquam consectetur posuere nibh dapibus consequat</td>
              <td>25/12/2023</td>
            </tr>
          </tbody>
        </Table>
      </Example>
      <Example title="Sortable table">
        <Table columnWidth="content">
          <caption>
            Applications{' '}
            <span className="visually-hidden">(column headers with buttons are sortable).</span>
          </caption>
          <thead>
            <tr>
              <TableSortHeaderButton
                direction={directionFor('ref')}
                onSort={(direction) => setSort({ column: 'ref', direction })}
              >
                Ref
              </TableSortHeaderButton>
              <TableSortHeaderButton
                direction={directionFor('title')}
                onSort={(direction) => setSort({ column: 'title', direction })}
              >
                Title
              </TableSortHeaderButton>
              <TableSortHeaderButton
                direction={directionFor('date')}
                onSort={(direction) => setSort({ column: 'date', direction })}
              >
                Date
              </TableSortHeaderButton>
            </tr>
          </thead>
          <tbody>
            {rows.map((row) => (
              <tr key={row.ref}>
                <td>{row.ref}</td>
                <td>{row.title}</td>
                <td>{row.dateLabel}</td>
              </tr>
            ))}
          </tbody>
        </Table>
      </Example>
      <Example title="Sortable table with links">
        <Table columnWidth="content">
          <caption>
            Applications{' '}
            <span className="visually-hidden">(column headers with buttons are sortable).</span>
          </caption>
          <thead>
            <tr>
              <TableSortHeaderLink
                direction={directionFor('ref')}
                createHref={(direction) => `/example?sortColumn=ref&sortDirection=${direction}`}
              >
                Ref
              </TableSortHeaderLink>
              <TableSortHeaderLink
                direction={directionFor('title')}
                createHref={(direction) => `/example?sortColumn=title&sortDirection=${direction}`}
              >
                Title
              </TableSortHeaderLink>
              <TableSortHeaderLink
                direction={directionFor('date')}
                createHref={(direction) => `/example?sortColumn=date&sortDirection=${direction}`}
              >
                Date
              </TableSortHeaderLink>
            </tr>
          </thead>
          <tbody>
            {rows.map((row) => (
              <tr key={row.ref}>
                <td>{row.ref}</td>
                <td>{row.title}</td>
                <td>{row.dateLabel}</td>
              </tr>
            ))}
          </tbody>
        </Table>
      </Example>
    </>
  )
}
