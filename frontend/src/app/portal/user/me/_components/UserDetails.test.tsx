import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { fakeUserInformationDto } from '@/client/generated/@faker-js/faker.gen'

import { buildUpdateUserDetailsHref, UserDetail } from '../edit-details/[detail]/navigation'

import { UserDetails } from './UserDetails'

const user = fakeUserInformationDto()

const expectedDetails: UserDetail[] = ['fullName', 'email', 'contactNumber']

const expectDefinitionValue = (labelText: string, expectedValue: string) => {
  const label = screen.getByText(labelText)
  const value = label.nextElementSibling
  expect(value?.textContent).toBe(expectedValue)
}

describe('UserDetails', () => {
  it('renders an error when the current user cannot be retrieved', () => {
    render(<UserDetails currentUser={undefined} />)
    expect(screen.getByTestId('failed-user-retrieval')).toBeInTheDocument()
  })

  it("renders the current user's details", () => {
    const currentUser = { ...user }

    render(<UserDetails currentUser={currentUser} />)

    expectDefinitionValue('Organisation', currentUser.organisationName)
    expectDefinitionValue('Full name', currentUser.fullName)
    expectDefinitionValue('Email address', currentUser.workEmail)
    expectDefinitionValue('Contact Number', currentUser.workTelephone)
  })

  it('renders a change link for each editable detail', () => {
    render(<UserDetails currentUser={{ ...user }} />)

    const changeLinks = screen.getAllByRole('link', { name: /change/i })

    expect(changeLinks).toHaveLength(expectedDetails.length)
  })

  it.each(expectedDetails)('links to the correct edit page for "%s"', (detail) => {
    render(<UserDetails currentUser={{ ...user }} />)

    const expectedHref = buildUpdateUserDetailsHref({ detail })
    const links = screen.getAllByRole('link', { name: /change/i })

    expect(links.map((link) => link.getAttribute('href'))).toContain(expectedHref)
  })

  it('does not render a change link for organisation', () => {
    render(<UserDetails currentUser={{ ...user }} />)

    const links = screen.getAllByRole('link', { name: /change/i })
    const hrefs = links.map((link) => link.getAttribute('href'))

    // Every link should map to one of the editable details, nothing extra
    expect(hrefs.sort()).toEqual(
      expectedDetails.map((detail) => buildUpdateUserDetailsHref({ detail })).sort(),
    )
  })

  it('renders no change links when the user could not be retrieved', () => {
    render(<UserDetails currentUser={undefined} />)

    expect(screen.getByTestId('failed-user-retrieval')).toBeInTheDocument()
    expect(screen.queryByRole('link', { name: /change/i })).not.toBeInTheDocument()
  })
})
