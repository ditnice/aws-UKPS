'use client'

import { revalidateLogic, useForm } from '@tanstack/react-form'
import { isValidPhoneNumber } from 'libphonenumber-js/max'
import { useRouter } from 'next/navigation'
import { ChangeEvent, useEffect, useState } from 'react'
import { z } from 'zod'

import {
  getOrganisationsPublicOptions,
  OrganisationListDto,
  postUsersRegister,
} from '@/client/generated'
import { Button } from '@/components/Button/Button'
import { Input } from '@/components/Input/Input'
import { Select, SelectOption } from '@/components/Select/Select'
import { errorMessages } from '@/lib/form/errorMessages'
import { getFieldErrorMessage } from '@/lib/form/getFieldErrorMessage'

import styles from './RegistrationRequestForm.module.scss'

const RegistrationRequest = z.object({
  organisationId: z.number().min(1, errorMessages.organisationRequired),
  fullName: z.string().trim().min(1, errorMessages.personalFullNameRequired),
  workEmail: z
    .string()
    .trim()
    .min(1, errorMessages.organisationEmailRequired)
    .pipe(z.email(errorMessages.emailFormat)),
  phoneNumber: z
    .string()
    .trim()
    .min(1, errorMessages.personalPhoneRequired)
    .refine((value) => isValidPhoneNumber(value, 'GB'), errorMessages.phoneFormat),
})

type RegistrationRequestValues = z.input<typeof RegistrationRequest>

export function RegistrationRequestForm() {
  const [organisations, setOrganisations] = useState<OrganisationListDto[]>([])
  useEffect(() => {
    const fetchOrganisations = async () => {
      const response = await getOrganisationsPublicOptions()
      if (response.data) {
        setOrganisations(response.data)
      }
    }
    fetchOrganisations()
  }, [])
  const router = useRouter()
  const form = useForm({
    defaultValues: {
      organisationId: 0,
      fullName: '',
      workEmail: '',
      phoneNumber: '',
    } satisfies RegistrationRequestValues,
    validationLogic: revalidateLogic({
      mode: 'submit',
      modeAfterSubmission: 'blur',
    }),
    validators: {
      onDynamic: RegistrationRequest,
    },
    onSubmit: async ({ value }) => {
      RegistrationRequest.parse(value)

      const response = await postUsersRegister({
        body: {
          fullName: value.fullName,
          workEmail: value.workEmail,
          phoneNumber: value.phoneNumber,
          organisationId: value.organisationId,
        },
      })
      if (response.data) {
        router.push(`/portal/register/request-submitted/${response.data.id}`)
        // TODO URP-312: Add the email being sent to request access to UKPS
      }
    },
  })

  return (
    <>
      <form
        noValidate
        onSubmit={(event) => {
          event.preventDefault()
          event.stopPropagation()
          void form.handleSubmit()
        }}
      >
        <form.Field name="organisationId">
          {(field) => {
            const errorMessage = getFieldErrorMessage(field.state.meta.errors)
            return (
              <>
                <Select
                  defaultValue="choose"
                  label="Select the organisation you are requesting access for"
                  name="organisation"
                  width="one-third"
                  error={Boolean(errorMessage)}
                  errorMessage={errorMessage}
                  onChange={(event) => field.handleChange(Number(event.target.value))}
                  onBlur={field.handleBlur}
                >
                  <SelectOption value="choose">Choose organisation</SelectOption>
                  {organisations.map((organisation) => (
                    <SelectOption key={organisation.organisationName} value={organisation.id}>
                      {organisation.organisationName}
                    </SelectOption>
                  ))}
                </Select>
                <p>
                  If your organisation is not registered, you must{' '}
                  <a href="URL">register your organisation (opens in a new tab)</a> before you can
                  set up your account.
                </p>
              </>
            )
          }}
        </form.Field>

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
                className={styles.marginBottom}
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
                hint="We'll use this email address to contact you about your request. You must use an email address from your organisation."
                onChange={(event: ChangeEvent<HTMLInputElement>) =>
                  field.handleChange(event.target.value)
                }
                onBlur={field.handleBlur}
                type="email"
                value={field.state.value}
                width="one-third"
                className={styles.marginBottom}
              />
            )
          }}
        </form.Field>
        <form.Field name="phoneNumber">
          {(field) => {
            const errorMessage = getFieldErrorMessage(field.state.meta.errors)

            return (
              <Input
                autoComplete="phone number"
                label="Phone number"
                name={field.name}
                onBlur={field.handleBlur}
                type="tel"
                onChange={(event: ChangeEvent<HTMLInputElement>) =>
                  field.handleChange(event.target.value)
                }
                error={Boolean(errorMessage)}
                errorMessage={errorMessage}
                hint="For international numbers include the country code."
                width="one-third"
                required
                value={field.state.value}
                className={styles.marginBottom}
              />
            )
          }}
        </form.Field>
        <Button type="submit" variant="cta">
          Submit request
        </Button>
      </form>
    </>
  )
}
