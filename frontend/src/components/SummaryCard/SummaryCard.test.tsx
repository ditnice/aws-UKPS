import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import {
  SummaryList,
  SummaryListAction,
  SummaryListRow,
} from '@/components/SummaryList/SummaryList'

import { SummaryCard } from './SummaryCard'

describe('SummaryCard', () => {
  it('renders the title as a level 2 heading above the children', () => {
    render(
      <SummaryCard title="Names and identifiers">
        <SummaryList>
          <SummaryListRow label="Company code" value="GC22" />
        </SummaryList>
      </SummaryCard>,
    )

    const heading = screen.getByRole('heading', { level: 2, name: 'Names and identifiers' })
    const label = screen.getByText('Company code')
    expect(heading.compareDocumentPosition(label) & Node.DOCUMENT_POSITION_FOLLOWING).toBeTruthy()
  })

  it('renders the title at the given heading level', () => {
    render(
      <SummaryCard title="Trial 1" titleHeadingLevel={3}>
        <p>Study name</p>
      </SummaryCard>,
    )

    expect(screen.getByRole('heading', { level: 3, name: 'Trial 1' })).toBeInTheDocument()
  })

  it('renders actions as links on the title line with hidden context', () => {
    const { container } = render(
      <SummaryCard
        actions={
          <SummaryListAction href="#edit-section" visuallyHiddenText="names and identifiers">
            Edit section
          </SummaryListAction>
        }
        title="Names and identifiers"
      >
        <p>Not yet provided</p>
      </SummaryCard>,
    )

    const heading = screen.getByRole('heading', { name: 'Names and identifiers' })
    const link = screen.getByRole('link', { name: 'Edit section names and identifiers' })
    expect(link).toHaveAttribute('href', '#edit-section')
    expect(heading.parentElement).toContainElement(link)
    expect(container.querySelector('[class*="actions"]')).not.toBeNull()
  })

  it('does not render an actions area when there are no actions', () => {
    const { container } = render(
      <SummaryCard title="Pricing and budget impact">
        <p>Not yet provided</p>
      </SummaryCard>,
    )

    expect(screen.getByText('Not yet provided')).toBeInTheDocument()
    expect(container.querySelector('[class*="actions"]')).toBeNull()
  })
})
