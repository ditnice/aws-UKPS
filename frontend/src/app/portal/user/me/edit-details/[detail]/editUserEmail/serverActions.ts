'use server'

import { initialFormState, ServerValidateError } from '@tanstack/react-form-nextjs'
import { redirect } from 'next/navigation'

import { updateUserEmail } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { errorMessages } from '@/lib/form/errorMessages'
import { getFormStateServerErrors } from '@/lib/form/formErrorHandling'
import { isValidationProblemDetails } from '@/lib/responses/typeGuards'

import { serverValidate } from './shared'

export const updateUserEmailAction = async (userId: number, prev: unknown, formData: FormData) => {
  try {
    const values = await serverValidate(formData)
    const { error, response } = await updateUserEmail({
      client: await createServerApiClient(),
      path: { userId },
      body: { ...values },
    })

    if (response?.ok) {
      redirect('/portal/user/me?updated=true')
    }

    if (response?.status == 409) {
      // Remove this
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
