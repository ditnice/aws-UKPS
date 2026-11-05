import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'

const supportEmail = process.env.NEXT_PUBLIC_QA_SUPPORT_EMAIL

export default function RegistrationRequest() {
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
      <p>
        You can contact the support team by email{' '}
        <a href={`mailto:${supportEmail}`}>{supportEmail}</a>.
      </p>
    </>
  )
}
