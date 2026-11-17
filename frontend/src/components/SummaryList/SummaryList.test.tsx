import { render, screen, within } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { SummaryList, SummaryListAction, SummaryListRow } from './SummaryList'

describe('SummaryList', () => {
  it('renders a title as a heading above the list', () => {
    const { asFragment, container } = render(
      <SummaryList title="Indication details">
        <SummaryListRow label="Indication" value="Early rheumatoid arthritis" />
      </SummaryList>,
    )

    const heading = screen.getByRole('heading', { level: 4, name: 'Indication details' })
    expect(heading.nextElementSibling).toBe(container.querySelector('dl'))
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders the title at the given heading level', () => {
    render(
      <SummaryList title="Indication details" titleHeadingLevel={3}>
        <SummaryListRow label="Indication" value="Early rheumatoid arthritis" />
      </SummaryList>,
    )

    expect(screen.getByRole('heading', { level: 3, name: 'Indication details' })).toBeDefined()
  })

  it('renders row children as a description list and ignores empty children', () => {
    const showOptionalRow = false
    const { asFragment, container } = render(
      <SummaryList className="additional-class">
        <SummaryListRow label="Name" value="Sarah Philips" />
        {showOptionalRow && <SummaryListRow label="Status" value="Active" />}
        {null}
        <SummaryListRow label="Date of birth" value="5 January 1978" />
      </SummaryList>,
    )

    const list = container.querySelector('dl')
    expect(list).not.toBeNull()
    expect(list?.classList.contains('additional-class')).toBe(true)
    expect(Array.from(list?.querySelectorAll('dt') ?? []).map((key) => key.textContent)).toEqual([
      'Name',
      'Date of birth',
    ])
    expect(
      Array.from(list?.querySelectorAll('dd') ?? []).map((value) => value.textContent),
    ).toEqual(['Sarah Philips', '5 January 1978'])
    expect(asFragment()).toMatchSnapshot()
  })

  it('adds hidden context to a row action child', () => {
    const { asFragment } = render(
      <SummaryList>
        <SummaryListRow label="Name" value="Sarah Philips">
          <SummaryListAction href="/change-name" visuallyHiddenText="name">
            Change
          </SummaryListAction>
        </SummaryListRow>
      </SummaryList>,
    )

    const action = screen.getByRole('link', { name: 'Change name' })
    expect(action.getAttribute('href')).toBe('/change-name')
    expect(action.querySelector('.visually-hidden')?.textContent).toBe('name')
    expect(asFragment()).toMatchSnapshot()
  })

  it('supports mixed rows, multiple action children and rich values', () => {
    const { asFragment, container } = render(
      <SummaryList>
        <SummaryListRow label="Name" value="Sarah Philips" />
        <SummaryListRow
          label="Contact details"
          value={
            <>
              <p>07700 900457</p>
              <p>sarah.philips@example.com</p>
            </>
          }
        >
          <SummaryListAction href="/add-contact" visuallyHiddenText="contact details">
            Add
          </SummaryListAction>
          <SummaryListAction href="/change-contact" visuallyHiddenText="contact details">
            Change
          </SummaryListAction>
        </SummaryListRow>
      </SummaryList>,
    )

    expect(screen.getByRole('link', { name: 'Add contact details' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Change contact details' })).toBeInTheDocument()
    expect(container.querySelectorAll('dd').length).toBe(3)

    const actionsList = container.querySelector('ul')
    expect(actionsList).not.toBeNull()
    expect(within(actionsList as HTMLElement).getAllByRole('listitem').length).toBe(2)
    expect(screen.getByText('sarah.philips@example.com').tagName).toBe('P')
    expect(asFragment()).toMatchSnapshot()
  })

  it('supports a two-column variant for lists without actions', () => {
    const { asFragment, container } = render(
      <SummaryList variant="two-column">
        <SummaryListRow label="Name" value="Sarah Philips" />
        <SummaryListRow label="Status" value="Active" />
      </SummaryList>,
    )

    const list = container.querySelector('dl')
    expect(list?.className).toContain('summary-list--two-column')
    expect(container.querySelector('[class*="summary-list__row--no-actions"]')).toBeNull()
    expect(asFragment()).toMatchSnapshot()
  })

  it('supports a wide-value variant', () => {
    const { asFragment, container } = render(
      <SummaryList variant="wide-value">
        <SummaryListRow label="Email" value="sarah@example.com" />
        <SummaryListRow label="Name" value="Sarah Philips">
          <SummaryListAction href="/change-name" visuallyHiddenText="name">
            Change
          </SummaryListAction>
        </SummaryListRow>
      </SummaryList>,
    )

    expect(container.querySelector('dl')?.className).toContain('summary-list--wide-value')
    expect(container.querySelector('[class*="summary-list__row--no-actions"]')).not.toBeNull()
    expect(asFragment()).toMatchSnapshot()
  })

  it('renders a row tag in the value cell', () => {
    const { asFragment, container } = render(
      <SummaryList>
        <SummaryListRow label="Record title" tag={<span>Hidden</span>} value="Early RA" />
      </SummaryList>,
    )

    const valueCell = container.querySelector('dd') as HTMLElement
    expect(within(valueCell).getByText('Hidden')).toBeInTheDocument()
    expect(within(valueCell).getByText('Early RA')).toBeInTheDocument()
    expect(asFragment()).toMatchSnapshot()
  })

  it('wraps a tagged row value so it keeps the value content styles', () => {
    render(
      <SummaryList>
        <SummaryListRow label="Notes" tag={<span>Hidden</span>} value={<p>First note</p>} />
      </SummaryList>,
    )

    const paragraph = screen.getByText('First note')
    expect(paragraph.parentElement?.className).toContain('summary-list__value-content')
  })
})
