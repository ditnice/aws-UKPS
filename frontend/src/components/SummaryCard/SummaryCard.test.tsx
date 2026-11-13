import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { SummaryListAction } from '@/components/SummaryList/SummaryList'

import { SummaryCard } from './SummaryCard'

describe('SummaryCard', () => {
  it('renders the title as a level 2 heading and the children below it', () => {
    const { asFragment } = render(
      <SummaryCard title="Names and identifiers">
        <p>Not yet provided</p>
      </SummaryCard>,
    )

    expect(
      screen.getByRole('heading', { level: 2, name: 'Names and identifiers' }),
    ).toBeInTheDocument()
    expect(screen.getByText('Not yet provided')).toBeInTheDocument()
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

  it('renders header actions as links with hidden context', () => {
    render(
      <SummaryCard
        actions={
          <>
            <SummaryListAction href="/delete" visuallyHiddenText="trial 1">
              Delete
            </SummaryListAction>
            <SummaryListAction href="/edit" visuallyHiddenText="trial 1">
              Edit
            </SummaryListAction>
          </>
        }
        title="Trial 1"
      >
        <p>Study name</p>
      </SummaryCard>,
    )

    expect(screen.getByRole('link', { name: 'Delete trial 1' })).toHaveAttribute('href', '/delete')
    expect(screen.getByRole('link', { name: 'Edit trial 1' })).toHaveAttribute('href', '/edit')
  })

  it('does not render an actions area when there are no actions', () => {
    const { container } = render(
      <SummaryCard title="Names and identifiers">
        <p>Not yet provided</p>
      </SummaryCard>,
    )

    expect(container.querySelector('[class*="actions"]')).toBeNull()
  })
})
