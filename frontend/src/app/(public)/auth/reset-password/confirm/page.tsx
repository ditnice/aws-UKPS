'use server'

import { cookies } from 'next/headers'
import { notFound } from 'next/navigation'

import { PageHeader } from '@/components/PageHeader/PageHeader'

import { resetEmailCookieName } from '../resetPasswordShared'

import ConfirmResetPasswordForm from './ConfirmResetPasswordForm'
import ResendPasswordLinkSection from './ResendPasswordLinkSection'

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
      <ResendPasswordLinkSection email={emailCookie.value} />
      <hr />
      <ConfirmResetPasswordForm email={emailCookie.value} />
    </>
  )
}

export default Page
