import { PaginatedResponseDtoOfRecordListItemDto } from '@/client/generated'
import { Table } from '@/components/Table/Table'

import { recordStatusLabels } from './labels'

type RecordsTableProps = {
  data: PaginatedResponseDtoOfRecordListItemDto
}
const RecordsTable = async ({ data: records }: RecordsTableProps) => {
  return (
    <Table>
      <caption className="visually-hidden">Organisation Records</caption>
      <thead>
        <tr>
          <th scope="col">ID</th>
          <th scope="col">Development name</th>
          <th scope="col">Records title</th>
          <th scope="col">Record Status</th>
          <th scope="col">Next update</th>
          <th scope="col">Action</th>
        </tr>
      </thead>
      <tbody>
        {records.items.map((x) => (
          <tr key={x.id}>
            <td>{x.niceTaDevelopmentId}</td>
            <td>TODO</td>
            <td>TODO</td>
            <td>{recordStatusLabels[x.recordStatus]}</td>
            <td>TODO</td>
            <td>TODO</td>
          </tr>
        ))}
      </tbody>
    </Table>
  )
}

export default RecordsTable
