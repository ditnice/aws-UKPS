'use client'

import { ColumnList } from '@nice-digital/nds-column-list'

import { Example } from '../../_components/Example'

const topics = [
  'Cardiovascular conditions',
  'Diabetes and other endocrinal conditions',
  'Kidney conditions',
  'Mental health and wellbeing',
  'Respiratory conditions',
  'Skin conditions',
]

export function Examples() {
  return (
    <>
      <Example title="Boxed, three columns (default)">
        <ColumnList columns={3} id="column-list-example">
          {topics.map((topic) => (
            <li key={topic}>
              <a href="#column-list-example">{topic}</a>
            </li>
          ))}
        </ColumnList>
      </Example>
      <Example title="Plain, two columns">
        <ColumnList columns={2} plain>
          {topics.map((topic) => (
            <li key={topic}>{topic}</li>
          ))}
        </ColumnList>
      </Example>
    </>
  )
}
