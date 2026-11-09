'use client'

import { revalidateLogic, useForm } from '@tanstack/react-form'
import { isValidPhoneNumber } from 'libphonenumber-js/max'
import { redirect, useRouter } from 'next/navigation'
import { useState, type ChangeEvent } from 'react'
import { z } from 'zod'

import { patchUsersByUserId } from '@/client/generated'
import { UpdateUserDetailsCommand } from '@/client/generated/types.gen'
import { Alert } from '@/components/Alert/Alert'
import { Button, ButtonGroup } from '@/components/Button/Button'
import { Input } from '@/components/Input/Input'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { buildSignInHref } from '@/lib/auth/routing'
import { errorMessages } from '@/lib/form/errorMessages'
import { updateFormApiErrors } from '@/lib/form/formErrorHandling'
import { getFieldErrorMessage } from '@/lib/form/getFieldErrorMessage'
import { isValidationProblemDetails } from '@/lib/responses/typeGuards'

const retrieveUserDetails = (key: string | undefined): UpdateUserDetailsCommand | undefined => {
  if (!key) return undefined
  const data = sessionStorage.getItem(key)
  return data ? (JSON.parse(data) as UpdateUserDetailsCommand) : undefined
}

const storeUserDetails = (userDetailsCommand: UpdateUserDetailsCommand) => {
  const key = crypto.randomUUID()
  sessionStorage.setItem(key, JSON.stringify(userDetailsCommand))
  return key
}

const clearUserDetailsSessionData = (key: string | undefined) => {
  if (key) {
    sessionStorage.removeItem(key)
  }
}

const EditDetails = z.object({
  fullName: z.string().trim().min(1, errorMessages.personalFullNameRequired),
  workEmail: z
    .string()
    .trim()
    .min(1, errorMessages.workEmailRequired)
    .pipe(z.email(errorMessages.emailFormat)),
  workTelephone: z
    .string()
    .trim()
    .min(1, errorMessages.personalPhoneRequired)
    .refine((value) => isValidPhoneNumber(value, 'GB'), errorMessages.phoneFormat),
})

export type EditDetailsFormProps = {
  userId: number
  initialValues: UpdateUserDetailsCommand
  sessionKey: string | undefined
}
export function EditDetailsForm({ userId, initialValues, sessionKey }: EditDetailsFormProps) {
  const [error, setError] = useState(false)
  const [reauthenticationRequired, setReauthenticationRequired] = useState(false)

  const router = useRouter()
  const sessionData = retrieveUserDetails(sessionKey)
  const form = useForm({
    defaultValues: sessionData ?? (initialValues satisfies UpdateUserDetailsCommand),
    validationLogic: revalidateLogic({
      mode: 'submit',
      modeAfterSubmission: 'blur',
    }),
    validators: {
      onDynamic: EditDetails,
    },
    onSubmit: async ({ value, formApi }) => {
      setError(false)

      const data = EditDetails.parse(value)
      const { response, error } = await patchUsersByUserId({ path: { userId }, body: data })

      if (response?.ok) {
        clearUserDetailsSessionData(sessionKey)
        router.push('/portal/user/me?updated=true')
        return
      }

      setError(true)

      if (error && 'code' in error && error.code == 'ReauthenticationRequired') {
        setReauthenticationRequired(true)
      }
      if (response && isValidationProblemDetails(error)) {
        formApi.setFieldMeta('fullName', updateFormApiErrors(error, 'FullName'))
        formApi.setFieldMeta('workEmail', updateFormApiErrors(error, 'WorkEmail'))
        formApi.setFieldMeta('workTelephone', updateFormApiErrors(error, 'WorkTelephone'))
      }
    },
  })

  const triggerReauthenticationFlow = () => {
    const key = storeUserDetails(form.state.values)
    redirect(buildSignInHref(`/portal/user/me/edit-details?sessionKey=${key}`))
  }
  return (
    <>
      {reauthenticationRequired && (
        <Alert type="error">
          <h2>You need to sign in again to change your email address</h2>
          <p>
            For your security, we need to check it&apos;s you before we can change your email
            address.
          </p>
          <ol>
            <li>Select &quot;Sign in again&quot;.</li>
            <li>Sign in on the next page.</li>
            <li>
              You&apos;ll come back to this form with your details still filled in. Select
              &quot;Save&quot;.
            </li>
          </ol>
          <Button type="button" onClick={triggerReauthenticationFlow}>
            Sign in again
          </Button>
        </Alert>
      )}

      <form
        noValidate
        onSubmit={(event) => {
          event.preventDefault()
          event.stopPropagation()
          void form.handleSubmit()
        }}
      >
        {error && <ErrorState>{errorMessages.updatingUserDetailsError}</ErrorState>}
        <form.Field name="fullName">
          {(field) => {
            const errorMessage = getFieldErrorMessage(field.state.meta.errors)
            return (
              <Input
                autoComplete="name"
                error={Boolean(errorMessage)}
                errorMessage={errorMessage}
                label="Full name"
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
        <form.Field name="workEmail">
          {(field) => {
            const errorMessage = getFieldErrorMessage(field.state.meta.errors)
            return (
              <Input
                autoComplete="email"
                error={Boolean(errorMessage)}
                errorMessage={errorMessage}
                label="Work email address"
                name={field.name}
                onChange={(event: ChangeEvent<HTMLInputElement>) =>
                  field.handleChange(event.target.value)
                }
                onBlur={field.handleBlur}
                type="email"
                value={field.state.value}
                width="one-third"
              />
            )
          }}
        </form.Field>
        <form.Field name="workTelephone">
          {(field) => {
            const errorMessage = getFieldErrorMessage(field.state.meta.errors)

            return (
              <Input
                autoComplete="tel"
                error={Boolean(errorMessage)}
                errorMessage={errorMessage}
                label="Contact number"
                name={field.name}
                onBlur={field.handleBlur}
                onChange={(event: ChangeEvent<HTMLInputElement>) =>
                  field.handleChange(event.target.value)
                }
                hint="This number will be visible to horizon scanning organisations. For international numbers include the country code."
                type="tel"
                width="one-third"
                value={field.state.value}
              />
            )
          }}
        </form.Field>
        <ButtonGroup>
          <Button type="submit" variant="cta">
            Save
          </Button>
          <Button onClick={() => router.back()} variant="secondary">
            Cancel
          </Button>
        </ButtonGroup>
      </form>
    </>
  )
}
