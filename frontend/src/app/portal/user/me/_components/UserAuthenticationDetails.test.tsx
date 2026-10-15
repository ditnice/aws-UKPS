import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import type { UserInformationDto } from '@/client/generated'

import { UserAuthenticationDetails } from './UserAuthenticationDetails'

const user = {
  userId: 4,
  fullName: 'Julie Brooks',
  workTelephone: '01234 567890',
  workEmail: 'julie.brooks@example.com',
  organisationMembershipId: 9,
  organisationId: 2,
  organisationName: 'Example Pharma',
  userRole: 'Standard',
} satisfies UserInformationDto

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
