'use client'

import { useRouter } from 'next/navigation'
import { useState, useTransition } from 'react'

import { Button } from '@nice-digital/nds-button'
import { FormGroup } from '@nice-digital/nds-form-group'
import { Input } from '@nice-digital/nds-input'
import { Textarea } from '@nice-digital/nds-textarea'

import {
  EMAIL_FORMAT_ERROR_MESSAGE,
  PHONE_FORMAT_ERROR_MESSAGE,
  COMPANY_NAME_REQUIRED_ERROR_MESSAGE,
  ADDRESS_REQUIRED_ERROR_MESSAGE,
  EMAIL_REQUIRED_ERROR_MESSAGE,
  PHONE_REQUIRED_ERROR_MESSAGE,
} from '@/app/common/ErrorMessages'
import { isValidEmail, isValidPhoneNumber } from '@/app/common/RegEx'
import type { UpdateOrganisationDetailsDto } from '@/client/generated/types.gen'

import { updateOrganisationDetailsAction } from '../_actions/updateOrganisationDetails'

import type { ChangeEvent, SubmitEvent } from 'react'

export type OrganisationDetailsFormErrors = Partial<
  Record<keyof UpdateOrganisationDetailsDto, string>
>

export type EditOrganisationDetailsFormProps = UpdateOrganisationDetailsDto & {
  organisationId: number
}

function validateOrganisationDetailsForm(
  values: UpdateOrganisationDetailsDto,
): OrganisationDetailsFormErrors {
  const errors: OrganisationDetailsFormErrors = {}

  if (!values.organisationName.trim()) {
    errors.organisationName = COMPANY_NAME_REQUIRED_ERROR_MESSAGE
  }

  if (!values.headOfficeAddress.trim()) {
    errors.headOfficeAddress = ADDRESS_REQUIRED_ERROR_MESSAGE
  }

  if (!values.headOfficeEmail.trim()) {
    errors.headOfficeEmail = EMAIL_REQUIRED_ERROR_MESSAGE
  } else if (!isValidEmail(values.headOfficeEmail)) {
    errors.headOfficeEmail = EMAIL_FORMAT_ERROR_MESSAGE
  }

  if (!values.headOfficeTelephone.trim()) {
    errors.headOfficeTelephone = PHONE_REQUIRED_ERROR_MESSAGE
  } else if (!isValidPhoneNumber(values.headOfficeTelephone)) {
    errors.headOfficeTelephone = PHONE_FORMAT_ERROR_MESSAGE
  }

  return errors
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
  const [values, setValues] = useState<UpdateOrganisationDetailsDto>({
    organisationName,
    headOfficeAddress,
    headOfficeEmail,
    headOfficeTelephone,
  })
  const [errors, setErrors] = useState<OrganisationDetailsFormErrors>({})
  const [submitError, setSubmitError] = useState<string>()

  function handleChange(field: keyof UpdateOrganisationDetailsDto) {
    return (event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
      setValues((current) => ({ ...current, [field]: event.target.value }))
    }
  }

  function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault()
    setSubmitError(undefined)

    const validationErrors = validateOrganisationDetailsForm(values)
    setErrors(validationErrors)

    if (Object.keys(validationErrors).length > 0) {
      return
    }

    startTransition(async () => {
      const result = await updateOrganisationDetailsAction(organisationId, values)

      if (result.status === 'error') {
        setSubmitError(result.message)
        return
      }

      router.push(`/portal/organisations/${organisationId}`)
    })
  }

  return (
    <form noValidate onSubmit={handleSubmit}>
      {submitError && <p role="alert">{submitError}</p>}

      <FormGroup>
        <Input
          label="Company name"
          name="organisationName"
          value={values.organisationName}
          onChange={handleChange('organisationName')}
          error={!!errors.organisationName}
          errorMessage={errors.organisationName}
          required
        />
        <Textarea
          label="Company address"
          name="headOfficeAddress"
          value={values.headOfficeAddress}
          onChange={handleChange('headOfficeAddress')}
          error={!!errors.headOfficeAddress}
          errorMessage={errors.headOfficeAddress}
          required
        />
        <Input
          label="Company email address"
          name="headOfficeEmail"
          type="email"
          value={values.headOfficeEmail}
          onChange={handleChange('headOfficeEmail')}
          error={!!errors.headOfficeEmail}
          errorMessage={errors.headOfficeEmail}
          required
        />
        <Input
          label="Company phone number"
          name="headOfficeTelephone"
          type="tel"
          value={values.headOfficeTelephone}
          onChange={handleChange('headOfficeTelephone')}
          error={!!errors.headOfficeTelephone}
          errorMessage={errors.headOfficeTelephone}
          required
        />
      </FormGroup>

      <Button buttonType={Button.types.submit} disabled={isPending}>
        {isPending ? 'Saving...' : 'Save changes'}
      </Button>
    </form>
  )
}
