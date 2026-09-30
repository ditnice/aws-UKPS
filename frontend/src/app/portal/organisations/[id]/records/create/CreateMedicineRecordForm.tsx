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

const developmentNameRequiredError = 'Enter development name'
const genericNameRequiredError = 'Enter generic name'
export const createRecordCommandSchema = z.object({
  organisationId: z.number(),
  developmentNames: z
    .array(z.string().trim().min(1, { message: developmentNameRequiredError }))
    .min(1, { message: developmentNameRequiredError })
    .refine((values) => new Set(values).size === values.length, {
      message: 'Development names must be distinct',
    }),
  brandedName: z.string().trim().nullable().optional(),
  genericNames: z
    .array(z.string().trim().min(1, { message: genericNameRequiredError }))
    .min(1, { message: genericNameRequiredError })
    .refine((values) => new Set(values).size === values.length, {
      message: 'Generic names must be distinct',
    }),
  recordTitle: z
    .string()
    .trim()
    .min(1, { message: 'Enter record title' })
    .max(100, { message: 'Record title cannot be greater than 100 characters' }),
})

type CreateMedicineRecordFormProps = {
  organisationId: number
}
const CreateMedicineRecordForm = ({ organisationId }: CreateMedicineRecordFormProps) => {
  const router = useRouter()
  const [error, setError] = useState<boolean>()
  const inputWidth = 'two-thirds' as const
  const defaultValues: CreateRecordCommand = {
    organisationId,
    developmentNames: [''],
    brandedName: '',
    genericNames: [''],
    recordTitle: '',
  }
  const form = useForm({
    defaultValues,
    onSubmit: async ({ value, formApi }) => {
      createRecordCommandSchema.parse(value)
      const { error, response } = await createRecord({ body: value })
      if (response?.ok) {
        router.push(`/portal/organisations/${organisationId}/records`)
      }

      setError(true)

      if (isValidationProblemDetails(error)) {
        formApi.setFieldMeta('developmentNames', updateFormApiErrors(error, 'DevelopmentNames'))
        formApi.setFieldMeta('brandedName', updateFormApiErrors(error, 'BrandedName'))
        formApi.setFieldMeta('genericNames', updateFormApiErrors(error, 'GenericNames'))
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
        form.handleSubmit()
      }}
    >
      {error && <ErrorState>{errorMessages.creatingNewRecordError}</ErrorState>}
      <form.Field name="developmentNames" mode="array">
        {(field) => (
          <ArrayInput
            labelPrefix={'Development name'}
            hint="Enter the name this medicine is known by in development (also called a synonym). This can include code names, historical names, abbreviations or alternate spellings."
            addItemLabel="Add additional development name"
            removeItemLabel="Remove Development Name"
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
            <>
              <Input
                error={Boolean(errorMessage)}
                errorMessage={errorMessage}
                label="Branded name (Optional)"
                name={field.name}
                onBlur={field.handleBlur}
                onChange={(event: ChangeEvent<HTMLInputElement>) =>
                  field.handleChange(event.target.value)
                }
                value={field.state.value?.toString()}
                width={inputWidth}
              />
            </>
          )
        }}
      </form.Field>
      <form.Field name="genericNames" mode="array">
        {(field) => (
          <ArrayInput
            labelPrefix={'Generic name'}
            hint="Enter the standard, non-proprietary name for the active substances. For example, adalimumab, atorvastain."
            addItemLabel="Add additional active substance"
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
            <>
              <Textarea
                error={Boolean(errorMessage)}
                errorMessage={errorMessage}
                label="Record title"
                name={field.name}
                hint="Enter a title to help you to identify this record. You can enter up to 100 characters."
                onBlur={field.handleBlur}
                onChange={(event: ChangeEvent<HTMLInputElement>) =>
                  field.handleChange(event.target.value)
                }
                value={field.state.value?.toString()}
                width={inputWidth}
              />
            </>
          )
        }}
      </form.Field>
      <Button type="submit" variant="cta">
        Save and continue
      </Button>
    </form>
  )
}

export default CreateMedicineRecordForm
