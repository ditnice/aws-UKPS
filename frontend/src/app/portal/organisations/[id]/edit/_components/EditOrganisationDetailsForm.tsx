'use client'

import { revalidateLogic, useForm } from '@tanstack/react-form'
import { isValidPhoneNumber } from 'libphonenumber-js/min'
import { useRouter } from 'next/navigation'
import { useState, useTransition } from 'react'
import { z } from 'zod'

import { Button } from '@nice-digital/nds-button'
import { FormGroup } from '@nice-digital/nds-form-group'
import { Textarea } from '@nice-digital/nds-textarea'

import {
  EMAIL_FORMAT_ERROR_MESSAGE,
  PHONE_FORMAT_ERROR_MESSAGE,
  COMPANY_NAME_REQUIRED_ERROR_MESSAGE,
  ADDRESS_REQUIRED_ERROR_MESSAGE,
  EMAIL_REQUIRED_ERROR_MESSAGE,
  PHONE_REQUIRED_ERROR_MESSAGE,
} from '@/app/common/form/ErrorMessages'
import { getFieldErrorMessage } from '@/app/common/form/getFieldErrorMessage'
import type { UpdateOrganisationDetailsDto } from '@/client/generated/types.gen'
import { Input } from '@/components/Input/Input'

import { updateOrganisationDetailsAction } from '../_actions/updateOrganisationDetails'

import type { ChangeEvent } from 'react'

const editOrganisationDetailsSchema = z.object({
  organisationName: z.string().trim().min(1, COMPANY_NAME_REQUIRED_ERROR_MESSAGE),
  headOfficeAddress: z.string().trim().min(1, ADDRESS_REQUIRED_ERROR_MESSAGE),
  headOfficeEmail: z
    .string()
    .trim()
    .min(1, EMAIL_REQUIRED_ERROR_MESSAGE)
    .pipe(z.email(EMAIL_FORMAT_ERROR_MESSAGE)),
  headOfficeTelephone: z
    .string()
    .trim()
    .min(1, PHONE_REQUIRED_ERROR_MESSAGE)
    .refine((value) => isValidPhoneNumber(value, 'GB'), PHONE_FORMAT_ERROR_MESSAGE),
})

type EditOrganisationDetailsFormValues = z.input<typeof editOrganisationDetailsSchema>

export type EditOrganisationDetailsFormProps = UpdateOrganisationDetailsDto & {
  organisationId: number
}

export function EditOrganisationDetailsForm({
  organisationId,
  organisationName,
  headOfficeAddress,
  headOfficeEmail,
  headOfficeTelephone,
}: EditOrganisationDetailsFormProps) {
  const router = useRouter()
  const [isPending, startTransition] = useTransition()
  const [submitError, setSubmitError] = useState<string>()

  const form = useForm({
    defaultValues: {
      organisationName,
      headOfficeAddress,
      headOfficeEmail,
      headOfficeTelephone,
    } satisfies EditOrganisationDetailsFormValues,
    validationLogic: revalidateLogic({
      mode: 'submit',
      modeAfterSubmission: 'blur',
    }),
    validators: {
      onDynamic: editOrganisationDetailsSchema,
    },
    onSubmit: ({ value }) => {
      setSubmitError(undefined)
      const values = editOrganisationDetailsSchema.parse(value)

      startTransition(async () => {
        const result = await updateOrganisationDetailsAction(organisationId, values)

        if (result.status === 'error') {
          setSubmitError(result.message)
          return
        }

        router.push(`/portal/organisations/${organisationId}`)
      })
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
      {submitError && <p role="alert">{submitError}</p>}

      <FormGroup>
        <form.Field name="organisationName">
          {(field) => {
            const errorMessage = getFieldErrorMessage(field.state.meta.errors)

            return (
              <Input
                label="Company name"
                name={field.name}
                value={field.state.value}
                onBlur={field.handleBlur}
                onChange={(event: ChangeEvent<HTMLInputElement>) =>
                  field.handleChange(event.target.value)
                }
                error={Boolean(errorMessage)}
                errorMessage={errorMessage}
                required
                width="one-half"
              />
            )
          }}
        </form.Field>

        <form.Field name="headOfficeAddress">
          {(field) => {
            const errorMessage = getFieldErrorMessage(field.state.meta.errors)

            return (
              <Textarea
                label="Company address"
                name={field.name}
                value={field.state.value}
                onBlur={field.handleBlur}
                onChange={(event: ChangeEvent<HTMLTextAreaElement>) =>
                  field.handleChange(event.target.value)
                }
                error={Boolean(errorMessage)}
                errorMessage={errorMessage}
                required
              />
            )
          }}
        </form.Field>

        <form.Field name="headOfficeEmail">
          {(field) => {
            const errorMessage = getFieldErrorMessage(field.state.meta.errors)

            return (
              <Input
                label="Company email address"
                name={field.name}
                type="email"
                value={field.state.value}
                onBlur={field.handleBlur}
                onChange={(event: ChangeEvent<HTMLInputElement>) =>
                  field.handleChange(event.target.value)
                }
                error={Boolean(errorMessage)}
                errorMessage={errorMessage}
                required
                width="one-half"
              />
            )
          }}
        </form.Field>

        <form.Field name="headOfficeTelephone">
          {(field) => {
            const errorMessage = getFieldErrorMessage(field.state.meta.errors)

            return (
              <Input
                label="Company phone number"
                name={field.name}
                hint="For international numbers include the country code. For example +1 555-123-4567."
                type="tel"
                value={field.state.value}
                onBlur={field.handleBlur}
                onChange={(event: ChangeEvent<HTMLInputElement>) =>
                  field.handleChange(event.target.value)
                }
                error={Boolean(errorMessage)}
                errorMessage={errorMessage}
                required
                width="one-half"
              />
            )
          }}
        </form.Field>
      </FormGroup>

      <Button buttonType="submit" disabled={isPending} variant="cta">
        {isPending ? 'Saving...' : 'Save changes'}
      </Button>

      <Button buttonType="button" variant="secondary" onClick={() => router.back()}>
        Cancel
      </Button>
    </form>
  )
}
