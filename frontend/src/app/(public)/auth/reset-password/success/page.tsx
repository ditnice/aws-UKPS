import Link from 'next/link'

import { Alert } from '@/components/Alert/Alert'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { buildSignInHref } from '@/lib/auth/routing'

const Page = () => {
  return (
    <>
      <PageHeader heading="Reset your password" />
      <Alert title="Password changed successfully" type="success">
        <p>
          User you new password to sign in. You will need access to your authenticator to sign in
          with your new password.
        </p>
        <p>You can your password at any time from manage my details.</p>
      </Alert>
      <Link href={buildSignInHref(undefined)}>Sign in to your account</Link>
    </>
  )
}

export default Page
