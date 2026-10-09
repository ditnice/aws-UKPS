'use client'

import { revalidateLogic, useForm } from '@tanstack/react-form'
import { useRouter } from 'next/navigation'
import { ChangeEvent, useState } from 'react'
import z from 'zod'

import { createRecord, CreateRecordCommand } from '@/client/generated'
import { Button } from '@/components/Button/Button'
import { Input } from '@/components/Input/Input'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { Textarea } from '@/components/Textarea/Textarea'
import { errorMessages } from '@/lib/form/errorMessages'
import { updateFormApiErrors } from '@/lib/form/formErrorHandling'
import { getFieldErrorMessage } from '@/lib/form/getFieldErrorMessage'
import { isValidationProblemDetails } from '@/lib/responses/typeGuards'

import ArrayInput from './ArrayInput'

const inputWidth = 'two-thirds'

const isDistinctIgnoringCase = (values: string[]) =>
  new Set(values.map((value) => value.toLowerCase())).size === values.length

export const createRecordCommandSchema = z.object({
  organisationId: z.number(),
  companyCode: z.string().trim().min(1, errorMessages.companyCodeRequired),
  // Other identifiers are optional, so blank entries are dropped rather than rejected.
  otherIdentifiers: z
    .array(z.string())
    .transform((values) => values.map((value) => value.trim()).filter(Boolean)),
  brandedName: z
    .string()
    .nullish()
    .transform((value) => value?.trim() || null),
  genericNames: z
    .array(z.string().trim().min(1, errorMessages.genericNameRequired))
    .min(1, errorMessages.genericNameRequired)
    .refine(isDistinctIgnoringCase, errorMessages.genericNamesDistinct),
  recordTitle: z
    .string()
    .trim()
    .min(1, errorMessages.recordTitleRequired)
    .max(100, errorMessages.recordTitleTooLong),
})

type CreateMedicineRecordFormProps = {
  organisationId: number
}
const CreateMedicineRecordForm = ({ organisationId }: CreateMedicineRecordFormProps) => {
  const router = useRouter()
  const [error, setError] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const defaultValues: CreateRecordCommand = {
    organisationId,
    companyCode: '',
    otherIdentifiers: [''],
    brandedName: '',
    genericNames: [''],
    recordTitle: '',
  }
  const form = useForm({
    defaultValues,
    onSubmit: async ({ value, formApi }) => {
      setError(false)
      setIsSubmitting(true)
      const data = createRecordCommandSchema.parse(value)
      const { error, response } = await createRecord({ body: data })
      if (response?.ok) {
        router.push(`/portal/organisations/${organisationId}/records`)
        return
      }

      setIsSubmitting(false)
      setError(true)

      if (isValidationProblemDetails(error)) {
        formApi.setFieldMeta('companyCode', updateFormApiErrors(error, 'CompanyCode'))
        formApi.setFieldMeta('brandedName', updateFormApiErrors(error, 'BrandedName'))
        formApi.setFieldMeta('genericNames', updateFormApiErrors(error, 'GenericNames'))
        formApi.setFieldMeta('otherIdentifiers', updateFormApiErrors(error, 'OtherIdentifiers'))
        formApi.setFieldMeta('recordTitle', updateFormApiErrors(error, 'RecordTitle'))
      }
    },
    validationLogic: revalidateLogic({
      mode: 'submit',
      modeAfterSubmission: 'blur',
    }),
    validators: {
      onDynamic: createRecordCommandSchema,
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
      {error && <ErrorState>{errorMessages.creatingNewRecordError}</ErrorState>}
      <form.Field name="companyCode">
        {(field) => {
          const errorMessage = getFieldErrorMessage(field.state.meta.errors)
          return (
            <Input
              error={Boolean(errorMessage)}
              errorMessage={errorMessage}
              label="Company code"
              hint="Enter the primary internal code or working name your company uses for this product seeking market access."
              name={field.name}
              onBlur={field.handleBlur}
              onChange={(event: ChangeEvent<HTMLInputElement>) =>
                field.handleChange(event.target.value)
              }
              value={field.state.value}
              width={inputWidth}
            />
          )
        }}
      </form.Field>
      <form.Field name="otherIdentifiers" mode="array">
        {(field) => (
          <ArrayInput
            labelPrefix="Other names and identifiers (optional)"
            hint="Enter any other development names, synonyms, or identifiers that you want to associate with this product."
            addItemLabel="Add another identifier"
            removeItemLabel="Remove identifier"
            width={inputWidth}
            field={field}
            getSubfield={(name, renderInputs) => (
              <form.Field name={name}>{renderInputs}</form.Field>
            )}
          />
        )}
      </form.Field>
      <form.Field name="brandedName">
        {(field) => {
          const errorMessage = getFieldErrorMessage(field.state.meta.errors)
          return (
            <Input
              error={Boolean(errorMessage)}
              errorMessage={errorMessage}
              label="Branded name (optional)"
              hint="Enter the name this medicine is sold under. For example, Humira, Lipitor."
              name={field.name}
              onBlur={field.handleBlur}
              onChange={(event: ChangeEvent<HTMLInputElement>) =>
                field.handleChange(event.target.value)
              }
              value={field.state.value ?? ''}
              width={inputWidth}
            />
          )
        }}
      </form.Field>
      <form.Field name="genericNames" mode="array">
        {(field) => (
          <ArrayInput
            labelPrefix="Generic name"
            hint="Enter the standard, non-proprietary name for the active substances. For example, adalimumab, atorvastatin."
            addItemLabel="Add another active substance"
            removeItemLabel="Remove active substance"
            width={inputWidth}
            field={field}
            getSubfield={(name, renderInputs) => (
              <form.Field name={name}>{renderInputs}</form.Field>
            )}
          />
        )}
      </form.Field>
      <form.Field name="recordTitle">
        {(field) => {
          const errorMessage = getFieldErrorMessage(field.state.meta.errors)
          return (
            <Textarea
              error={Boolean(errorMessage)}
              errorMessage={errorMessage}
              label="Record title"
              name={field.name}
              hint="Enter a title to help you to identify this record. You can enter up to 100 characters."
              onBlur={field.handleBlur}
              onChange={(event: ChangeEvent<HTMLTextAreaElement>) =>
                field.handleChange(event.target.value)
              }
              value={field.state.value}
              width={inputWidth}
            />
          )
        }}
      </form.Field>
      <Button disabled={isSubmitting} type="submit" variant="cta">
        Save and continue
      </Button>
    </form>
  )
}

export default CreateMedicineRecordForm
