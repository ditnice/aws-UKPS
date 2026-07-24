'use client'

import { usePathname, useRouter, useSearchParams } from 'next/navigation'

import { FilterByInput, FilterGroup, FilterOption, FilterPanel } from '@nice-digital/nds-filters'

import { statusLabels } from '@/app/portal/_constants/userLabels'
import type { UserOrgStatus } from '@/client/generated/types.gen'

import type { ChangeEvent } from 'react'

// Rejected is not filterable as the backend never returns it
const filterableStatuses = (Object.keys(statusLabels) as UserOrgStatus[]).filter(
  (status) => status !== 'Rejected',
)

export function OrganisationFilters() {
  const router = useRouter()
  const pathname = usePathname()
  const searchParams = useSearchParams()

  const selectedStatuses = searchParams.getAll('status')

  function handleStatusChanged(status: UserOrgStatus, isSelected: boolean) {
    const nextStatuses = isSelected
      ? [...selectedStatuses, status]
      : selectedStatuses.filter((value) => value !== status)

    const params = new URLSearchParams(searchParams.toString())
    params.delete('status')
    nextStatuses.forEach((value) => params.append('status', value))
    params.set('page', '1')

    router.push(`${pathname}?${params.toString()}`, { scroll: false })
  }

  return (
    <FilterPanel heading="Filters">
      <FilterByInput
        label="Filter users"
        name="Filter users"
        buttonLabel="Apply filter"
        inputProps={{ placeholder: 'Enter an email address' }}
      ></FilterByInput>

      <FilterGroup heading="Role" id="filter-role">
        <FilterOption
          isSelected={false}
          value="Standard user"
          onChanged={() => console.log('Changed!')}
        >
          Standard user
        </FilterOption>
        <FilterOption
          isSelected={false}
          value="Champion user"
          onChanged={() => console.log('Changed!')}
        >
          Champion user
        </FilterOption>
      </FilterGroup>

      <FilterGroup heading="Last active" id="filter-last-active">
        <FilterOption
          isSelected={false}
          value="In the last week"
          onChanged={() => console.log('Changed!')}
        >
          In the last week
        </FilterOption>
        <FilterOption
          isSelected={false}
          value="In the last month"
          onChanged={() => console.log('Changed!')}
        >
          In the last month
        </FilterOption>
        <FilterOption
          isSelected={false}
          value="In the last 6 months"
          onChanged={() => console.log('Changed!')}
        >
          In the last 6 months
        </FilterOption>
        <FilterOption
          isSelected={false}
          value="In the last year"
          onChanged={() => console.log('Changed!')}
        >
          In the last year
        </FilterOption>
      </FilterGroup>

      <FilterGroup heading="Status" id="filter-status">
        {filterableStatuses.map((status) => (
          <FilterOption
            key={status}
            isSelected={selectedStatuses.includes(status)}
            value={status}
            onChanged={(e: ChangeEvent<HTMLInputElement>) =>
              handleStatusChanged(status, e.target.checked)
            }
          >
            {statusLabels[status]}
          </FilterOption>
        ))}
      </FilterGroup>
    </FilterPanel>
  )
}
