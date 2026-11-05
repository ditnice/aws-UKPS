'use client'

import { initialFormState, mergeForm, useForm, useTransform } from '@tanstack/react-form-nextjs'
import { ChangeEvent, useActionState } from 'react'

import { Button } from '@/components/Button/Button'
import { Input } from '@/components/Input/Input'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { getFieldErrorMessage } from '@/lib/form/getFieldErrorMessage'

import { resetPasswordAction } from './resetPasswordAction'
import { passwordRestFormOptions } from './resetPasswordShared'

const ResetPasswordForm = () => {
  const [state, action] = useActionState(resetPasswordAction, initialFormState)
  const form = useForm({
    ...passwordRestFormOptions,
    transform: useTransform(
      (baseForm) =>
        mergeForm(baseForm, {
          ...state!,
          values: { ...passwordRestFormOptions.defaultValues, ...state?.values },
        }),
      [state],
    ),
  })
  return (
    <form action={action as never} onSubmit={() => form.handleSubmit()}>
      {state.errors?.length != 0 ? (
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
        {(field) => {
          const errorMessage = getFieldErrorMessage(field.state.meta.errors)

          return (
            <Input
              autoComplete="email"
              error={Boolean(errorMessage)}
              errorMessage={errorMessage}
              label="Email address"
              name={field.name}
              onBlur={field.handleBlur}
              onChange={(event: ChangeEvent<HTMLInputElement>) =>
                field.handleChange(event.target.value)
              }
              type="email"
              value={field.state.value}
              width="full"
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

export default ResetPasswordForm
