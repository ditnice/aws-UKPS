'use client'

import { revalidateLogic, useForm } from '@tanstack/react-form'
import { isValidPhoneNumber } from 'libphonenumber-js/max'
import { useRouter } from 'next/navigation'
import { useState, type ChangeEvent } from 'react'
import { z } from 'zod'

import { initiateUpdateSmsMultiFactorAuthentication } from '@/client/generated/sdk.gen'
import { BackLink } from '@/components/BackLink/BackLink'
import { Button } from '@/components/Button/Button'
import { Input } from '@/components/Input/Input'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { errorMessages } from '@/lib/form/errorMessages'
import { getFieldErrorMessage } from '@/lib/form/getFieldErrorMessage'

import { buildMyDetailsHref } from '../../navigation'

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

  const form = useForm({
    defaultValues: { phoneNumber: '' } satisfies FormValues,
    validationLogic: revalidateLogic({ mode: 'submit', modeAfterSubmission: 'blur' }),
    validators: { onDynamic: phoneSchema },
    onSubmit: async ({ value, formApi }) => {
      const { phoneNumber } = phoneSchema.parse(value)
      setSubmitError(null)

      try {
        const result = await initiateUpdateSmsMultiFactorAuthentication({
          body: { phoneNumber },
          credentials: 'include',
        })

        if (!result.error) {
          try {
            sessionStorage.setItem('changeSmsMfa', JSON.stringify({ phoneNumber }))
          } catch {}
          router.push('/portal/user/me/update-sms-mfa/check-your-phone')
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
          result.error.detail ?? 'We could not update your phone number. Try again later.',
        )
      } catch {
        setSubmitError('We could not update your phone number. Try again later.')
      }
    },
  })

  return (
    <>
      <PageHeader
        backLink={<BackLink href={buildMyDetailsHref()} />}
        heading="Change phone number"
      />
      <p>
        Text messages will be sent to this phone to verify your identity when you sign in. We will
        send a security code to this number to confirm the change.
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
