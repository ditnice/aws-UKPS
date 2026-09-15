'use client'

import { usePathname, useRouter } from 'next/navigation'

import { FilterByInput, FilterPanel } from '@nice-digital/nds-filters'

import { RecordStatus } from '@/client/generated'

import { FilterOptionsGroup } from '../_components/OrganisationFilters'

import { convertQueryToSearchParams, RecordsQuery } from './recordsQuery'

import type { SubmitEvent } from 'react'

const recordStatusLabels: Record<RecordStatus, string> = {
  Unpublished: 'Unpublished',
  Active: 'Active',
  OnHold: 'On Hold',
  Archived: 'Archived',
}

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
    const queryParams = convertQueryToSearchParams({ ...query, Page: 1 })
    router.push(`${pathname}?${queryParams.toString()}`, { scroll: false })
  }
  const handleFilterSubmit = (event: SubmitEvent<HTMLFormElement>) => {
    event.preventDefault()

    const searchValue = new FormData(event.currentTarget).get('search')
    const trimmedSearchValue = typeof searchValue === 'string' ? searchValue.trim() : undefined
    handleUpdatedQuery({ ...query, Search: trimmedSearchValue })
  }
  return (
    <FilterPanel heading={'Filters'} onSubmit={handleFilterSubmit}>
      <FilterByInput
        label="Search Records"
        name="search"
        buttonLabel="Apply filter"
        inputProps={{
          placeholder: 'Enter an email address',
          defaultValue: query.Search ?? '',
        }}
      ></FilterByInput>
      <FilterOptionsGroup
        heading="Record Status"
        id="record-status-filter"
        options={Object.values(RecordStatus)}
        labels={recordStatusLabels}
        isSelected={(recordStatus: RecordStatus) =>
          query.RecordStatus?.includes(recordStatus) ?? false
        }
        onChanged={(recordStatus, isSelected) =>
          handleUpdatedQuery({
            ...query,
            RecordStatus: addOrRemoveItem(query.RecordStatus, recordStatus, isSelected),
          })
        }
      />
      <div>Update status</div>
    </FilterPanel>
  )
}

export default RecordsTablesFilters
