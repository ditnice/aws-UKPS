import { PageHeader } from '@/components/PageHeader/PageHeader'

import ResetPasswordForm from './ResetPasswordForm'

const Page = () => {
  return (
    <>
      <PageHeader heading="Enter your email address" preheading="Reset your password" />
      <p>We will email you a code to reset your password.</p>
      <ResetPasswordForm />
    </>
  )
}

export default Page
