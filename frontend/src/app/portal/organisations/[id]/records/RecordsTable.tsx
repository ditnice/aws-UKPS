import { fakePaginatedResponseDtoOfRecordListItemDto } from '@/client/generated/@faker-js/faker.gen'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { Table } from '@/components/Table/Table'

import { recordStatusLabels } from './labels'
import { RecordsQuery } from './recordsQuery'

const mockGetRecords = (input: { query: RecordsQuery }) => {
  return {
    data: fakePaginatedResponseDtoOfRecordListItemDto(),
    error: undefined,
  }
}

type RecordsTableProps = {
  query: RecordsQuery
}
const RecordsTable = async ({ query }: RecordsTableProps) => {
  // TODO: Replace with getRecords when it is implemented.
  const { data: records, error } = await mockGetRecords({ query })

  if (!records || error) {
    return (
      <ErrorState>There was a problem retrieving the records. Please try again later.</ErrorState>
    )
  }

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
