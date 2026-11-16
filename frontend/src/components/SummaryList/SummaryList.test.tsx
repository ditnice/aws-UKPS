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

  it('draws the title and rows inside a panel when card is set', () => {
    const { asFragment, container } = render(
      <SummaryList card title="Names and identifiers">
        <SummaryListRow label="Company code" value="GC22" />
      </SummaryList>,
    )

    const panel = container.querySelector('.panel')
    expect(panel).not.toBeNull()
    expect(
      within(panel as HTMLElement).getByRole('heading', { name: 'Names and identifiers' }),
    ).toBeInTheDocument()
    expect(panel?.querySelector('dl')).not.toBeNull()
    expect(asFragment()).toMatchSnapshot()
  })

  it('does not render a panel unless card is set', () => {
    const { container } = render(
      <SummaryList title="Names and identifiers">
        <SummaryListRow label="Company code" value="GC22" />
      </SummaryList>,
    )

    expect(container.querySelector('.panel')).toBeNull()
  })

  it('renders actions on the title line as links with hidden context', () => {
    render(
      <SummaryList
        actions={
          <SummaryListAction href="#edit-section" visuallyHiddenText="names and identifiers">
            Edit section
          </SummaryListAction>
        }
        title="Names and identifiers"
      >
        <SummaryListRow label="Company code" value="GC22" />
      </SummaryList>,
    )

    const heading = screen.getByRole('heading', { level: 4, name: 'Names and identifiers' })
    const link = screen.getByRole('link', { name: 'Edit section names and identifiers' })
    expect(link).toHaveAttribute('href', '#edit-section')
    expect(heading.parentElement).toContainElement(link)
  })

  it('renders intro text without a list when there are no rows', () => {
    const { container } = render(
      <SummaryList card intro={<p>Not yet provided</p>} title="Pricing and budget impact" />,
    )

    expect(screen.getByText('Not yet provided')).toBeInTheDocument()
    expect(container.querySelector('dl')).toBeNull()
  })

  it('renders intro above the rows', () => {
    const { container } = render(
      <SummaryList
        intro={
          <p>This task becomes mandatory within 12 months of the market authorisation date.</p>
        }
        title="Laboratory testing (optional)"
      >
        <SummaryListRow label="Does the product require a specific biomarker?" value="No" />
      </SummaryList>,
    )

    const intro = screen.getByText(/mandatory within 12 months/)
    const list = container.querySelector('dl') as HTMLElement
    expect(intro.compareDocumentPosition(list) & Node.DOCUMENT_POSITION_FOLLOWING).toBeTruthy()
  })

  it('renders a row tag in the value cell', () => {
    const { container } = render(
      <SummaryList>
        <SummaryListRow label="Record title" tag={<span>Hidden</span>} value="Early RA" />
      </SummaryList>,
    )

    const valueCell = container.querySelector('dd') as HTMLElement
    expect(within(valueCell).getByText('Hidden')).toBeInTheDocument()
    expect(within(valueCell).getByText('Early RA')).toBeInTheDocument()
  })
})
