'use server'

import { redirect, unstable_rethrow } from 'next/navigation'

import { resendVerificationEmail } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'

export type Navigations = {
  onSuccess: string
  onError: string
}

export type ResendVerificationState =
  { status: 'idle' } | { status: 'success' } | { status: 'error'; message: string }

export const resendWorkEmailVerification = async (
  navigation: Navigations,
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
      redirect(navigation.onSuccess)
    }

    redirect(navigation.onError)
  } catch (error) {
    unstable_rethrow(error)
    redirect(navigation.onError)
  }
}
