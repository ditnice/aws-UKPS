'use client'

import { initialFormState, mergeForm, useForm, useTransform } from '@tanstack/react-form-nextjs'
import { ChangeEvent, startTransition, useActionState, useEffect } from 'react'

import { Button } from '@/components/Button/Button'
import { Input } from '@/components/Input/Input'
import { PasswordInput } from '@/components/PasswordInput/PasswordInput'
import FormLevelErrors from '@/lib/form/FormLevelErrors'
import { getFieldErrorMessage } from '@/lib/form/getFieldErrorMessage'

import { confirmResetPasswordAction } from './confirmResetPasswordAction'
import { confirmPasswordResetFormOptions } from './confirmResetPasswordShared'

type ServerErrorState = { errorMap?: { onServer?: { fields?: Record<string, unknown> } } }

const shownFields = ['confirmationCode', 'newPassword'] as const

type ConfirmResetPasswordFormProps = {
  email: string
}
const ConfirmResetPasswordForm = ({ email }: ConfirmResetPasswordFormProps) => {
  const [state, action, isPending] = useActionState(confirmResetPasswordAction, initialFormState)
  const form = useForm({
    ...confirmPasswordResetFormOptions,
    defaultValues: {
      ...confirmPasswordResetFormOptions.defaultValues,
      email,
    },
    // Only the values: server errors go on each field below, so each clears when its own field
    // changes rather than all clearing together.
    transform: useTransform(
      (baseForm) =>
        mergeForm(baseForm, {
          values: {
            ...confirmPasswordResetFormOptions.defaultValues,
            ...state?.values,
            email,
          },
        }),
      [state],
    ),
    onSubmitMeta: {} as { formData: FormData },
    onSubmit: ({ meta }) => {
      startTransition(() => {
        action(meta.formData)
      })
    },
  })

  useEffect(() => {
    const fields = (state as ServerErrorState).errorMap?.onServer?.fields
    shownFields.forEach((name) =>
      form.setFieldMeta(name, (meta) => ({
        ...meta,
        errorMap: { ...meta.errorMap, onSubmit: fields?.[name] },
      })),
    )
  }, [state, form])

  return (
    <form
      action={action as never}
      noValidate
      onSubmit={(e) => {
        e.preventDefault()
        void form.handleSubmit({ formData: new FormData(e.currentTarget) })
      }}
    >
      <FormLevelErrors errors={state.errors} />
      <form.Field name="email">
        {(field) => <input type="hidden" name={field.name} value={field.state.value} />}
      </form.Field>
      <form.Field name="confirmationCode">
        {(field) => {
          const errorMessage = getFieldErrorMessage(field.state.meta.errors)
          return (
            <Input
              inputMode="numeric"
              autoComplete="one-time-code"
              error={Boolean(errorMessage)}
              errorMessage={errorMessage}
              label="Confirmation code"
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
      <form.Field name="newPassword">
        {(field) => {
          const errorMessage = getFieldErrorMessage(field.state.meta.errors)

          return (
            <PasswordInput
              error={Boolean(errorMessage)}
              errorMessage={errorMessage}
              label="New Password"
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
      <Button type="submit" variant="cta" disabled={isPending}>
        Reset password
      </Button>
    </form>
  )
}

export default ConfirmResetPasswordForm
