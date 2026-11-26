'use client'

import { revalidateLogic, useForm } from '@tanstack/react-form'
import Link from 'next/link'
import { useState } from 'react'
import { z } from 'zod'

import { respondToMultiFactorAuthenticationChallenge } from '@/client/generated'
import { Button } from '@/components/Button/Button'
import { Input } from '@/components/Input/Input'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { navigateOnSuccessfulAuth } from '@/lib/auth/routing'
import { errorMessages } from '@/lib/form/errorMessages'
import { submitForm } from '@/lib/form/formErrorHandling'
import { getFieldErrorMessage, getFormErrorMessage } from '@/lib/form/getFieldErrorMessage'

import EmailVerificationRequiredAlert from '../../../EmailVerificationRequiredAlert'

import styles from './SignInMfaForm.module.scss'

import type { ChangeEvent } from 'react'

function normaliseSecurityCode(value: string) {
  return value.replace(/[\s-]/g, '')
}

const signInMfaSchema = z.object({
  securityCode: z
    .string()
    .trim()
    .min(1, errorMessages.securityCodeRequired)
    .refine(
      (value) => /^\d{6}$/.test(normaliseSecurityCode(value)),
      errorMessages.securityCodeFormat,
    ),
})

type SignInMfaFormValues = z.input<typeof signInMfaSchema>

type SignInFormProps = {
  returnTo?: string
  username: string
  session: string
}
export function SignInMfaForm({ username, returnTo, session }: SignInFormProps) {
  // The authentication session is single-use, so once it has been accepted the form stays disabled
  // while the full page load to the portal is in progress.
  const [succeeded, setSucceeded] = useState(false)

  const form = useForm({
    defaultValues: {
      securityCode: '',
    } satisfies SignInMfaFormValues,
    validationLogic: revalidateLogic({
      mode: 'submit',
      modeAfterSubmission: 'submit',
    }),
    validators: {
      onDynamic: signInMfaSchema,
    },
    onSubmit: async ({ value, formApi }) => {
      const { securityCode } = signInMfaSchema.parse(value)
      const response = await respondToMultiFactorAuthenticationChallenge({
        body: {
          username,
          code: normaliseSecurityCode(securityCode),
          authenticationSession: session,
        },
        credentials: 'include',
      })
      if (!response.error) {
        setSucceeded(true)
        navigateOnSuccessfulAuth(returnTo)
        return
      }
      if (response.error?.code === 'PendingEmailRequiredVerification') {
        formApi.setErrorMap({
          onSubmit: {
            form: 'PendingEmailRequiredVerification',
            fields: {},
          },
        })
        return
      }
      if (response.error?.status === 401) {
        formApi.setErrorMap({
          onSubmit: {
            form: '',
            fields: {
              securityCode: errorMessages.incorrectMfaCode,
            },
          },
        })
      }
    },
  })

  return (
    <form
      method="post"
      noValidate
      onSubmit={(event) => {
        event.preventDefault()
        event.stopPropagation()
        if (succeeded) return
        void submitForm(form)
      }}
    >
      <form.Subscribe selector={(state) => getFormErrorMessage(state.errorMap.onSubmit)}>
        {(formError) => {
          if (!formError) return null
          if (formError === 'PendingEmailRequiredVerification')
            return <EmailVerificationRequiredAlert />
          return <ErrorState>{formError}</ErrorState>
        }}
      </form.Subscribe>
      <form.Field name="securityCode">
        {(field) => {
          const errorMessage = getFieldErrorMessage(field.state.meta.errors)

          return (
            <Input
              autoComplete="one-time-code"
              error={Boolean(errorMessage)}
              errorMessage={errorMessage}
              hint={`Enter the 6-digit authentication code.`}
              inputMode="numeric"
              label="Security code"
              name={field.name}
              onBlur={field.handleBlur}
              onChange={(event: ChangeEvent<HTMLInputElement>) =>
                field.handleChange(event.target.value)
              }
              type="text"
              value={field.state.value}
              width="one-quarter"
            />
          )
        }}
      </form.Field>

      <Button disabled={succeeded} type="submit" variant="cta">
        Continue
      </Button>

      {/* TODO 620 - link this up when the page is created */}
      <p className={styles.helpLink}>
        <Link href="/">I do not have access to my authenticator</Link>
      </p>
    </form>
  )
}
