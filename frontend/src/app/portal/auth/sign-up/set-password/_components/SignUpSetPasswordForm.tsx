'use client'

import { revalidateLogic, useForm } from '@tanstack/react-form'
import { z } from 'zod'

import { Button } from '@nice-digital/nds-button'

import { getFieldErrorMessage } from '@/components/Form/getFieldErrorMessage'
import { PasswordInput } from '@/components/PasswordInput/PasswordInput'

import type { ChangeEvent } from 'react'

const signUpSetPasswordSchema = z.object({
  password: z
    .string()
    .min(1, 'Enter your password')
    .min(8, 'Password must be at least 8 characters long'),
})

type SignUpSetPasswordFormValues = z.input<typeof signUpSetPasswordSchema>

export function SignUpSetPasswordForm() {
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
    onSubmit: ({ value }) => {
      signUpSetPasswordSchema.parse(value)
      // Password setup will be wired once the submit target is confirmed.
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
        {/* TODO - update TF to move from 14 to 8 */}
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

      <Button type="submit" variant="cta">
        Continue
      </Button>
    </form>
  )
}
