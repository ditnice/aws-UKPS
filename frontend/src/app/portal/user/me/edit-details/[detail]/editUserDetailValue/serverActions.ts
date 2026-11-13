'use server'

import { initialFormState, ServerValidateError } from '@tanstack/react-form-nextjs'
import { redirect } from 'next/navigation'

import { updateUserDetails } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { errorMessages } from '@/lib/form/errorMessages'
import { getFormStateServerErrors } from '@/lib/form/formErrorHandling'
import { isValidationProblemDetails } from '@/lib/responses/typeGuards'

import { buildMyDetailsHref } from '../../../navigation'

import { serverValidate } from './shared'

export const updateUserDetailsAction = async (
  userId: number,
  prev: unknown,
  formData: FormData,
) => {
  try {
    const values = await serverValidate(formData)
    const { error, response } = await updateUserDetails({
      client: await createServerApiClient(),
      path: { userId },
      body: { ...values },
    })

    if (response?.ok) {
      redirect(buildMyDetailsHref({ updated: true }))
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
