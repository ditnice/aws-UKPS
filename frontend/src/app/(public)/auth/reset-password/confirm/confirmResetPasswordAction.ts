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
        ...getFormStateServerErrors(error.errors),
      }
    }

    if (error || !response?.ok) {
      let message = undefined
      switch (response?.status) {
        case 429:
          message = 'Number of requests exceeded limit. Try again later.'
          break
        default:
          message =
            'An unexpected error occurred while confirming your password reset. Please try again.'
          break
      }

      return { ...initialFormState, errors: [message] }
    }
  } catch (e) {
    if (e instanceof ServerValidateError) {
      return e.formState
    }
    throw e
  }
  redirect(createPasswordUpdateSuccessHref())
}
