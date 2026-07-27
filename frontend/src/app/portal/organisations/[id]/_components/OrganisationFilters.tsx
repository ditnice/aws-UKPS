'use client'

import { usePathname, useRouter, useSearchParams } from 'next/navigation'

import { FilterByInput, FilterGroup, FilterOption, FilterPanel } from '@nice-digital/nds-filters'

import {
  lastActiveLabels,
  lastActivePresets,
  roleLabels,
  statusLabels,
  type LastActivePreset,
} from '@/app/portal/_constants/userLabels'
import type { UserOrgStatus, UserRole } from '@/client/generated/types.gen'

import type { ChangeEvent, SubmitEvent } from 'react'

// Rejected is not filterable as the backend never returns it
const filterableStatuses = (Object.keys(statusLabels) as UserOrgStatus[]).filter(
  (status) => status !== 'Rejected',
)

const filterableRoles = Object.keys(roleLabels) as UserRole[]

export function OrganisationFilters() {
  const router = useRouter()
  const pathname = usePathname()
  const searchParams = useSearchParams()

  const selectedStatuses = searchParams.getAll('status')
  const selectedRoles = searchParams.getAll('role')
  const selectedLastActive = searchParams.get('lastActive')

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

  function handleRoleChanged(role: UserRole, isSelected: boolean) {
    const nextRoles = isSelected
      ? [...selectedRoles, role]
      : selectedRoles.filter((value) => value !== role)

    const params = new URLSearchParams(searchParams.toString())
    params.delete('role')
    nextRoles.forEach((value) => params.append('role', value))
    params.set('page', '1')

    router.push(`${pathname}?${params.toString()}`, { scroll: false })
  }

  function handleLastActiveChanged(preset: LastActivePreset, isSelected: boolean) {
    const params = new URLSearchParams(searchParams.toString())
    if (isSelected) {
      params.set('lastActive', preset)
    } else {
      params.delete('lastActive')
    }
    params.set('page', '1')

    router.push(`${pathname}?${params.toString()}`, { scroll: false })
  }

  function handleFilterSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault()

    const emailValue = new FormData(event.currentTarget).get('email')
    const email = typeof emailValue === 'string' ? emailValue.trim() : undefined

    const params = new URLSearchParams(searchParams.toString())
    if (email) {
      params.set('email', email)
    } else {
      params.delete('email')
    }
    params.set('page', '1')

    router.push(`${pathname}?${params.toString()}`, { scroll: false })
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
              handleRoleChanged(role, e.target.checked)
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
