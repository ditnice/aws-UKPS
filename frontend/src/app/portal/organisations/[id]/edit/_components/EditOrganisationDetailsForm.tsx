'use client'

import { revalidateLogic, useForm } from '@tanstack/react-form'
import { isValidPhoneNumber } from 'libphonenumber-js/max'
import { useRouter } from 'next/navigation'
import { useState } from 'react'
import { z } from 'zod'

import { FormGroup } from '@nice-digital/nds-form-group'
import { Textarea } from '@nice-digital/nds-textarea'

import type { UpdateOrganisationDetailsDto } from '@/client/generated/types.gen'
import { Button } from '@/components/Button/Button'
import { Input } from '@/components/Input/Input'
import {
  EMAIL_FORMAT_ERROR_MESSAGE,
  PHONE_FORMAT_ERROR_MESSAGE,
  COMPANY_NAME_REQUIRED_ERROR_MESSAGE,
  ADDRESS_REQUIRED_ERROR_MESSAGE,
  ORGANISATION_EMAIL_REQUIRED_ERROR_MESSAGE,
  PHONE_REQUIRED_ERROR_MESSAGE,
} from '@/lib/form/errorMessages'
import { getFieldErrorMessage } from '@/lib/form/getFieldErrorMessage'

import { updateOrganisationDetailsAction } from '../_actions/updateOrganisationDetails'

import type { ChangeEvent } from 'react'

const editOrganisationDetailsSchema = z.object({
  organisationName: z.string().trim().min(1, COMPANY_NAME_REQUIRED_ERROR_MESSAGE),
  headOfficeAddress: z.string().trim().min(1, ADDRESS_REQUIRED_ERROR_MESSAGE),
  headOfficeEmail: z
    .string()
    .trim()
    .min(1, ORGANISATION_EMAIL_REQUIRED_ERROR_MESSAGE)
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
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [formError, setFormError] = useState<string>()

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
    onSubmit: async ({ value }) => {
      setFormError(undefined)
      setIsSubmitting(true)

      const values = editOrganisationDetailsSchema.parse(value)
      const response = await updateOrganisationDetailsAction(organisationId, values)

      if (response.status === 'error') {
        setIsSubmitting(false)
        setFormError(response.message)
        return
      }

      router.push(`/portal/organisations/${organisationId}`)
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
      {formError && <p role="alert">{formError}</p>}

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

      <Button buttonType="submit" disabled={isSubmitting} variant="cta">
        {isSubmitting ? 'Saving...' : 'Save changes'}
      </Button>

      <Button buttonType="button" variant="secondary" onClick={() => router.back()}>
        Cancel
      </Button>
    </form>
  )
}
