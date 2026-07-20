import { cleanup, render, screen, within } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import { SummaryList, SummaryListAction, SummaryListRow } from './SummaryList'

afterEach(cleanup)

describe('SummaryList', () => {
  it('renders row children as a description list and ignores empty children', () => {
    const showOptionalRow = false
    const { container } = render(
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
  })

  it('adds hidden context to a row action child', () => {
    render(
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
  })

  it('supports mixed rows, multiple action children and rich values', () => {
    const { container } = render(
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

    expect(screen.getByRole('link', { name: 'Add contact details' })).toBeDefined()
    expect(screen.getByRole('link', { name: 'Change contact details' })).toBeDefined()
    expect(container.querySelectorAll('dd').length).toBe(3)

    const actionsList = container.querySelector('ul')
    expect(actionsList).not.toBeNull()
    expect(within(actionsList as HTMLElement).getAllByRole('listitem').length).toBe(2)
    expect(screen.getByText('sarah.philips@example.com').tagName).toBe('P')
  })
})
