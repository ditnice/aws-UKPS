import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { fakeUserInformationDto } from '@/client/generated/@faker-js/faker.gen'

import { UserAuthenticationDetails } from './UserAuthenticationDetails'

const user = fakeUserInformationDto()

const actions = {
  changePassword: '/change-password',
  sendRecoveryLink: {
    primary: '/send-primary-recovery-link',
    secondary: '/send-backup-recovery-link',
  },
  changeMfa: {
    primary: '/change-primary-mfa',
    secondary: '/change-backup-mfa',
  },
}

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

  it('renders no actions when none are provided', () => {
    render(<UserAuthenticationDetails currentUser={user} />)

    expect(screen.queryAllByRole('link')).toHaveLength(0)
  })

  it('renders each action with its provided href', () => {
    render(<UserAuthenticationDetails currentUser={user} actions={actions} />)

    expect(screen.getByRole('link', { name: 'Change password' })).toHaveAttribute(
      'href',
      actions.changePassword,
    )
    expect(
      screen.getByRole('link', { name: 'Send recovery link primary-recovery-link' }),
    ).toHaveAttribute('href', actions.sendRecoveryLink.primary)
    expect(screen.getByRole('link', { name: 'Change update-primary-method' })).toHaveAttribute(
      'href',
      actions.changeMfa.primary,
    )
    expect(
      screen.getByRole('link', { name: 'Send recovery link backup-recovery-link' }),
    ).toHaveAttribute('href', actions.sendRecoveryLink.secondary)
    expect(screen.getByRole('link', { name: 'Change update-backup-method' })).toHaveAttribute(
      'href',
      actions.changeMfa.secondary,
    )
  })

  it('only renders the actions that are provided', () => {
    render(
      <UserAuthenticationDetails
        currentUser={user}
        actions={{ changeMfa: { primary: actions.changeMfa.primary } }}
      />,
    )

    expect(screen.getAllByRole('link')).toHaveLength(1)
    expect(screen.getByRole('link', { name: 'Change update-primary-method' })).toHaveAttribute(
      'href',
      actions.changeMfa.primary,
    )
  })
})
