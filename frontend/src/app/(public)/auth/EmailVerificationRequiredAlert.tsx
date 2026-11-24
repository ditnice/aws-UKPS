import { Alert } from '@/components/Alert/Alert'

const EmailVerificationRequiredAlert = () => {
  return (
    <Alert title="Email not verified" type="error">
      <h3>Email not verified</h3>
      <p>
        We couldn&apos;t log you in because this pending email address hasn&apos;t been verified
        yet. Please click the verification link we sent to your inbox, then try logging in again.
        Check your spam folder if you can&apos;t find it.
      </p>
    </Alert>
  )
}

export default EmailVerificationRequiredAlert
