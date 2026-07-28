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

interface FilterOptionsGroupProps<T extends string> {
  heading: string
  id: string
  options: readonly T[]
  labels: Record<T, string>
  /** Single and multi select groups differ only in how selection is derived. */
  isSelected: (option: T) => boolean
  onChanged: (option: T, isSelected: boolean) => void
}

function FilterOptionsGroup<T extends string>({
  heading,
  id,
  options,
  labels,
  isSelected,
  onChanged,
}: FilterOptionsGroupProps<T>) {
  return (
    <FilterGroup heading={heading} id={id}>
      {options.map((option) => (
        <FilterOption
          key={option}
          isSelected={isSelected(option)}
          value={option}
          onChanged={(e: ChangeEvent<HTMLInputElement>) => onChanged(option, e.target.checked)}
        >
          {labels[option]}
        </FilterOption>
      ))}
    </FilterGroup>
  )
}

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

      <FilterOptionsGroup
        heading="Role"
        id="filter-role"
        options={filterableRoles}
        labels={roleLabels}
        isSelected={(role) => selectedRoles.includes(role)}
        onChanged={(role, isSelected) => handleMultiChanged('role', role, isSelected)}
      />

      <FilterOptionsGroup
        heading="Last active"
        id="filter-last-active"
        options={lastActivePresets}
        labels={lastActiveLabels}
        isSelected={(preset) => selectedLastActive === preset}
        onChanged={handleLastActiveChanged}
      />

      <FilterOptionsGroup
        heading="Status"
        id="filter-status"
        options={filterableStatuses}
        labels={statusLabels}
        isSelected={(status) => selectedStatuses.includes(status)}
        onChanged={(status, isSelected) => handleMultiChanged('status', status, isSelected)}
      />
    </FilterPanel>
  )
}
