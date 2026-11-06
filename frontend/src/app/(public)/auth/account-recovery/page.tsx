import { connection } from 'next/server'

import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'

export default async function AccountRecovery() {
  await connection()
  const supportEmail = process.env.QA_SUPPORT_EMAIL

  return (
    <>
      <PageHeader
        backLink={<BackLink href="/auth/sign-in">Back</BackLink>}
        preheading="Recover your UK PharmaScan account"
        heading="Contact the support team to recover your account"
      />
      <p>
        If you cannot recover your account, contact the UK PharmaScan support team for assistance.
        For example, you do not know both the email address or password you use to sign in.
      </p>
      {supportEmail ? (
        <p>
          You can contact the support team by email{' '}
          <a href={`mailto:${supportEmail}`}>{supportEmail}</a>.
        </p>
      ) : (
        <p>Please contact the UK PharmaScan support team for assistance.</p>
      )}
    </>
  )
}
