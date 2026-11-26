import { UserInformationDto } from '@/client/generated'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import {
  SummaryList,
  SummaryListAction,
  SummaryListRow,
} from '@/components/SummaryList/SummaryList'
import { errorMessages } from '@/lib/form/errorMessages'

type UserAuthenticationDetailsProps = {
  currentUser: UserInformationDto | undefined
  actions?: {
    changePassword?: string
    sendRecoveryLink?: {
      primary?: string
      secondary?: string
    }
    changeMfa?: {
      primary?: string
      secondary?: string
    }
  }
}
export const UserAuthenticationDetails = ({
  currentUser,
  actions,
}: UserAuthenticationDetailsProps) => {
  if (!currentUser)
    return (
      <ErrorState data-testid="failed-user-retrieval">
        {errorMessages.failedToRetrieveCurrentUser}
      </ErrorState>
    )

  const mfaMethodLabel =
    currentUser.mfaMethod === 'Sms'
      ? 'Text message'
      : currentUser.mfaMethod === 'AuthenticatorApp'
        ? 'Authenticator app'
        : currentUser.mfaMethod
  const mfaValue =
    currentUser.mfaMethod === 'Sms' && currentUser.mfaPhoneNumber
      ? `${mfaMethodLabel} (${currentUser.mfaPhoneNumber})`
      : mfaMethodLabel

  return (
    <div>
      <h2>Sign in method</h2>
      <SummaryList>
        <SummaryListRow label="Password" value="*************">
          {actions?.changePassword !== undefined && (
            <SummaryListAction href={actions?.changePassword} visuallyHiddenText="password">
              Change
            </SummaryListAction>
          )}
        </SummaryListRow>
        <SummaryListRow label="Primary authentication method" value={mfaValue}>
          {actions?.sendRecoveryLink?.primary !== undefined && (
            <SummaryListAction
              href={actions.sendRecoveryLink.primary}
              visuallyHiddenText="primary-recovery-link"
            >
              Send recovery link
            </SummaryListAction>
          )}
          {actions?.changeMfa?.primary !== undefined && (
            <SummaryListAction
              href={actions.changeMfa.primary}
              visuallyHiddenText="update-primary-method"
            >
              Change
            </SummaryListAction>
          )}
        </SummaryListRow>
        <SummaryListRow label="Backup authentication method" value="Placeholder">
          {actions?.sendRecoveryLink?.secondary !== undefined && (
            <SummaryListAction
              href={actions.sendRecoveryLink.secondary}
              visuallyHiddenText="backup-recovery-link"
            >
              Send recovery link
            </SummaryListAction>
          )}
          {actions?.changeMfa?.secondary !== undefined && (
            <SummaryListAction
              href={actions.changeMfa.secondary}
              visuallyHiddenText="update-backup-method"
            >
              Change
            </SummaryListAction>
          )}
        </SummaryListRow>
      </SummaryList>
    </div>
  )
}
