'use server'

import { redirect, unstable_rethrow } from 'next/navigation'

import { resendVerificationEmail } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'

import { buildUpdateUserDetailsHref } from '../navigation'

export type ResendVerificationState =
  { status: 'idle' } | { status: 'success' } | { status: 'error'; message: string }

export const resendWorkEmailVerification = async (
  userId: number,
  _prevState: unknown,
  _formData: FormData,
): Promise<ResendVerificationState> => {
  try {
    const { response } = await resendVerificationEmail({
      path: { userId },
      client: await createServerApiClient(),
    })

    if (response?.ok) {
      redirect(buildUpdateUserDetailsHref({ detail: 'email', emailLinkResent: true }))
    }

    redirect(buildUpdateUserDetailsHref({ detail: 'email', error: true }))
  } catch (error) {
    unstable_rethrow(error)
    redirect(buildUpdateUserDetailsHref({ detail: 'email', error: true }))
  }
}
