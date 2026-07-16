'use client'

import { useSyncExternalStore } from 'react'

import { Table } from '@nice-digital/nds-table'

import { Example } from '../../_components/Example'

const subscribe = () => () => undefined

export function Examples() {
  const isClient = useSyncExternalStore(
    subscribe,
    () => true,
    () => false,
  )

  if (!isClient) return null

  return (
    <>
      <Example title="Default">
        <Table>
          <tr>
            <th>Ref</th>
            <th>Title</th>
            <th>Date</th>
          </tr>
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
        </Table>
      </Example>
      <Example title="Table captions">
        <Table>
          <caption>Here is a caption!</caption>
          <tr>
            <th>Ref</th>
            <th>Title</th>
            <th>Date</th>
          </tr>
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
        </Table>
      </Example>
    </>
  )
}
