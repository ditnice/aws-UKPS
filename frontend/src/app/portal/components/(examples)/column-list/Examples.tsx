'use client'

import { ColumnList } from '@nice-digital/nds-column-list'

import { Example } from '../../_components/Example'

const items = (
  <>
    <li>One</li>
    <li>Two</li>
    <li>Three</li>
    <li>Four</li>
    <li>Five</li>
    <li>Six</li>
  </>
)

export function Examples() {
  return (
    <>
      <Example title="Default column list">
        <ColumnList>{items}</ColumnList>
      </Example>
      <Example title="Column list with two columns">
        <ColumnList columns={2}>{items}</ColumnList>
      </Example>
      <Example title="Plain column list">
        <ColumnList plain>{items}</ColumnList>
      </Example>
    </>
  )
}
