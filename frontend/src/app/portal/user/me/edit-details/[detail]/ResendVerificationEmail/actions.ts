'use server'

import { unstable_rethrow } from 'next/navigation'

import { resendVerificationEmail } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'

export type ResendVerificationState =
  { status: 'idle' } | { status: 'success' } | { status: 'error'; message: string }

export const resendWorkEmailVerification = async (
  userId: number,
  _prevState: unknown,
  _formData: FormData,
): Promise<ResendVerificationState> => {
  try {
    const { response, error } = await resendVerificationEmail({
      path: { userId },
      client: await createServerApiClient(),
    })

    if (response?.ok) {
      return { status: 'success' }
    }

    return {
      status: 'error',
      message: error?.title ?? 'An unexpected error occurred.',
    }
  } catch (error) {
    unstable_rethrow(error)
    return { status: 'error', message: 'An unexpected error occurred.' }
  }
}
