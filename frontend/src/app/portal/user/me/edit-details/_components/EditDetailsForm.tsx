'use client'

import { revalidateLogic, useForm } from '@tanstack/react-form'
import { isValidPhoneNumber } from 'libphonenumber-js/max'
import { useRouter } from 'next/navigation'
import { useState, type ChangeEvent } from 'react'
import { z } from 'zod'

import { patchUsersByUserId } from '@/client/generated'
import { UpdateUserDetailsCommand } from '@/client/generated/types.gen'
import { Button, ButtonGroup } from '@/components/Button/Button'
import { Input } from '@/components/Input/Input'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { errorMessages } from '@/lib/form/errorMessages'
import { updateFormApiErrors } from '@/lib/form/formErrorHandling'
import { getFieldErrorMessage } from '@/lib/form/getFieldErrorMessage'
import { isValidationProblemDetails } from '@/lib/responses/typeGuards'

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

export type EditDetailsFormProps = { userId: number; initialValues: UpdateUserDetailsCommand }
export function EditDetailsForm({ userId, initialValues }: EditDetailsFormProps) {
  const [error, setError] = useState(false)
  const router = useRouter()
  const form = useForm({
    defaultValues: {
      fullName: initialValues.fullName,
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
        formApi.setFieldMeta('fullName', updateFormApiErrors(response.error, 'FullName'))
        formApi.setFieldMeta('workEmail', updateFormApiErrors(response.error, 'WorkEmail'))
        formApi.setFieldMeta('workTelephone', updateFormApiErrors(response.error, 'WorkTelephone'))
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
