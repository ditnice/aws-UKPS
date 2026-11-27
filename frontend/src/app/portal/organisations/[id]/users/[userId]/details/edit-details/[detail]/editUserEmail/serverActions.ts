'use server'

import { initialFormState, ServerValidateError } from '@tanstack/react-form-nextjs'
import { redirect } from 'next/navigation'

import { cancelPendingEmailChange, updateUserEmail } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { errorMessages } from '@/lib/form/errorMessages'
import { getFormStateServerErrors } from '@/lib/form/formErrorHandling'
import { isValidationProblemDetails } from '@/lib/responses/typeGuards'

import { serverValidate } from './shared'

export const updateUserEmailAction = async (
  onSuccessRedirect: string,
  userId: number,
  prev: unknown,
  formData: FormData,
) => {
  try {
    const values = await serverValidate(formData)
    const { error, response } = await updateUserEmail({
      client: await createServerApiClient(),
      path: { userId },
      body: { ...values },
    })

    if (response?.ok) {
      redirect(onSuccessRedirect)
    }

    if (response?.status == 409) {
      return {
        ...initialFormState,
        values,
        errorMap: {
          onServer: {
            fields: { value: 'Provided email address conflicts with an existing email.' },
          },
        },
      }
    }

    if (isValidationProblemDetails(error)) {
      return { ...initialFormState, values, ...getFormStateServerErrors(error.errors) }
    }

    if (error || !response?.ok) {
      return {
        ...initialFormState,
        values,
        errors: [errorMessages.genericWithRetry],
      }
    }
  } catch (e) {
    if (e instanceof ServerValidateError) {
      return e.formState
    }
    throw e
  }
}

export const cancelPendingEmailChangeAction = async (onCancel: string, userId: number) => {
  const { error, response } = await cancelPendingEmailChange({
    client: await createServerApiClient(),
    path: { userId },
  })

  if (response?.ok) {
    redirect(onCancel)
  }

  if (error || !response?.ok) {
    return { errors: [errorMessages.genericWithRetry] }
  }
}
