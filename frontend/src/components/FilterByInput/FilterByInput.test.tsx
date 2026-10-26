import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'

import { FilterPanel } from '@nice-digital/nds-filters'

import { FilterByInput } from './FilterByInput'

describe('FilterByInput', () => {
  it('renders the label, input and submit button', () => {
    const { asFragment } = render(<FilterByInput label="Search records" name="search" />)

    expect(screen.getByRole('button', { name: 'Search records' })).toBeInTheDocument()
    expect(screen.getByLabelText('Search records')).toHaveAttribute('name', 'search')
    expect(screen.getByRole('button', { name: 'Filter' })).toHaveAttribute('type', 'submit')
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders hint text and associates it with the input', () => {
    const { asFragment } = render(
      <FilterByInput
        hint="Search by record ID"
        inputProps={{ 'aria-describedby': 'existing-description' }}
        label="Search records"
        name="search"
      />,
    )

    expect(screen.getByText('Search by record ID')).toHaveAttribute('id', 'inputFilter-search-hint')
    expect(screen.getByLabelText('Search records')).toHaveAttribute(
      'aria-describedby',
      'existing-description inputFilter-search-hint',
    )
    expect(asFragment()).toMatchSnapshot()
  })

  it('collapses and expands the controls when the heading is clicked', async () => {
    const user = userEvent.setup()
    render(<FilterByInput label="Search records" name="search" />)

    const toggle = screen.getByRole('button', { name: 'Search records' })
    const controls = document.getElementById('inputFilter-search')!

    await user.click(toggle)
    expect(toggle).toHaveAttribute('aria-expanded', 'false')
    expect(controls).toHaveAttribute('aria-hidden', 'true')

    await user.click(toggle)
    expect(toggle).toHaveAttribute('aria-expanded', 'true')
    expect(controls).toHaveAttribute('aria-hidden', 'false')
  })

  it('starts collapsed when collapseByDefault is set', () => {
    render(<FilterByInput collapseByDefault label="Search records" name="search" />)

    expect(screen.getByRole('button', { name: 'Search records' })).toHaveAttribute(
      'aria-expanded',
      'false',
    )
  })

  it('takes its heading level from FilterPanel', () => {
    render(
      <FilterPanel heading="Filters">
        <FilterByInput label="Search records" name="search" />
      </FilterPanel>,
    )

    expect(screen.getByRole('heading', { level: 3, name: 'Search records' })).toBeInTheDocument()
  })
})
