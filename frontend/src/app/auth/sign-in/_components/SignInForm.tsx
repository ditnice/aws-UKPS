'use client'

import { revalidateLogic, useForm } from '@tanstack/react-form'
import { useRouter } from 'next/navigation'
import { type ChangeEvent } from 'react'
import { z } from 'zod'

import {
  EMAIL_FORMAT_ERROR_MESSAGE,
  PERSONAL_EMAIL_REQUIRED_ERROR_MESSAGE,
} from '@/app/common/form/ErrorMessages'
import { getFieldErrorMessage } from '@/app/common/form/getFieldErrorMessage'
import { postAuthLogin } from '@/client/generated'
import { Button } from '@/components/Button/Button'
import { Details } from '@/components/Details/Details'
import { Input } from '@/components/Input/Input'
import { PasswordInput } from '@/components/PasswordInput/PasswordInput'

import { routeOnSuccessfulAuth } from '../../constants'

const signInSchema = z.object({
  email: z
    .string()
    .trim()
    .min(1, PERSONAL_EMAIL_REQUIRED_ERROR_MESSAGE)
    .pipe(z.email(EMAIL_FORMAT_ERROR_MESSAGE)),
  password: z.string().min(1, 'Enter your password'),
})

type SignInFormValues = z.input<typeof signInSchema>
type SignInFormProps = {
  returnTo?: string
}

export function SignInForm({ returnTo }: SignInFormProps) {
  const router = useRouter()
  const form = useForm({
    defaultValues: {
      email: '',
      password: '',
    } satisfies SignInFormValues,
    validationLogic: revalidateLogic({
      mode: 'submit',
      modeAfterSubmission: 'blur',
    }),
    validators: {
      onDynamic: signInSchema,
    },
    onSubmit: async ({ value, formApi }) => {
      signInSchema.parse(value)

      const result = await postAuthLogin({
        body: {
          username: value.email,
          password: value.password,
        },
        credentials: 'include',
      })

      if (!result.error) {
        router.push(returnTo ?? routeOnSuccessfulAuth)
        return
      }

      if (result.error.challengeType === 'MultiFactorAuthenticationRequired') {
        const { authenticationSession } = result.error

        if (!authenticationSession) {
          throw new Error('Expected authentication session to be set.')
        }

        const params = new URLSearchParams({
          username: value.email,
          session: authenticationSession,
        })
        if (returnTo) params.set('returnTo', returnTo)

        router.push(`/auth/sign-in/mfa?${params.toString()}`)
        return
      }

      if (result.error.status === 401) {
        formApi.setErrorMap({
          onSubmit: {
            fields: {
              email: 'Invalid email or password',
              password: 'Invalid email or password',
            },
          },
        })
      }
    },
  })

  return (
    <form
      noValidate
      onSubmit={(event) => {
        event.preventDefault()
        event.stopPropagation()
        form.handleSubmit()
      }}
    >
      <form.Field name="email">
        {(field) => {
          const errorMessage = getFieldErrorMessage(field.state.meta.errors)

          return (
            <Input
              autoComplete="email"
              error={Boolean(errorMessage)}
              errorMessage={errorMessage}
              label="Email address"
              name={field.name}
              onBlur={field.handleBlur}
              onChange={(event: ChangeEvent<HTMLInputElement>) =>
                field.handleChange(event.target.value)
              }
              type="email"
              value={field.state.value}
              width="one-third"
            />
          )
        }}
      </form.Field>

      <form.Field name="password">
        {(field) => {
          const errorMessage = getFieldErrorMessage(field.state.meta.errors)

          return (
            <PasswordInput
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

      <Details summary="Forgotten your password?">
        If you have forgotten your password visit the{' '}
        {/* TODO - Update this to point to the correct page once built */}
        <a href="#" target="_blank">
          account recovery (opens in a new tab)
        </a>{' '}
        page.
      </Details>

      <Button type="submit" variant="cta">
        Continue
      </Button>
    </form>
  )
}
