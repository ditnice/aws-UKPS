'use client'

import { revalidateLogic, useForm } from '@tanstack/react-form'
import { useRouter } from 'next/navigation'
import { useState, type ChangeEvent } from 'react'
import { z } from 'zod'

import { getFieldErrorMessage } from '@/app/common/form/getFieldErrorMessage'
import { patchUsersByUserId } from '@/client/generated'
import { UpdateUserDetailsCommand, ValidationProblemDetails } from '@/client/generated/types.gen'
import { Button, ButtonGroup } from '@/components/Button/Button'
import { Input } from '@/components/Input/Input'
import { ErrorState } from '@/components/Paceholder/ErrorState'

const isValidationProblemDetails = (value: unknown): value is ValidationProblemDetails => {
  if (!value || typeof value !== 'object') {
    return false
  }

  const candidate = value as Record<string, unknown>

  if (
    !candidate.errors ||
    typeof candidate.errors !== 'object' ||
    Array.isArray(candidate.errors)
  ) {
    return false
  }

  return Object.values(candidate.errors).every(
    (error) => Array.isArray(error) && error.every((message) => typeof message === 'string'),
  )
}

const setErrors = (
  validationProblemDetails: ValidationProblemDetails,
  key: string,
): import('@tanstack/react-form').Updater<import('@tanstack/react-form').AnyFieldLikeMetaBase> => {
  return (meta) => ({
    ...meta,
    errorMap: {
      ...meta.errorMap,
      onSubmit: validationProblemDetails.errors[key],
    },
  })
}

const EditDetails = z.object({
  fullName: z.string().trim().min(1, 'Enter your full name'),
  workEmail: z
    .string()
    .trim()
    .min(1, 'Enter your work email address')
    .pipe(z.email('Enter an email address in the correct format, like name@example.com')),
  workTelephone: z.string().trim().min(1, 'Enter your phone number'),
})

type EditDetailsFormProps = { userId: number; initialValues: UpdateUserDetailsCommand }
export function EditDetailsForm({ userId, initialValues }: EditDetailsFormProps) {
  const [error, setError] = useState(false)
  const router = useRouter()
  const form = useForm({
    defaultValues: {
      fullName: initialValues.fullName, // These default values will be from their existing account
      workEmail: initialValues.workEmail,
      workTelephone: initialValues.workTelephone,
    } satisfies UpdateUserDetailsCommand,
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
      const response = await patchUsersByUserId({ path: { userId }, body: data })

      if (response.response?.ok) {
        router.push('/portal/user/me')
        return
      }

      setError(true)

      if (response && isValidationProblemDetails(response.error)) {
        formApi.setFieldMeta('fullName', setErrors(response.error, 'FullName'))
        formApi.setFieldMeta('workEmail', setErrors(response.error, 'WorkEmail'))
        formApi.setFieldMeta('workTelephone', setErrors(response.error, 'WorkTelephone'))
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
      {error && <ErrorState>An Error Occurred when trying to update the user.</ErrorState>}
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
              hint="For international numbers include the country code."
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
  )
}
