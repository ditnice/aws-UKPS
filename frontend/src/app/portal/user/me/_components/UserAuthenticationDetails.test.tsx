import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { fakeUserInformationDto } from '@/client/generated/@faker-js/faker.gen'

import { UserAuthenticationDetails } from './UserAuthenticationDetails'

const user = fakeUserInformationDto()

describe('UserAuthenticationDetails', () => {
  it('renders an error when the current user cannot be retrieved', () => {
    render(<UserAuthenticationDetails currentUser={undefined} />)

    expect(screen.getByTestId('failed-user-retrieval')).toBeInTheDocument()
  })

  it('renders the sign in method rows', () => {
    render(<UserAuthenticationDetails currentUser={user} />)

    expect(screen.getByRole('heading', { level: 2, name: 'Sign in method' })).toBeInTheDocument()
    expect(screen.getByText('Password')).toBeInTheDocument()
    expect(screen.getByText('Primary authentication method')).toBeInTheDocument()
    expect(screen.getByText('Backup authentication method')).toBeInTheDocument()
  })

  it('renders the sign in method actions', () => {
    render(<UserAuthenticationDetails currentUser={user} />)

    expect(screen.getAllByRole('link', { name: /^Change / })).toHaveLength(3)
    expect(screen.getAllByRole('link', { name: /^Send recovery link / })).toHaveLength(2)
    expect(screen.getAllByRole('link', { name: /^Make primary method / })).toHaveLength(1)
  })
})
