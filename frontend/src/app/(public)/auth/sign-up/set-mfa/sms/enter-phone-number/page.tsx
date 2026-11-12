'use client'

import { revalidateLogic, useForm } from '@tanstack/react-form'
import { isValidPhoneNumber } from 'libphonenumber-js/max'
import { useRouter } from 'next/navigation'
import { useState, type ChangeEvent } from 'react'
import { z } from 'zod'

import { postAuthSetupSmsMfa } from '@/client/generated/sdk.gen'
import { BackLink } from '@/components/BackLink/BackLink'
import { Button } from '@/components/Button/Button'
import { Input } from '@/components/Input/Input'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { errorMessages } from '@/lib/form/errorMessages'
import { getFieldErrorMessage } from '@/lib/form/getFieldErrorMessage'

import { signUpMfaSetupStorageKey } from '../../../_lib/mfaSetupStorage'

const phoneSchema = z.object({
  phoneNumber: z
    .string()
    .trim()
    .min(1, errorMessages.personalPhoneRequired)
    .refine((value) => isValidPhoneNumber(value, 'GB'), errorMessages.phoneFormat),
})

type FormValues = z.input<typeof phoneSchema>

export default function EnterPhoneNumber() {
  const router = useRouter()
  const [submitError, setSubmitError] = useState<string | null>(null)

  let setupToken = ''
  let hasSetup = false
  try {
    const stored = sessionStorage.getItem(signUpMfaSetupStorageKey)
    if (stored) {
      setupToken = JSON.parse(stored).setupToken ?? ''
      hasSetup = Boolean(setupToken)
    }
  } catch {
    hasSetup = false
  }

  const form = useForm({
    defaultValues: { phoneNumber: '' } satisfies FormValues,
    validationLogic: revalidateLogic({ mode: 'submit', modeAfterSubmission: 'blur' }),
    validators: { onDynamic: phoneSchema },
    onSubmit: async ({ value, formApi }) => {
      if (!setupToken) return

      const { phoneNumber } = phoneSchema.parse(value)
      setSubmitError(null)

      try {
        const result = await postAuthSetupSmsMfa({
          body: { setupToken, phoneNumber },
          credentials: 'include',
        })

        if (!result.error) {
          try {
            sessionStorage.setItem(
              'signUpSmsSetup',
              JSON.stringify({
                phoneNumber,
                setupToken,
                authenticationSession: (
                  result.data as { authenticationSession?: string } | undefined
                )?.authenticationSession,
              }),
            )
          } catch {
            setSubmitError(
              'Your phone number was saved, but we could not continue. Return to your sign-up link and try again.',
            )
            return
          }

          router.push('/auth/sign-up/set-mfa/sms/check-your-phone')
          return
        }

        if (result.response?.status === 400) {
          formApi.setErrorMap({
            onSubmit: {
              fields: {
                phoneNumber: errorMessages.phoneFormat,
              },
            },
          })
          return
        }

        setSubmitError(
          result.error.detail ?? 'We could not set up SMS authentication. Try again later.',
        )
      } catch {
        setSubmitError('We could not set up SMS authentication. Try again later.')
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
        backLink={<BackLink href="/auth/sign-up/set-mfa">Back</BackLink>}
        heading="Enter your phone number"
      />
      <p>
        Text messages will be sent to this phone to verify your identity when you sign in. We will
        send a security code to this number to set up your authentication method. This is a normal
        part of keeping you account secure.
      </p>
      <form
        noValidate
        onSubmit={(event) => {
          event.preventDefault()
          event.stopPropagation()
          void form.handleSubmit()
        }}
      >
        <form.Field name="phoneNumber">
          {(field) => {
            const errorMessage = getFieldErrorMessage(field.state.meta.errors)

            return (
              <Input
                autoComplete="tel"
                error={Boolean(errorMessage)}
                errorMessage={errorMessage}
                hint="For international numbers include a country code."
                label="Phone number"
                name={field.name}
                onBlur={field.handleBlur}
                onChange={(event: ChangeEvent<HTMLInputElement>) =>
                  field.handleChange(event.target.value)
                }
                type="tel"
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
    </>
  )
}
