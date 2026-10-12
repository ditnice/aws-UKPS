'use client'

import { useState } from 'react'

import { MultiSelect } from '@/components/MultiSelect/MultiSelect'
import type { MultiSelectOption, MultiSelectProps } from '@/components/MultiSelect/MultiSelect'

import { Example } from '../../_components/Example'

const locations: readonly MultiSelectOption[] = [
  { value: 'eastmidlands', label: 'East Midlands' },
  { value: 'eastofengland', label: 'East of England' },
  { value: 'london', label: 'London' },
  { value: 'northeast', label: 'North East' },
  { value: 'northwest', label: 'North West' },
  { value: 'southeast', label: 'South East' },
  { value: 'southwest', label: 'South West' },
  { value: 'westmidlands', label: 'West Midlands' },
  { value: 'yorkshire', label: 'Yorkshire and the Humber' },
]

const locationsWithDisabledOption = locations.map((option) => ({
  ...option,
  disabled: option.value === 'london',
}))

type MultiSelectExampleProps = Omit<MultiSelectProps, 'value' | 'onChange' | 'options'> & {
  initialValue?: readonly string[]
  options?: readonly MultiSelectOption[]
}

function MultiSelectExample({
  initialValue = [],
  options = locations,
  ...props
}: MultiSelectExampleProps) {
  const [value, setValue] = useState<readonly string[]>(initialValue)
  return <MultiSelect {...props} onChange={setValue} options={options} value={value} />
}

export function Examples() {
  return (
    <>
      <Example title="Overview">
        <MultiSelectExample label="Choose locations" name="locations-overview" />
      </Example>
      <Example title="Example: multi-select with hint and selection limit">
        <MultiSelectExample
          hint="Select up to 3 locations. You can answer this question later."
          label="Locations (optional)"
          maxSelections={3}
          name="locations-limit"
          placeholder="Choose or search a location"
        />
      </Example>
      <Example title="Example: preselected options">
        <MultiSelectExample
          initialValue={['eastmidlands', 'london']}
          label="Selected locations"
          name="locations-preselected"
        />
      </Example>
      <Example title="Example: multi-select with error">
        <MultiSelectExample
          error
          errorMessage="Select no more than 3 locations"
          hint="Remove a location before choosing another."
          initialValue={['eastmidlands', 'eastofengland', 'london', 'northeast']}
          label="Locations with an error"
          maxSelections={3}
          name="locations-error"
        />
      </Example>
      <Example title="Example: disabled multi-select">
        <MultiSelectExample
          disabled
          initialValue={['london']}
          label="Disabled locations"
          name="locations-disabled"
        />
      </Example>
      <Example title="Example: disabled option">
        <MultiSelectExample
          hint="London is currently unavailable."
          label="Available locations"
          name="locations-disabled-option"
          options={locationsWithDisabledOption}
        />
      </Example>
      <Example title="Example: fixed width">
        <MultiSelectExample label="20 characters" name="locations-width-20" width={20} />
      </Example>
      <Example title="Example: fluid width">
        <MultiSelectExample label="Full width" name="locations-width-full" width="full" />
        <MultiSelectExample
          label="Two-thirds width"
          name="locations-width-two-thirds"
          width="two-thirds"
        />
        <MultiSelectExample label="One-half width" name="locations-width-half" width="one-half" />
      </Example>
    </>
  )
}
