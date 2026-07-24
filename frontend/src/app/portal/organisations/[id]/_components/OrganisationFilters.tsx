'use client'

import { FilterByInput, FilterGroup, FilterOption, FilterPanel } from '@nice-digital/nds-filters'

export function OrganisationFilters() {
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
        <FilterOption isSelected={false} value="active" onChanged={() => console.log('Changed!')}>
          Active
        </FilterOption>
        <FilterOption isSelected={false} value="inactive" onChanged={() => console.log('Changed!')}>
          Inactive
        </FilterOption>
        <FilterOption
          isSelected={false}
          value="deactivated"
          onChanged={() => console.log('Changed!')}
        >
          Deactivated
        </FilterOption>
        <FilterOption
          isSelected={false}
          value="requested"
          onChanged={() => console.log('Changed!')}
        >
          Requested
        </FilterOption>
        <FilterOption isSelected={false} value="pending" onChanged={() => console.log('Changed!')}>
          Pending
        </FilterOption>
      </FilterGroup>
    </FilterPanel>
  )
}
