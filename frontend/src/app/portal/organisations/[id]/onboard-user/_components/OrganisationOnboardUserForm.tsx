'use client'

import { revalidateLogic, useForm } from '@tanstack/react-form'
import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { useState } from 'react'
import { z } from 'zod'

import { postUsersOnboard } from '@/client/generated/sdk.gen'
import { Button } from '@/components/Button/Button'
import { getFieldErrorMessage } from '@/components/Form/getFieldErrorMessage'
import { Input } from '@/components/Input/Input'

import styles from './OrganisationOnboardUserForm.module.scss'

import type { ChangeEvent } from 'react'

const onboardUserSchema = z.object({
  fullName: z.string().trim().min(1, "Enter the user's full name"),
  newUserEmail: z
    .string()
    .trim()
    .min(1, "Enter the user's work email address")
    .pipe(z.email('Enter an email address in the correct format, like name@example.com')),
  contactNumber: z.string().trim().min(1, "Enter the user's phone number"),
})

type OnboardUserFormValues = z.input<typeof onboardUserSchema>

const genericInviteError = 'There was a problem sending the invite. Please try again later.'
const forbiddenInviteError = 'You do not have permission to invite users to this organisation.'
const invalidInviteError = 'The invite details are invalid. Check the information and try again.'
const usernameConflictError = 'A user with this email address already exists.'

interface OrganisationOnboardUserFormProps {
  organisationId: number
}

export function OrganisationOnboardUserForm({ organisationId }: OrganisationOnboardUserFormProps) {
  const router = useRouter()
  const [formError, setFormError] = useState<string | null>(null)
  const [emailApiError, setEmailApiError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const cancelHref = `/portal/organisations/${organisationId}`

  const form = useForm({
    defaultValues: {
      fullName: '',
      newUserEmail: '',
      contactNumber: '',
    } satisfies OnboardUserFormValues,
    validationLogic: revalidateLogic({
      mode: 'submit',
      modeAfterSubmission: 'blur',
    }),
    validators: {
      onDynamic: onboardUserSchema,
    },
    onSubmit: async ({ value }) => {
      setFormError(null)
      setEmailApiError(null)
      setIsSubmitting(true)

      const parsedValue = onboardUserSchema.parse(value)
      const response = await postUsersOnboard({
        body: {
          ...parsedValue,
          organisationId,
        },
        credentials: 'include',
      })

      if (response.error) {
        setIsSubmitting(false)

        switch (response.response?.status) {
          case 400:
            setFormError(invalidInviteError)
            return
          case 403:
            setFormError(forbiddenInviteError)
            return
          case 409:
            setEmailApiError(usernameConflictError)
            return
          default:
            setFormError(genericInviteError)
            return
        }
      }

      // TODO: Look to use a short-lived flash cookie or server-side action state so the email is not visible in the URL
      router.push(`${cancelHref}?invited=${encodeURIComponent(parsedValue.newUserEmail)}`)
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
      <p>
        New users will be assigned the standard user role by default. You can change the permissions
        later using user management.
      </p>

      {formError ? (
        <p className={styles.error} role="alert">
          {formError}
        </p>
      ) : null}

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
              value={field.state.value}
              width="one-third"
            />
          )
        }}
      </form.Field>

      <form.Field name="newUserEmail">
        {(field) => {
          const errorMessage =
            getFieldErrorMessage(field.state.meta.errors) ?? emailApiError ?? undefined

          return (
            <Input
              autoComplete="email"
              error={Boolean(errorMessage)}
              errorMessage={errorMessage}
              label="Work email address"
              name={field.name}
              onBlur={field.handleBlur}
              onChange={(event: ChangeEvent<HTMLInputElement>) => {
                setEmailApiError(null)
                field.handleChange(event.target.value)
              }}
              type="email"
              value={field.state.value}
              width="one-third"
            />
          )
        }}
      </form.Field>

      <form.Field name="contactNumber">
        {(field) => {
          const errorMessage = getFieldErrorMessage(field.state.meta.errors)

          return (
            <Input
              autoComplete="tel"
              error={Boolean(errorMessage)}
              errorMessage={errorMessage}
              hint="For international numbers include the country code."
              label="Phone number"
              name={field.name}
              onBlur={field.handleBlur}
              onChange={(event: ChangeEvent<HTMLInputElement>) =>
                field.handleChange(event.target.value)
              }
              type="tel"
              value={field.state.value}
              width="one-third"
            />
          )
        }}
      </form.Field>

      <div className={styles.actions}>
        <Button disabled={isSubmitting} type="submit" variant="cta">
          Send invite
        </Button>
        <Button elementType={Link} href={cancelHref} variant="secondary">
          Cancel
        </Button>
      </div>
    </form>
  )
}
