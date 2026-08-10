'use client'

import { Select, SelectOption } from '@/components/Select/Select'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <>
      <Example title="Overview">
        <Select defaultValue="updated" label="Sort by" name="sort-overview">
          <SelectOption value="published">Recently published</SelectOption>
          <SelectOption value="updated">Recently updated</SelectOption>
          <SelectOption value="views">Most views</SelectOption>
          <SelectOption value="comments">Most comments</SelectOption>
        </Select>
      </Example>
      <Example title="Example: standard select">
        <Select defaultValue="updated" label="Sort by" name="sort">
          <SelectOption value="published">Recently published</SelectOption>
          <SelectOption value="updated">Recently updated</SelectOption>
          <SelectOption value="views">Most views</SelectOption>
          <SelectOption value="comments">Most comments</SelectOption>
        </Select>
      </Example>
      <Example title="Example: select with hint">
        <Select
          defaultValue="choose"
          hint="This can be different to where you went before"
          label="Choose location"
          name="location-hint"
        >
          <SelectOption value="choose">Choose location</SelectOption>
          <SelectOption value="eastmidlands">East Midlands</SelectOption>
          <SelectOption value="eastofengland">East of England</SelectOption>
          <SelectOption value="london">London</SelectOption>
          <SelectOption value="northeast">North East</SelectOption>
          <SelectOption value="northwest">North West</SelectOption>
          <SelectOption value="southeast">South East</SelectOption>
          <SelectOption value="southwest">South West</SelectOption>
          <SelectOption value="westmidlands">West Midlands</SelectOption>
          <SelectOption value="yorkshire">Yorkshire and the Humber</SelectOption>
        </Select>
      </Example>
      <Example title="Example: select with error">
        <Select
          defaultValue="choose"
          error
          errorMessage="Select a location"
          hint="This can be different to where you went before"
          label="Choose location"
          name="location-error"
        >
          <SelectOption value="choose">Choose location</SelectOption>
          <SelectOption value="eastmidlands">East Midlands</SelectOption>
          <SelectOption value="eastofengland">East of England</SelectOption>
          <SelectOption value="london">London</SelectOption>
          <SelectOption value="northeast">North East</SelectOption>
          <SelectOption value="northwest">North West</SelectOption>
          <SelectOption value="southeast">South East</SelectOption>
          <SelectOption value="southwest">South West</SelectOption>
          <SelectOption value="westmidlands">West Midlands</SelectOption>
          <SelectOption value="yorkshire">Yorkshire and the Humber</SelectOption>
        </Select>
      </Example>
      <Example title="Example: fluid width">
        <Select label="Full width" name="sort-width-full" width="full">
          <SelectOption value="published">Recently published</SelectOption>
          <SelectOption value="updated">Recently updated</SelectOption>
        </Select>
        <Select
          label="Three-quarters width"
          name="sort-width-three-quarters"
          width="three-quarters"
        >
          <SelectOption value="published">Recently published</SelectOption>
          <SelectOption value="updated">Recently updated</SelectOption>
        </Select>
        <Select label="Two-thirds width" name="sort-width-two-thirds" width="two-thirds">
          <SelectOption value="published">Recently published</SelectOption>
          <SelectOption value="updated">Recently updated</SelectOption>
        </Select>
        <Select label="One-half width" name="sort-width-one-half" width="one-half">
          <SelectOption value="published">Recently published</SelectOption>
          <SelectOption value="updated">Recently updated</SelectOption>
        </Select>
        <Select label="One-third width" name="sort-width-one-third" width="one-third">
          <SelectOption value="published">Recently published</SelectOption>
          <SelectOption value="updated">Recently updated</SelectOption>
        </Select>
        <Select label="One-quarter width" name="sort-width-one-quarter" width="one-quarter">
          <SelectOption value="published">Recently published</SelectOption>
          <SelectOption value="updated">Recently updated</SelectOption>
        </Select>
      </Example>
    </>
  )
}
