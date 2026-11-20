'use client'

import { useState } from 'react'

import { Autocomplete } from '@/components/Autocomplete/Autocomplete'
import type { AutocompleteProps, GetSuggestions } from '@/components/Autocomplete/Autocomplete'

import { Example } from '../../_components/Example'

const companyCodes = [
  'ABC-123',
  'ABC-123-XR',
  'AZD1222',
  'BNT162b2',
  'GS-7977',
  'LY3298176',
  'mRNA-1273',
  'MK-3475',
  'NVX-CoV2373',
  'V116',
]

/** Stands in for a request to the server, matching anywhere in the code, ignoring case. */
const getCompanyCodes: GetSuggestions = async (query) => {
  const lowered = query.toLowerCase()
  return companyCodes
    .filter((code) => code.toLowerCase().includes(lowered))
    .sort(
      (a, b) =>
        Number(!a.toLowerCase().startsWith(lowered)) -
          Number(!b.toLowerCase().startsWith(lowered)) || a.localeCompare(b),
    )
}

const noSuggestions: GetSuggestions = async () => []

type ExampleAutocompleteProps = Omit<AutocompleteProps, 'onChange' | 'value' | 'getSuggestions'> & {
  getSuggestions?: GetSuggestions
  initialValue?: string
}

function ExampleAutocomplete({
  getSuggestions = getCompanyCodes,
  initialValue = '',
  ...props
}: ExampleAutocompleteProps) {
  const [value, setValue] = useState(initialValue)
  return (
    <Autocomplete {...props} getSuggestions={getSuggestions} onChange={setValue} value={value} />
  )
}

export function Examples() {
  return (
    <>
      <Example title="Default">
        <p>Type &ldquo;abc&rdquo;, &ldquo;mk&rdquo; or &ldquo;v&rdquo; to see suggestions.</p>
        <ExampleAutocomplete label="Company code" name="autocomplete-default" />
      </Example>
      <Example title="With a hint">
        <ExampleAutocomplete
          hint="Enter the primary internal code or working name your company uses for this product seeking market access."
          label="Company code"
          name="autocomplete-hint"
          width="two-thirds"
        />
      </Example>
      <Example title="With an error">
        <ExampleAutocomplete
          error
          errorMessage="Enter company code"
          label="Company code"
          name="autocomplete-error"
          width="two-thirds"
        />
      </Example>
      <Example title="No matching suggestions">
        <p>Any value can be entered when nothing matches.</p>
        <ExampleAutocomplete
          getSuggestions={noSuggestions}
          initialValue="NEW-001"
          label="Company code"
          name="autocomplete-no-results"
          width="one-half"
        />
      </Example>
      <Example title="At least 2 characters before suggesting">
        <ExampleAutocomplete
          label="Company code"
          minQueryLength={2}
          name="autocomplete-min-length"
          width="one-half"
        />
      </Example>
    </>
  )
}
