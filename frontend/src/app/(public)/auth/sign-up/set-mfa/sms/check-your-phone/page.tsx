'use client'

import { revalidateLogic, useForm } from '@tanstack/react-form'
import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { useState, type ChangeEvent } from 'react'
import { z } from 'zod'

import { postAuthVerifySmsMfa } from '@/client/generated/sdk.gen'
import { BackLink } from '@/components/BackLink/BackLink'
import { Button } from '@/components/Button/Button'
import { Input } from '@/components/Input/Input'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { errorMessages } from '@/lib/form/errorMessages'
import { getFieldErrorMessage } from '@/lib/form/getFieldErrorMessage'

const codeSchema = z.object({
  securityCode: z
    .string()
    .trim()
    .min(1, errorMessages.securityCodeRequired)
    .refine(
      (value) => /^\d{6}$/.test(value.replace(/[\s-]/g, '')),
      errorMessages.securityCodeFormat,
    ),
})

type FormValues = z.input<typeof codeSchema>

function normaliseSecurityCode(value: string) {
  return value.replace(/[\s-]/g, '')
}

export default function CheckYourPhone() {
  const router = useRouter()
  const [submitError, setSubmitError] = useState<string | null>(null)

  let authenticationSession = ''
  let setupToken = ''
  let hasSetup = false
  try {
    const stored = sessionStorage.getItem('signUpSmsSetup')
    if (stored) {
      const parsed = JSON.parse(stored)
      authenticationSession = parsed.authenticationSession ?? ''
      setupToken = parsed.setupToken ?? ''
      hasSetup = Boolean(authenticationSession && setupToken)
    }
  } catch {
    hasSetup = false
  }

  const form = useForm({
    defaultValues: { securityCode: '' } satisfies FormValues,
    validationLogic: revalidateLogic({ mode: 'submit', modeAfterSubmission: 'blur' }),
    validators: { onDynamic: codeSchema },
    onSubmit: async ({ value, formApi }) => {
      const { securityCode } = codeSchema.parse(value)
      setSubmitError(null)

      try {
        const result = await postAuthVerifySmsMfa({
          body: {
            authenticationSession,
            code: normaliseSecurityCode(securityCode),
            setupToken,
          },
          credentials: 'include',
        })

        if (!result.error) {
          try {
            sessionStorage.removeItem('signUpSmsSetup')
          } catch {}

          router.push('/portal')
          return
        }

        if (result.response?.status === 400) {
          formApi.setErrorMap({
            onSubmit: {
              fields: {
                securityCode: errorMessages.incorrectMfaCode,
              },
            },
          })
          return
        }

        setSubmitError(
          result.error.detail ?? 'We could not verify your authentication code. Try again later.',
        )
      } catch {
        setSubmitError('We could not verify your authentication code. Try again later.')
      }
    },
  })

  if (!hasSetup) {
    return (
      <>
        <PageHeader heading="There is a problem setting up two-factor authentication" />
        <p>
          We could not find your multi-factor authentication setup details. Return to your sign-up
          link and try again.
        </p>
      </>
    )
  }

  return (
    <>
      <PageHeader
        backLink={<BackLink href="/auth/sign-up/set-mfa/sms/enter-phone-number">Back</BackLink>}
        heading="Check your phone"
      />
      <p>We have sent you a text message with a security code.</p>
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
                inputMode="numeric"
                label="Enter the security code"
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

        {submitError ? <p>{submitError}</p> : null}

        <Button type="submit" variant="cta">
          Continue
        </Button>
      </form>
      <p>
        <Link href="/auth/sign-up/set-mfa/sms/request-new-code">Not received text message?</Link>
      </p>
    </>
  )
}
