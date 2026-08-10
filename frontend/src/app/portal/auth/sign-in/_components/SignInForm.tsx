'use client'

import { revalidateLogic, useForm } from '@tanstack/react-form'
import { z } from 'zod'

import { Button } from '@/components/Button/Button'
import { Details } from '@/components/Details/Details'
import { getFieldErrorMessage } from '@/components/Form/getFieldErrorMessage'
import { Input } from '@/components/Input/Input'
import { PasswordInput } from '@/components/PasswordInput/PasswordInput'

import type { ChangeEvent } from 'react'

const signInSchema = z.object({
  email: z
    .string()
    .trim()
    .min(1, 'Enter your email address')
    .pipe(z.email('Enter an email address in the correct format, like name@example.com')),
  password: z.string().min(1, 'Enter your password'),
})

type SignInFormValues = z.input<typeof signInSchema>

export function SignInForm() {
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
    onSubmit: ({ value }) => {
      signInSchema.parse(value)
      // Authentication will be wired once the submit target is confirmed.
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
