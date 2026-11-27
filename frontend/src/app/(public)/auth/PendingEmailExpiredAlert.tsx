import { Alert } from '@/components/Alert/Alert'

const PendingEmailExpiredAlert = () => {
  return (
    <Alert title="Email change expired" type="error">
      <h2>Email change expired</h2>
      <p>
        We couldn&apos;t log you in because the request to change your email address to this one has
        expired. Log in with your current email address, then request the email change again.
      </p>
    </Alert>
  )
}

export default PendingEmailExpiredAlert
