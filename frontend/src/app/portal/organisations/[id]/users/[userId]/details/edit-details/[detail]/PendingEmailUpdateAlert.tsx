'use server'

import { PendingWorkEmailChangeDto } from '@/client/generated'
import { Alert } from '@/components/Alert/Alert'
import { ErrorState } from '@/components/Placeholder/ErrorState'

import CancelPendingEmailChange from './editUserEmail/CancelPendingEmailChange'
import { buildUpdateUserDetailsHref, getStaticProps, TypedPageProps } from './navigation'
import styles from './PendingEmailUpdateAlert.module.scss'
import ResendVerificationEmailButton from './ResendVerificationEmail/ResendVerificationEmailButton'

type EmailPageProps = Extract<TypedPageProps, { detail: 'email' }>

export type PendingEmailUpdateAlertProps = PendingWorkEmailChangeDto & EmailPageProps

const PendingEmailUpdateAlert = async ({
  value,
  verifiedByEmail,
  ...props
}: PendingEmailUpdateAlertProps) => {
  const { userId } = props

  const resendButton = (
    <ResendVerificationEmailButton
      userId={userId}
      navigation={{
        onSuccess: buildUpdateUserDetailsHref({
          ...getStaticProps(props),
          verificationLinkResent: true,
        }),
        onError: buildUpdateUserDetailsHref({ ...getStaticProps(props), error: true }),
      }}
    />
  )

  const cancelButton = (
    <CancelPendingEmailChange userId={userId} onSuccessRedirect={props.returnLink} />
  )

  const buttons = verifiedByEmail ? (
    <div className={styles.buttons}>{cancelButton}</div>
  ) : (
    <div className={styles.buttons}>
      {resendButton}
      {cancelButton}
    </div>
  )
  const messages = (
    <>
      {props.verificationLinkResent ? (
        <p role="status">Verification email resent to {value}.</p>
      ) : undefined}
      {props.error ? <ErrorState>An unexpected error occurred.</ErrorState> : undefined}
    </>
  )

  const buttonsWithMessage = (
    <>
      {buttons}
      {messages}
    </>
  )

  if (props.owner === 'me') {
    return (
      <Alert type="caution" title="Email update pending">
        <h3>Email update pending</h3>
        <p>
          A request has been made to change your work email address to{' '}
          <strong data-testid="target-email">{value}</strong>. This change has not been completed
          yet.
        </p>
        {verifiedByEmail ? (
          <>
            <p>
              This address has been verified. To complete the change, log in using your new email
              address.
            </p>
            {buttonsWithMessage}
          </>
        ) : (
          <>
            <p>
              We&rsquo;ve sent a verification email to{' '}
              <strong data-testid="target-email-not-verified">{value}</strong>. Check your inbox and
              use the link in the email to verify this address. Once it&rsquo;s verified, log in
              with it to complete the change.
            </p>
            {buttonsWithMessage}
          </>
        )}
      </Alert>
    )
  }

  return (
    <Alert type="caution" title="Email update pending">
      <h3>Email update pending</h3>
      <p>
        A request has been made to change this user&rsquo;s work email address to{' '}
        <strong data-testid="target-email">{value}</strong>. This change has not been completed yet.
      </p>
      {verifiedByEmail ? (
        <>
          <p>
            This address has been verified. The change will be completed the next time the user logs
            in using their new email address.
          </p>
          {buttonsWithMessage}
        </>
      ) : (
        <>
          <p>
            A verification email has been sent to{' '}
            <strong data-testid="target-email-not-verified">{value}</strong>. The user needs to use
            the link in that email to verify the address. Once it&rsquo;s verified, they&rsquo;ll
            need to log in with it to complete the change.
          </p>
          {buttonsWithMessage}
        </>
      )}
    </Alert>
  )
}

export default PendingEmailUpdateAlert
