import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { UserInformationDto } from '@/client/generated'
import { fakeUserInformationDto } from '@/client/generated/@faker-js/faker.gen'

import { UserDetails } from './UserDetails'

const expectDefinitionValue = (labelText: string, expectedValue: string) => {
  const label = screen.getByText(labelText)
  const value = label.nextElementSibling
  expect(value?.textContent).toBe(expectedValue)
}

const getLinkByContent = (container: HTMLElement, content: string) => {
  return Array.from(container.querySelectorAll('a')).find(
    (element) => element.textContent?.trim() === content,
  )
}

describe('UserDetails', () => {
  it('renders an error when the current user cannot be retrieved', () => {
    render(<UserDetails currentUser={undefined} />)
    expect(screen.getByTestId('failed-user-retrieval')).toBeDefined()
  })

  it("renders the current user's details", () => {
    const currentUser: UserInformationDto = fakeUserInformationDto()

    render(<UserDetails currentUser={currentUser} />)

    expectDefinitionValue('Organisation', currentUser.organisationName)
    expectDefinitionValue('Full name', currentUser.fullName)
    expectDefinitionValue('Email address', currentUser.workEmail)
    expectDefinitionValue('Contact Number', currentUser.workTelephone)
  })

  it('renders the edit details link', () => {
    const currentUser = fakeUserInformationDto()

    const { container } = render(<UserDetails currentUser={currentUser} />)

    const link = getLinkByContent(container, 'Edit Details')

    expect(link).toBeDefined()
    expect(link!.getAttribute('href')).toBe('/portal/user/me/edit-details')
  })

  it('renders the return to view and manage records link', () => {
    const currentUser = fakeUserInformationDto()

    const { container } = render(<UserDetails currentUser={currentUser} />)

    const link = getLinkByContent(container, 'Return to view and manage records')

    expect(link).toBeDefined()
    expect(link!.getAttribute('href')).toBe('/placeholder')
  })
})
