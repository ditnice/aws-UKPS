'use client'
import { revalidateLogic, useForm } from '@tanstack/react-form'
import { useRouter } from 'next/navigation'
import { z } from 'zod'

import { Button, ButtonGroup } from '@/components/Button/Button'
import { getFieldErrorMessage } from '@/components/Form/getFieldErrorMessage'
import { Input } from '@/components/Input/Input'

import type { ChangeEvent } from 'react'

const EditDetails = z.object({
  fullName: z.string().trim().min(1, 'Enter your full name'),
  workEmail: z
    .string()
    .trim()
    .min(1, 'Enter your work email address')
    .pipe(z.email('Enter an email address in the correct format, like name@example.com')),
  phoneNumber: z.string().trim().min(1, 'Enter your phone number'),
})

type EditDetailsValues = z.input<typeof EditDetails>

export function EditDetailsForm() {
  const router = useRouter()
  const form = useForm({
    defaultValues: {
      fullName: 'Julie Brooks', // These default values will be from their existing account
      workEmail: 'admin@bigpharma1.com',
      phoneNumber: '01234567890',
    } satisfies EditDetailsValues,
    validationLogic: revalidateLogic({
      mode: 'submit',
      modeAfterSubmission: 'blur',
    }),
    validators: {
      onDynamic: EditDetails,
    },
    onSubmit: ({ value }) => {
      EditDetails.parse(value)
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
      <form.Field name="phoneNumber">
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
