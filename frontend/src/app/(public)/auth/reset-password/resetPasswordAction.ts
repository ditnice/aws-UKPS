'use server'

import { initialFormState, ServerValidateError } from '@tanstack/react-form-nextjs'
import { cookies } from 'next/headers'
import { redirect } from 'next/navigation'

import { initiateForgottenPasswordProcedure } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { getFormStateServerErrors } from '@/lib/form/formErrorHandling'
import { isValidationProblemDetails } from '@/lib/responses/typeGuards'

import { createResetPasswordCodeHref } from './confirm/navigation'
import { resetEmailCookieName, serverValidate } from './resetPasswordShared'

export const resetPasswordAction = async (prev: unknown, formData: FormData) => {
  try {
    const values = await serverValidate(formData)
    const { error, response } = await initiateForgottenPasswordProcedure({
      client: await createServerApiClient(),
      body: values,
    })

    if (isValidationProblemDetails(error)) {
      return { ...initialFormState, values, ...getFormStateServerErrors(error.errors) }
    }

    if (error || !response?.ok) {
      let message = undefined
      switch (response?.status) {
        case 429:
          message = 'Number of requests exceeded limit. Try again later.'
          break
        default:
          message =
            'An unexpected error occurred while attempting to reset your password. Please try again.'
          break
      }

      return { ...initialFormState, errors: [message] }
    }

    // passed to the next page to pre-fill the email address
    // field to avoid passing it in the url query string
    const myCookies = await cookies()
    myCookies.set(resetEmailCookieName, values.email, {
      httpOnly: true,
      secure: true,
      sameSite: 'lax',
      path: '/',
      maxAge: 60 * 30, // 30 minutes
    })
  } catch (e) {
    if (e instanceof ServerValidateError) {
      return e.formState
    }
    throw e
  }

  redirect(createResetPasswordCodeHref())
}
