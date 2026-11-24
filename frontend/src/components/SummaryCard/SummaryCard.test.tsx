import { render, screen, within } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import {
  SummaryList,
  SummaryListAction,
  SummaryListRow,
} from '@/components/SummaryList/SummaryList'

import { SummaryCard } from './SummaryCard'

function isBefore(first: Node, second: Node) {
  return Boolean(first.compareDocumentPosition(second) & Node.DOCUMENT_POSITION_FOLLOWING)
}

describe('SummaryCard', () => {
  it('renders the title as a level 2 heading above the children', () => {
    const { asFragment } = render(
      <SummaryCard title="Names and identifiers">
        <SummaryList>
          <SummaryListRow label="Company code" value="GC22" />
        </SummaryList>
      </SummaryCard>,
    )

    const heading = screen.getByRole('heading', { level: 2, name: 'Names and identifiers' })
    expect(isBefore(heading, screen.getByText('Company code'))).toBe(true)
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders the title at the given heading level', () => {
    render(
      <SummaryCard title="Trial 1" titleHeadingLevel={3}>
        <p>Study name</p>
      </SummaryCard>,
    )

    expect(screen.getByRole('heading', { level: 3, name: 'Trial 1' })).toBeInTheDocument()
  })

  it('renders a single action as a link between the title and the children', () => {
    const { asFragment } = render(
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
    expect(isBefore(heading, link)).toBe(true)
    expect(isBefore(link, screen.getByText('Not yet provided'))).toBe(true)
    expect(screen.queryByRole('list')).toBeNull()
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders several actions as a list', () => {
    const { asFragment } = render(
      <SummaryCard
        actions={[
          <SummaryListAction href="#edit" key="edit" visuallyHiddenText="trial 1">
            Edit
          </SummaryListAction>,
          <SummaryListAction href="#remove" key="remove" visuallyHiddenText="trial 1">
            Remove
          </SummaryListAction>,
        ]}
        title="Trial 1"
      >
        <p>Study name</p>
      </SummaryCard>,
    )

    const items = within(screen.getByRole('list')).getAllByRole('listitem')
    expect(items).toHaveLength(2)
    expect(within(items[0]).getByRole('link', { name: 'Edit trial 1' })).toBeInTheDocument()
    expect(within(items[1]).getByRole('link', { name: 'Remove trial 1' })).toBeInTheDocument()
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders a tag between the title and the actions', () => {
    render(
      <SummaryCard
        actions={
          <SummaryListAction href="#edit" visuallyHiddenText="trial 1">
            Edit
          </SummaryListAction>
        }
        tag={<span>There is a problem</span>}
        title="Trial 1"
      >
        <p>Not yet provided</p>
      </SummaryCard>,
    )

    const tag = screen.getByText('There is a problem')
    const link = screen.getByRole('link', { name: 'Edit trial 1' })
    expect(isBefore(screen.getByRole('heading', { name: 'Trial 1' }), tag)).toBe(true)
    expect(isBefore(tag, link)).toBe(true)
  })

  it('does not render any actions when there are none', () => {
    render(
      <SummaryCard title="Pricing and budget impact">
        <p>Not yet provided</p>
      </SummaryCard>,
    )

    expect(screen.getByText('Not yet provided')).toBeInTheDocument()
    expect(screen.queryByRole('link')).toBeNull()
    expect(screen.queryByRole('list')).toBeNull()
  })
})
