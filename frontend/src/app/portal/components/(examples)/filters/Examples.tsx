'use client'

import { type FormEvent, useState } from 'react'

import {
  FilterByInput,
  FilterGroup,
  FilterOption,
  FilterPanel,
  FilterSummary,
} from '@nice-digital/nds-filters'

import { Example } from '../../_components/Example'

const preventSubmit = (event: FormEvent<HTMLFormElement>) => event.preventDefault()

export function Examples() {
  const [filterTypes, setFilterTypes] = useState<string[]>(['Guidance'])
  const [sortOrder, setSortOrder] = useState('relevance')
  const [reference, setReference] = useState('NG123')

  const toggleFilterType = (type: string, checked: boolean) => {
    setFilterTypes((current) =>
      checked ? [...current, type] : current.filter((item) => item !== type),
    )
  }

  const activeFilters = filterTypes.map((type) => ({
    label: type,
    onClick: () => setFilterTypes((current) => current.filter((item) => item !== type)),
  }))

  return (
    <>
      <Example title="Interactive filter panel">
        <FilterPanel heading="Filter guidance" headingLevel={3} onSubmit={preventSubmit}>
          <FilterGroup
            collapseByDefault
            heading="Content type"
            id="showcase-content-type"
            selectedCount={filterTypes.length}
          >
            {['Guidance', 'Quality standard', 'Advice'].map((type) => (
              <FilterOption
                isSelected={filterTypes.includes(type)}
                key={type}
                onChanged={(event: FormEvent<HTMLInputElement>) =>
                  toggleFilterType(type, event.currentTarget.checked)
                }
                value={type.toLowerCase().replaceAll(' ', '-')}
              >
                {type}
              </FilterOption>
            ))}
          </FilterGroup>
          <FilterByInput
            buttonLabel="Filter results"
            inputProps={{
              hint: 'For example, NG123',
              onChange: (event: FormEvent<HTMLInputElement>) =>
                setReference(event.currentTarget.value),
              value: reference,
            }}
            label="Guidance reference"
            name="showcase-guidance-reference"
            type="search"
          />
        </FilterPanel>
      </Example>
      <Example title="Summary, active filters and sorting">
        <FilterSummary
          activeFilters={activeFilters}
          headingLevel={3}
          sorting={[
            {
              active: sortOrder === 'relevance',
              label: 'Relevance',
              onSelected: () => setSortOrder('relevance'),
              value: 'relevance',
            },
            {
              active: sortOrder === 'newest',
              label: 'Newest',
              onSelected: () => setSortOrder('newest'),
              value: 'newest',
            },
          ]}
        >
          Showing 24 results
        </FilterSummary>
        <p aria-live="polite">
          {filterTypes.length
            ? `Filtering by ${filterTypes.join(', ')}. Sorted by ${sortOrder}.`
            : `No content type filters selected. Sorted by ${sortOrder}.`}
        </p>
      </Example>
    </>
  )
}
