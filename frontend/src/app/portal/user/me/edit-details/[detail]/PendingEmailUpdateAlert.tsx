'use server'

import { PendingWorkEmailChangeDto } from '@/client/generated'
import { Alert } from '@/components/Alert/Alert'
import { ErrorState } from '@/components/Placeholder/ErrorState'

import CancelPendingEmailChange from './editUserEmail/CancelPendingEmailChange'
import styles from './PendingEmailUpdateAlert.module.scss'
import ResendVerificationEmailButton from './ResendVerificationEmail/ResendVerificationEmailButton'

export type PendingEmailUpdateAlertProps = PendingWorkEmailChangeDto & {
  verificationEmailResent: true | undefined
  error: true | undefined
  userId: number
}

const PendingEmailUpdateAlert = async ({
  value,
  verifiedByEmail,
  userId,
  verificationEmailResent,
  error,
}: PendingEmailUpdateAlertProps) => {
  return (
    <Alert type="caution" title="Email update pending">
      <h2>Email update pending</h2>
      <p>
        A request has been made to change your work email address to{' '}
        <strong data-testid="target-email">{value}</strong>. This change has not been completed yet.
      </p>
      {verifiedByEmail ? (
        <>
          <p>
            This address has been verified. To complete the change, log in using your new email
            address.
          </p>
          <div className={styles.buttons}>
            <CancelPendingEmailChange userId={userId} />
          </div>
        </>
      ) : (
        <>
          <p>
            We&rsquo;ve sent a verification email to{' '}
            <strong data-testid="target-email-not-verified">{value}</strong>. Check your inbox and
            use the link in the email to verify this address. Once it&rsquo;s verified, log in with
            it to complete the change.
          </p>
          <div className={styles.buttons}>
            <ResendVerificationEmailButton userId={userId} />
            <CancelPendingEmailChange userId={userId} />
          </div>
          {verificationEmailResent ? (
            <p role="status">Verification email sent. Check your inbox.</p>
          ) : undefined}
          {error ? <ErrorState>An unexpected error occurred.</ErrorState> : undefined}
        </>
      )}
    </Alert>
  )
}

export default PendingEmailUpdateAlert
