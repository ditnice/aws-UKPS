'use client'

import { revalidateLogic, useForm } from '@tanstack/react-form'
import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { z } from 'zod'

import { Button } from '@nice-digital/nds-button'

import { postAuthMfa } from '@/client/generated'
import { getFieldErrorMessage } from '@/components/Form/getFieldErrorMessage'
import { Input } from '@/components/Input/Input'

import { routeOnSuccessfulAuth } from '../../../constants'

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

type SignInFormProps = {
  username: string
  session: string
}
export function SignInMfaForm({ username, session }: SignInFormProps) {
  const router = useRouter()
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
    onSubmit: async ({ value, formApi }) => {
      const { securityCode } = signInMfaSchema.parse(value)
      console.log(normaliseSecurityCode(securityCode))
      const response = await postAuthMfa({
        body: { username, code: value.securityCode, authenticationSession: session },
        credentials: 'include',
      })
      if (!response.error) {
        router.push(routeOnSuccessfulAuth)
      }
      if (response.error?.status === 401) {
        formApi.setErrorMap({
          onSubmit: {
            fields: {
              securityCode: 'Invalid security code.',
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
              hint={`Enter the 6-digit authentication code shown in the app for ${username}.`}
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

      <Button type="submit" variant="cta">
        Continue
      </Button>

      {/* TODO - link this up when the page is created */}
      <p className={styles.helpLink}>
        <Link href="/">Contact UKPS support</Link>
      </p>
    </form>
  )
}
