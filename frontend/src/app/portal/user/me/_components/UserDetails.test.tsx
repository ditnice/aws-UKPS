import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { fakeUserInformationDto } from '@/client/generated/@faker-js/faker.gen'

import { UserDetails } from './UserDetails'

const user = fakeUserInformationDto()

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

  it('renders the edit details link', () => {
    const currentUser = { ...user }

    const { container } = render(<UserDetails currentUser={currentUser} />)

    const link = getLinkByContent(container, 'Edit Details')

    expect(link).toBeInTheDocument()
    expect(link!.getAttribute('href')).toBe('/portal/user/me/edit-details')
  })
})
