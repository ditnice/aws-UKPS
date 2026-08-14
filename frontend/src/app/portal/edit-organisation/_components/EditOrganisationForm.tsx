'use client'
import { revalidateLogic, useForm } from '@tanstack/react-form'
import { ChangeEvent } from 'react'
import { z } from 'zod'

import { Button } from '@nice-digital/nds-button'
import { Textarea } from '@nice-digital/nds-textarea'

import { getFieldErrorMessage } from '@/components/Form/getFieldErrorMessage'
import { Input } from '@/components/Input/Input'

import styles from './EditOrganisationForm.module.scss'

const EditOrganisation = z.object({
  companyName: z.string().trim().min(1, 'Enter your company name'),
  companyAddress: z.string().trim().min(1, 'Enter your company address'),
  headOfficeEmail: z
    .string()
    .trim()
    .min(1, 'Enter your head office email address')
    .pipe(z.email('Enter an email address in the correct format, like name@example.com')),
  headOfficePhoneNumber: z.string().trim().min(1, 'Enter your head office phone number'),
})

type EditOrganisationValues = z.input<typeof EditOrganisationForm>

export function EditOrganisationForm() {
  const form = useForm({
    defaultValues: {
      companyName: 'Julie Brooks', // These default values will be from their existing account
      companyAddress: '1 Manchester Road, Manchester, M36 5RF',
      headOfficeEmail: 'admin@bigpharma1.com',
      headOfficePhoneNumber: '01234567890',
    } satisfies EditOrganisationValues,
    validationLogic: revalidateLogic({
      mode: 'submit',
      modeAfterSubmission: 'blur',
    }),
    validators: {
      onDynamic: EditOrganisationForm,
    },
    // onSubmit: ({ value }) => {
    //     EditOrganisationForm.parse(value)
    // },
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
      <div className={styles.marginBottom}>
        <form.Field name="companyName">
          {(field) => {
            const errorMessage = getFieldErrorMessage(field.state.meta.errors)
            return (
              <Input
                error={Boolean(errorMessage)}
                errorMessage={errorMessage}
                label="Company name"
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
      </div>
      <div className={styles.marginBottom}>
        <form.Field name="companyAddress">
          {(field) => {
            const errorMessage = getFieldErrorMessage(field.state.meta.errors)
            return (
              <Textarea
                error={Boolean(errorMessage)}
                errorMessage={errorMessage}
                label="Enter your company address"
                name={field.name}
                onBlur={field.handleBlur}
                onChange={(event: ChangeEvent<HTMLTextAreaElement>) =>
                  field.handleChange(event.target.value)
                }
                value={field.state.value}
                width="one-third"
              ></Textarea>
            )
          }}
        </form.Field>
      </div>
      <div className={styles.marginBottom}>
        <form.Field name="headOfficeEmail">
          {(field) => {
            const errorMessage = getFieldErrorMessage(field.state.meta.errors)
            return (
              <Input
                autoComplete="email"
                error={Boolean(errorMessage)}
                errorMessage={errorMessage}
                label="Enter the head office email address"
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
      </div>
      <div className={styles.marginBottom}>
        <form.Field name="headOfficePhoneNumber">
          {(field) => {
            const errorMessage = getFieldErrorMessage(field.state.meta.errors)

            return (
              <Input
                autoComplete="phone number"
                error={Boolean(errorMessage)}
                errorMessage={errorMessage}
                label="Enter the head office phone number"
                name={field.name}
                onBlur={field.handleBlur}
                onChange={(event: ChangeEvent<HTMLInputElement>) =>
                  field.handleChange(event.target.value)
                }
                width="one-third"
                value={field.state.value}
                className={styles.marginBottom}
              />
            )
          }}
        </form.Field>
      </div>
      <div className={styles.buttonGap}>
        <Button type="submit" variant="cta">
          Save
        </Button>
        <Button variant="secondary"> Cancel</Button>
      </div>
    </form>
  )
}
