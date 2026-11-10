import { Alert } from '@/components/Alert/Alert'
import { Button } from '@/components/Button/Button'

type ReauthenticationRequiredBannerProps = {
  reauthenticationRequested: () => void
  maxAge: number | null
}

export const ReauthenticationRequiredBanner = ({
  reauthenticationRequested,
  maxAge,
}: ReauthenticationRequiredBannerProps) => {
  const maxAgeInMinutes = maxAge !== null ? Math.ceil(maxAge / 60) : null

  return (
    <Alert type="error">
      <h2>You need to sign in again to change your email address</h2>
      <p>
        For your security, we need to check it&apos;s you before we can change your email address.
        {maxAgeInMinutes !== null && (
          <>
            {' '}
            You need to sign in again if you have been signed in for more than {
              maxAgeInMinutes
            }{' '}
            {maxAgeInMinutes === 1 ? 'minute' : 'minutes'}.
          </>
        )}
      </p>
      <ol>
        <li>Select &quot;Sign in again&quot;.</li>
        <li>Sign in on the next page.</li>
        <li>
          You&apos;ll come back to this form with your details still filled in. Select
          &quot;Save&quot;.
        </li>
      </ol>
      <Button type="button" onClick={reauthenticationRequested}>
        Sign in again
      </Button>
    </Alert>
  )
}
