'use server'

import { initialFormState, ServerValidateError } from '@tanstack/react-form-nextjs'
import { redirect } from 'next/navigation'

import { confirmNewPassword } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { getFormStateServerErrors } from '@/lib/form/formErrorHandling'
import { isValidationProblemDetails } from '@/lib/responses/typeGuards'

import { createPasswordUpdateSuccessHref } from '../success/navigation'

import { serverValidate } from './confirmResetPasswordShared'

export const confirmResetPasswordAction = async (prev: unknown, formData: FormData) => {
  try {
    const values = await serverValidate(formData)
    const { error, response } = await confirmNewPassword({
      client: await createServerApiClient(),
      body: values,
    })
    if (isValidationProblemDetails(error)) {
      return {
        ...initialFormState,
        values,
        ...getFormStateServerErrors(error.errors, ['confirmationCode', 'newPassword']),
      }
    }

    // Cognito's rate limits: the user needs the support team to recover their account.
    if (response?.status === 429) {
      redirect('/auth/account-recovery')
    }

    if (error || !response?.ok) {
      return {
        ...initialFormState,
        values,
        errors: [
          'An unexpected error occurred while confirming your password reset. Please try again.',
        ],
      }
    }
  } catch (e) {
    if (e instanceof ServerValidateError) {
      // Its errors are all for fields, which show them; as form-level errors they have no text.
      return { ...e.formState, errors: [] }
    }
    throw e
  }
  redirect(createPasswordUpdateSuccessHref())
}
