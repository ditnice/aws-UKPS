'use client'

import { usePathname, useRouter, useSearchParams } from 'next/navigation'

import { FilterByInput, FilterGroup, FilterOption, FilterPanel } from '@nice-digital/nds-filters'

import {
  filterableRoles,
  filterableStatuses,
  lastActiveLabels,
  lastActivePresets,
  roleLabels,
  statusLabels,
  type LastActivePreset,
} from '@/app/portal/_constants/userLabels'

import type { ChangeEvent, SubmitEvent } from 'react'

type MultiFilterKey = 'status' | 'role'

export function OrganisationFilters() {
  const router = useRouter()
  const pathname = usePathname()
  const searchParams = useSearchParams()

  const selectedStatuses = searchParams.getAll('status')
  const selectedRoles = searchParams.getAll('role')
  const selectedLastActive = searchParams.get('lastActive')

  // Every filter change resets to the first page, as the previous page may no
  // longer exist within the newly filtered results.
  function updateParams(mutate: (params: URLSearchParams) => void) {
    const params = new URLSearchParams(searchParams.toString())
    mutate(params)
    params.set('page', '1')

    router.push(`${pathname}?${params.toString()}`, { scroll: false })
  }

  function handleMultiChanged(key: MultiFilterKey, value: string, isSelected: boolean) {
    const selected = searchParams.getAll(key)
    const next = isSelected ? [...selected, value] : selected.filter((entry) => entry !== value)

    updateParams((params) => {
      params.delete(key)
      next.forEach((entry) => params.append(key, entry))
    })
  }

  function handleLastActiveChanged(preset: LastActivePreset, isSelected: boolean) {
    updateParams((params) => {
      if (isSelected) {
        params.set('lastActive', preset)
      } else {
        params.delete('lastActive')
      }
    })
  }

  function handleFilterSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault()

    const emailValue = new FormData(event.currentTarget).get('email')
    const email = typeof emailValue === 'string' ? emailValue.trim() : undefined

    updateParams((params) => {
      if (email) {
        params.set('email', email)
      } else {
        params.delete('email')
      }
    })
  }

  return (
    <FilterPanel heading="Filters" onSubmit={handleFilterSubmit}>
      <FilterByInput
        label="Filter users"
        name="email"
        buttonLabel="Apply filter"
        inputProps={{
          placeholder: 'Enter an email address',
          defaultValue: searchParams.get('email') ?? '',
        }}
      ></FilterByInput>

      <FilterGroup heading="Role" id="filter-role">
        {filterableRoles.map((role) => (
          <FilterOption
            key={role}
            isSelected={selectedRoles.includes(role)}
            value={role}
            onChanged={(e: ChangeEvent<HTMLInputElement>) =>
              handleMultiChanged('role', role, e.target.checked)
            }
          >
            {roleLabels[role]}
          </FilterOption>
        ))}
      </FilterGroup>

      <FilterGroup heading="Last active" id="filter-last-active">
        {lastActivePresets.map((preset) => (
          <FilterOption
            key={preset}
            isSelected={selectedLastActive === preset}
            value={preset}
            onChanged={(e: ChangeEvent<HTMLInputElement>) =>
              handleLastActiveChanged(preset, e.target.checked)
            }
          >
            {lastActiveLabels[preset]}
          </FilterOption>
        ))}
      </FilterGroup>

      <FilterGroup heading="Status" id="filter-status">
        {filterableStatuses.map((status) => (
          <FilterOption
            key={status}
            isSelected={selectedStatuses.includes(status)}
            value={status}
            onChanged={(e: ChangeEvent<HTMLInputElement>) =>
              handleMultiChanged('status', status, e.target.checked)
            }
          >
            {statusLabels[status]}
          </FilterOption>
        ))}
      </FilterGroup>
    </FilterPanel>
  )
}
