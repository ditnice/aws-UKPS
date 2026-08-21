'use client'

import { revalidateLogic, useForm } from '@tanstack/react-form'
import { useRouter } from 'next/navigation'
import { ChangeEvent, useEffect, useState } from 'react'
import { z } from 'zod'

import { Button } from '@nice-digital/nds-button'

import { postUsersRegister } from '@/client/generated'
import { getOrganisationsOrganisationNames } from '@/client/generated'
import { getFieldErrorMessage } from '@/components/Form/getFieldErrorMessage'
import { Input } from '@/components/Input/Input'
import { Select, SelectOption } from '@/components/Select/Select'

import styles from './RegistrationRequestForm.module.scss'

const RegistrationRequest = z.object({
  organisation: z.string(),
  fullName: z.string().trim().min(1, 'Enter your full name'),
  workEmail: z
    .string()
    .trim()
    .min(1, 'Enter your work email address')
    .pipe(z.email('Enter an email address in the correct format, like name@example.com')),
  phoneNumber: z.string().trim().min(1, 'Enter your phone number'),
})

type RegistrationRequestValues = z.input<typeof RegistrationRequest>

export function RegistrationRequestForm() {
  const [organisations, setOrganisations] = useState<string[]>([])
  useEffect(() => {
    const fetchOrganisations = async () => {
      const response = await getOrganisationsOrganisationNames()
      if (response.data) {
        setOrganisations(response.data)
      }
    }
    fetchOrganisations()
  }, [])
  const router = useRouter()
  const form = useForm({
    defaultValues: {
      organisation: '',
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

      await postUsersRegister({
        body: {
          fullName: value.fullName,
          workEmail: value.workEmail,
          phoneNumber: value.phoneNumber,
          organisation: value.organisation,
        },
      })
      router.push('/portal/register/request-submitted')
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
      <Select
        defaultValue="choose"
        label="Select the organisation you are requesting access for"
        name="select-organisation-hint"
        width="one-third"
      >
        <SelectOption value="choose">Choose organisation</SelectOption>
        {organisations.map((organisation) => (
          <SelectOption key={organisation} value={organisation}>
            {organisation}
          </SelectOption>
        ))}
      </Select>
      <p>
        If your organisation is not registered, you must{' '}
        <a href="URL">register your organisation (opens in a new tab)</a> before you can set up your
        account.
      </p>
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
              error={Boolean(errorMessage)}
              errorMessage={errorMessage}
              label="Phone number"
              name={field.name}
              onBlur={field.handleBlur}
              onChange={(event: ChangeEvent<HTMLInputElement>) =>
                field.handleChange(event.target.value)
              }
              hint="For international numbers include the country code."
              width="one-third"
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
  )
}
