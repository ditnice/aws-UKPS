'use server'

import { cookies } from 'next/headers'
import { notFound } from 'next/navigation'

import { PageHeader } from '@/components/PageHeader/PageHeader'

import { resetEmailCookieName } from '../resetPasswordShared'

import ConfirmResetPasswordForm from './ConfirmResetPasswordForm'

const Page = async () => {
  const myCookies = await cookies()
  const emailCookie = myCookies.get(resetEmailCookieName)

  if (!emailCookie) {
    // The email cookie should be set in the previous page.
    notFound()
  }

  return (
    <>
      <PageHeader heading="Reset your password" />
      <p>An email containing a confirmation code has been sent to {emailCookie.value}.</p>
      <ConfirmResetPasswordForm email={emailCookie.value} />
    </>
  )
}

export default Page
