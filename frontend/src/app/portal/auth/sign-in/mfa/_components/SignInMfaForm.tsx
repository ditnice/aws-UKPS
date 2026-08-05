'use client'

import { revalidateLogic, useForm } from '@tanstack/react-form'
import Link from 'next/link'
import { z } from 'zod'

import { Button } from '@nice-digital/nds-button'

import { getFieldErrorMessage } from '@/components/Form/getFieldErrorMessage'
import { Input } from '@/components/Input/Input'

import styles from './SignInMfaForm.module.scss'

import type { ChangeEvent } from 'react'

function normaliseSecurityCode(value: string) {
  return value.replace(/[\s-]/g, '')
}

const signInMfaSchema = z.object({
  securityCode: z
    .string()
    .trim()
    .min(1, 'Enter your security code')
    .refine(
      (value) => /^\d{6}$/.test(normaliseSecurityCode(value)),
      'Enter a 6-digit security code',
    ),
})

type SignInMfaFormValues = z.input<typeof signInMfaSchema>

export function SignInMfaForm() {
  const form = useForm({
    defaultValues: {
      securityCode: '',
    } satisfies SignInMfaFormValues,
    validationLogic: revalidateLogic({
      mode: 'submit',
      modeAfterSubmission: 'blur',
    }),
    validators: {
      onDynamic: signInMfaSchema,
    },
    onSubmit: ({ value }) => {
      const { securityCode } = signInMfaSchema.parse(value)
      console.log(normaliseSecurityCode(securityCode))
      // MFA verification will be wired once the submit target is confirmed.
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
      <form.Field name="securityCode">
        {(field) => {
          const errorMessage = getFieldErrorMessage(field.state.meta.errors)

          return (
            <Input
              autoComplete="one-time-code"
              error={Boolean(errorMessage)}
              errorMessage={errorMessage}
              hint="Enter the 6-digit authentication code shown in the app."
              inputMode="numeric"
              label="Security code"
              name={field.name}
              onBlur={field.handleBlur}
              onChange={(event: ChangeEvent<HTMLInputElement>) =>
                field.handleChange(event.target.value)
              }
              type="text"
              value={field.state.value}
              width="one-third"
            />
          )
        }}
      </form.Field>

      <Button type="submit" variant="cta">
        Continue
      </Button>

      {/* TODO - link this up when the page is created */}
      <p className={styles.helpLink}>
        <Link href="/">Having trouble?</Link>
      </p>
    </form>
  )
}
