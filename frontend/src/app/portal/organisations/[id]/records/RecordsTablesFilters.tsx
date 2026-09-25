'use client'

import { usePathname, useRouter } from 'next/navigation'

import { FilterByInput, FilterPanel } from '@nice-digital/nds-filters'

import { RecordStatus, UpdateStatus } from '@/client/generated'

import { FilterOptionsGroup } from '../_components/OrganisationFilters'

import { recordStatusLabels, updateStatusLabels } from './labels'
import { convertQueryToSearchParams, RecordsQuery } from './recordsQuery'

import type { SubmitEvent } from 'react'

function addOrRemoveItem<T>(items: T[] | undefined, item: T, isSelected: boolean): T[] {
  if (isSelected) {
    return [...new Set([...(items ?? []), item])]
  }

  return (items ?? []).filter((existingItem) => existingItem !== item)
}

type RecordsTablesFiltersTypes = {
  query: RecordsQuery
}
const RecordsTablesFilters = ({ query }: RecordsTablesFiltersTypes) => {
  const router = useRouter()
  const pathname = usePathname()
  const handleUpdatedQuery = (query: RecordsQuery) => {
    const queryParams = convertQueryToSearchParams({ ...query, page: 1 })
    router.push(`${pathname}?${queryParams.toString()}`, { scroll: false })
  }
  const handleFilterSubmit = (event: SubmitEvent<HTMLFormElement>) => {
    event.preventDefault()

    const searchValue = new FormData(event.currentTarget).get('search')
    const trimmedSearchValue = typeof searchValue === 'string' ? searchValue.trim() : undefined
    handleUpdatedQuery({ ...query, search: trimmedSearchValue })
  }
  return (
    <FilterPanel heading={'Filters'} onSubmit={handleFilterSubmit}>
      <FilterByInput
        label="Search Records"
        name="search"
        buttonLabel="Apply filter"
        inputProps={{
          placeholder: 'Enter an email address',
          defaultValue: query.search ?? '',
        }}
      ></FilterByInput>
      <FilterOptionsGroup
        heading="Record status"
        id="record-status-filter"
        options={Object.values(RecordStatus)}
        labels={recordStatusLabels}
        isSelected={(recordStatus: RecordStatus) =>
          query.recordStatus?.includes(recordStatus) ?? false
        }
        onChanged={(recordStatus, isSelected) =>
          handleUpdatedQuery({
            ...query,
            recordStatus: addOrRemoveItem(query.recordStatus, recordStatus, isSelected),
          })
        }
      />
      <FilterOptionsGroup
        heading="Update status"
        id="update-status-filter"
        options={Object.values(UpdateStatus)}
        labels={updateStatusLabels}
        isSelected={(updateStatus: UpdateStatus) => query.updateStatus == updateStatus}
        onChanged={(updateStatus, isSelected) =>
          handleUpdatedQuery({
            ...query,
            updateStatus: isSelected ? updateStatus : undefined,
          })
        }
      />
    </FilterPanel>
  )
}

export default RecordsTablesFilters
