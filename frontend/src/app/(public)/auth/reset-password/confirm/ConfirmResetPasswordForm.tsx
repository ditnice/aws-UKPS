'use client'

import { initialFormState, mergeForm, useForm, useTransform } from '@tanstack/react-form-nextjs'
import { ChangeEvent, useActionState } from 'react'

import { Button } from '@/components/Button/Button'
import { Input } from '@/components/Input/Input'
import { PasswordInput } from '@/components/PasswordInput/PasswordInput'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { getFieldErrorMessage } from '@/lib/form/getFieldErrorMessage'

import { confirmResetPasswordAction } from './confirmResetPasswordAction'
import { confirmPasswordResetFormOptions } from './confirmResetPasswordShared'

type ConfirmResetPasswordFormProps = {
  email: string
}
const ConfirmResetPasswordForm = ({ email }: ConfirmResetPasswordFormProps) => {
  const [state, action] = useActionState(confirmResetPasswordAction, initialFormState)
  const form = useForm({
    ...confirmPasswordResetFormOptions,
    defaultValues: {
      ...confirmPasswordResetFormOptions.defaultValues,
      email,
    },
    transform: useTransform(
      (baseForm) =>
        mergeForm(baseForm, {
          ...state!,
          values: {
            ...confirmPasswordResetFormOptions.defaultValues,
            ...state?.values,
            email,
          },
        }),
      [state],
    ),
  })

  return (
    <form action={action as never} onSubmit={() => form.handleSubmit()}>
      {state.errors.length != 0 ? (
        <ErrorState>
          <div>
            {state.errors.map((e) => (
              <div key={e}>{e}</div>
            ))}
          </div>
        </ErrorState>
      ) : (
        <></>
      )}
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
      <Button type="submit" variant="cta" disabled={form.state.isSubmitting}>
        Send link
      </Button>
    </form>
  )
}

export default ConfirmResetPasswordForm
