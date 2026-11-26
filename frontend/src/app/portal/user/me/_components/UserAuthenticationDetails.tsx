import { UserInformationDto } from '@/client/generated'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import {
  SummaryList,
  SummaryListAction,
  SummaryListRow,
} from '@/components/SummaryList/SummaryList'
import { errorMessages } from '@/lib/form/errorMessages'

import { buildUpdateUserMfaLink } from '../update-mfa/navigate'
import { buildUpdateUserSmsMfaLink } from '../update-sms-mfa/navigate'

export const UserAuthenticationDetails = ({
  currentUser,
}: {
  currentUser: UserInformationDto | undefined
}) => {
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
  const backupMfaValue = currentUser.mfaPhoneNumber
    ? `Text message (${currentUser.mfaPhoneNumber})`
    : 'Text message'

  return (
    <div>
      <h2>Sign in method</h2>
      <SummaryList>
        <SummaryListRow label="Password" value="*************">
          <SummaryListAction href="#change-password" visuallyHiddenText="password">
            Change
          </SummaryListAction>
        </SummaryListRow>
        <SummaryListRow label="Primary authentication method" value={mfaValue}>
          <SummaryListAction href="#send-recovery-link" visuallyHiddenText="primary-recovery-link">
            Send recovery link
          </SummaryListAction>
          <SummaryListAction
            href={buildUpdateUserMfaLink()}
            visuallyHiddenText="update-primary-method"
          >
            Change
          </SummaryListAction>
        </SummaryListRow>
        <SummaryListRow label="Backup authentication method" value={backupMfaValue}>
          <SummaryListAction href="#make-primary-method" visuallyHiddenText="make-primary">
            Make primary method
          </SummaryListAction>
          <SummaryListAction
            href="#send-backup-method-recovery-link"
            visuallyHiddenText="backup-recovery-link"
          >
            Send recovery link
          </SummaryListAction>
          <SummaryListAction
            href={buildUpdateUserSmsMfaLink()}
            visuallyHiddenText="update-backup-method"
          >
            Change
          </SummaryListAction>
        </SummaryListRow>
      </SummaryList>
    </div>
  )
}
