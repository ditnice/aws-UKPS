'use client'

import { revalidateLogic, useForm } from '@tanstack/react-form'
import { useRouter } from 'next/navigation'
import { useState, type ChangeEvent } from 'react'

import { patchUsersByUserId } from '@/client/generated'
import { UpdateUserDetailsCommand } from '@/client/generated/types.gen'
import { Button, ButtonGroup } from '@/components/Button/Button'
import { Input } from '@/components/Input/Input'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { buildSignInHref } from '@/lib/auth/routing'
import { errorMessages } from '@/lib/form/errorMessages'
import { updateFormApiErrors } from '@/lib/form/formErrorHandling'
import { getFieldErrorMessage } from '@/lib/form/getFieldErrorMessage'
import { isValidationProblemDetails } from '@/lib/responses/typeGuards'

import {
  getStepUpMaxAge,
  isReauthenticationRequiredError,
} from '../../../../../../lib/auth/authenticationHelpers'
import { buildEditDetailsHref } from '../navigation'

import { ReauthenticationRequiredBanner } from './ReauthenticationRequiredBanner'
import { EditDetails, storeUserDetails, useStoredUserDetails } from './userDetailsStore'

export type EditDetailsFormProps = {
  userId: number
  initialValues: UpdateUserDetailsCommand
  sessionKey: string | undefined
}
export function EditDetailsForm({ userId, initialValues, sessionKey }: EditDetailsFormProps) {
  const [error, setError] = useState(false)
  const [reauthenticationRequired, setReauthenticationRequired] = useState<
    undefined | { maxAge: number | null }
  >(undefined)
  const router = useRouter()
  const sessionData = useStoredUserDetails(sessionKey)
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
        router.push('/portal/user/me?updated=true')
        return
      }

      if (!response) {
        return
      }
      if (isReauthenticationRequiredError(error)) {
        const maxAge = getStepUpMaxAge(response)
        setReauthenticationRequired({ maxAge })
        return
      }
      if (isValidationProblemDetails(error)) {
        formApi.setFieldMeta('fullName', updateFormApiErrors(error, 'FullName'))
        formApi.setFieldMeta('workEmail', updateFormApiErrors(error, 'WorkEmail'))
        formApi.setFieldMeta('workTelephone', updateFormApiErrors(error, 'WorkTelephone'))
      }

      setError(true)
    },
  })

  const triggerReauthenticationFlow = () => {
    const sessionKey = storeUserDetails(form.state.values)
    router.push(buildSignInHref(buildEditDetailsHref({ sessionKey })))
  }
  return (
    <>
      {reauthenticationRequired && (
        <ReauthenticationRequiredBanner
          maxAge={reauthenticationRequired.maxAge}
          reauthenticationRequested={triggerReauthenticationFlow}
        />
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
