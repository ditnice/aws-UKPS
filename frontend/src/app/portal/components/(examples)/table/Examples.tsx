'use client'

import { Table } from '@nice-digital/nds-table'

import { Example } from '../../_components/Example'
import styles from '../../page.module.scss'

export function Examples() {
  return (
    <Example title="Guidance publication schedule">
      <Table>
        <caption>Guidance planned for publication</caption>
        <thead>
          <tr>
            <th scope="col">Topic</th>
            <th scope="col">Product type</th>
            <th className={styles.tableNumber} scope="col">
              Expected publications
            </th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <th scope="row">Cardiovascular</th>
            <td>Guideline</td>
            <td className={styles.tableNumber}>4</td>
          </tr>
          <tr>
            <th scope="row">Mental health</th>
            <td>Quality standard</td>
            <td className={styles.tableNumber}>2</td>
          </tr>
          <tr>
            <th scope="row">Respiratory</th>
            <td>Technology appraisal</td>
            <td className={styles.tableNumber}>7</td>
          </tr>
        </tbody>
      </Table>
    </Example>
  )
}
