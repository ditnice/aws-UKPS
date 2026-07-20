'use client'

import {
  FilterByInput,
  FilterGroup,
  FilterOption,
  FilterPanel,
  FilterSummary,
} from '@nice-digital/nds-filters'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <>
      <Example title="Filter summary">
        <FilterSummary
          activeFilters={[
            {
              label: 'Guidance',
            },
            {
              label: 'Pathway',
            },
          ]}
        >
          Showing results 1 to 10 of 1209
        </FilterSummary>
      </Example>

      <Example title="Filter panel with filter group">
        <FilterPanel heading="A filter panel">
          <FilterGroup heading="Type" id="filter-panel-type">
            <FilterOption
              isSelected={true}
              value="This is an example"
              onChanged={() => console.log('Changed!')}
            >
              This is an example
            </FilterOption>
            <FilterOption
              isSelected={false}
              value="Another example"
              onChanged={() => console.log('Changed!')}
            >
              Another example
            </FilterOption>
          </FilterGroup>
        </FilterPanel>
      </Example>

      <Example title="Filter panel with filter by input">
        <FilterPanel heading="A filter panel">
          <FilterByInput label="test" name="filter-input-test"></FilterByInput>
        </FilterPanel>
      </Example>

      <Example title="Filter panel with multiple filters">
        <FilterPanel heading="A filter panel">
          <FilterGroup heading="Type" id="multiple-filters-type">
            <FilterOption
              isSelected={true}
              value="This is an example"
              onChanged={() => console.log('Changed!')}
            >
              This is an example
            </FilterOption>
            <FilterOption
              isSelected={false}
              value="Another example"
              onChanged={() => console.log('Changed!')}
            >
              Another example
            </FilterOption>
          </FilterGroup>
          <FilterByInput label="test" name="multiple-filters-test"></FilterByInput>
        </FilterPanel>
      </Example>
    </>
  )
}
