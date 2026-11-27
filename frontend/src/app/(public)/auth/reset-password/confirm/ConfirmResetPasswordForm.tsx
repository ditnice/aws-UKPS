'use client'

import { initialFormState, mergeForm, useForm, useTransform } from '@tanstack/react-form-nextjs'
import { ChangeEvent, startTransition, useActionState } from 'react'

import { Button } from '@/components/Button/Button'
import { Input } from '@/components/Input/Input'
import { PasswordInput } from '@/components/PasswordInput/PasswordInput'
import FormLevelErrors from '@/lib/form/FormLevelErrors'
import { getFieldErrorMessage } from '@/lib/form/getFieldErrorMessage'

import { confirmResetPasswordAction } from './confirmResetPasswordAction'
import { confirmPasswordResetFormOptions } from './confirmResetPasswordShared'

type ServerErrorState = { errorMap?: { onServer?: { fields?: Record<string, unknown> } } }

const shownFields = ['confirmationCode', 'newPassword'] as const
type ShownField = (typeof shownFields)[number]

/**
 * The server's errors as each shown field's own `onServer` error, so each clears only when its own
 * field changes (see `clearServerError`) rather than all clearing together.
 */
const serverFieldMeta = (state: unknown) => {
  const fields = (state as ServerErrorState).errorMap?.onServer?.fields
  return Object.fromEntries(
    shownFields.map((name) => [name, { errorMap: { onServer: fields?.[name] } }]),
  )
}

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
    // Through the transform, so the errors also show when the page is rendered without JavaScript.
    transform: useTransform(
      (baseForm) =>
        mergeForm(baseForm, {
          values: {
            ...confirmPasswordResetFormOptions.defaultValues,
            ...state?.values,
            email,
          },
          fieldMetaBase: serverFieldMeta(state) as never,
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

  const clearServerError = (name: ShownField) =>
    form.setFieldMeta(name, (meta) => ({
      ...meta,
      errorMap: { ...meta.errorMap, onServer: undefined },
    }))

  return (
    <form
      action={action as never}
      noValidate
      onSubmit={(e) => {
        e.preventDefault()
        // The server checks again, so an earlier server error must not stop the submission.
        shownFields.forEach(clearServerError)
        void form.handleSubmit({ formData: new FormData(e.currentTarget) })
      }}
    >
      <FormLevelErrors errors={state.errors} />
      <form.Field name="email">
        {(field) => <input type="hidden" name={field.name} value={field.state.value} />}
      </form.Field>
      <form.Field
        name="confirmationCode"
        listeners={{ onChange: () => clearServerError('confirmationCode') }}
      >
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
      <form.Field
        name="newPassword"
        listeners={{ onChange: () => clearServerError('newPassword') }}
      >
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
