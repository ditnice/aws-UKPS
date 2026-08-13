'use client'

import { revalidateLogic, useForm } from '@tanstack/react-form'
import { useRouter } from 'next/navigation'
import { useState, type ChangeEvent } from 'react'
import { z } from 'zod'

import { Button } from '@nice-digital/nds-button'

import { postAuthSetupUser } from '@/client/generated/sdk.gen'
import { getFieldErrorMessage } from '@/components/Form/getFieldErrorMessage'
import { PasswordInput } from '@/components/PasswordInput/PasswordInput'

import { signUpMfaSetupStorageKey } from '../../constants'

const signUpSetPasswordSchema = z.object({
  password: z
    .string()
    .min(1, 'Enter your password')
    .min(8, 'Password must be at least 8 characters long'),
})

type SignUpSetPasswordFormValues = z.input<typeof signUpSetPasswordSchema>

type SignUpSetPasswordFormProps = {
  setupToken: string
}

export function SignUpSetPasswordForm({ setupToken }: SignUpSetPasswordFormProps) {
  const router = useRouter()
  const [submitError, setSubmitError] = useState<string | null>(null)
  const form = useForm({
    defaultValues: {
      password: '',
    } satisfies SignUpSetPasswordFormValues,
    validationLogic: revalidateLogic({
      mode: 'submit',
      modeAfterSubmission: 'blur',
    }),
    validators: {
      onDynamic: signUpSetPasswordSchema,
    },
    onSubmit: async ({ value, formApi }) => {
      signUpSetPasswordSchema.parse(value)
      setSubmitError(null)

      try {
        const result = await postAuthSetupUser({
          body: {
            setupToken,
            newPassword: value.password,
          },
          credentials: 'include',
        })

        if (!result.error) {
          if (!result.data?.otpAuthUri || !result.data.authenticationSession) {
            throw new Error('Expected multi-factor authentication setup data.')
          }

          try {
            sessionStorage.setItem(
              signUpMfaSetupStorageKey,
              JSON.stringify({
                authenticationSession: result.data.authenticationSession,
                otpAuthUri: result.data.otpAuthUri,
                setupToken,
              }),
            )
          } catch {
            setSubmitError(
              'Your password was created, but we could not continue to two-factor authentication setup. Return to your sign-up link and try again.',
            )
            return
          }

          router.push('/auth/sign-up/set-mfa')
          return
        }

        if (result.response?.status === 400) {
          formApi.setErrorMap({
            onSubmit: {
              fields: {
                password: 'The password does not meet the expected standards.',
              },
            },
          })
          return
        }

        setSubmitError(getSubmitError(result.error, result.response?.status))
      } catch {
        setSubmitError('We could not create your password. Try again later.')
      }
    },
  })

  return (
    <form
      noValidate
      onSubmit={(event) => {
        event.preventDefault()
        event.stopPropagation()
        void form.handleSubmit()
      }}
    >
      <p>Your password must:</p>
      <ul>
        <li>be at least 8 characters long</li>
      </ul>

      <form.Field name="password">
        {(field) => {
          const errorMessage = getFieldErrorMessage(field.state.meta.errors)

          return (
            <PasswordInput
              autoComplete="new-password"
              error={Boolean(errorMessage)}
              errorMessage={errorMessage}
              label="Password"
              name={field.name}
              onBlur={field.handleBlur}
              onChange={(event: ChangeEvent<HTMLInputElement>) =>
                field.handleChange(event.target.value)
              }
              value={field.state.value}
              width="one-third"
            />
          )
        }}
      </form.Field>

      {submitError ? <p>{submitError}</p> : null}

      <Button type="submit" variant="cta">
        Continue
      </Button>
    </form>
  )
}

function getSubmitError(error: { detail?: null | string }, status?: number) {
  if (status === 401) {
    return error.detail ?? 'This sign-up link has expired or has already been used.'
  }

  if (status === 404) {
    return error.detail ?? 'This sign-up link could not be found.'
  }

  return error.detail ?? 'We could not create your password. Try again later.'
}
