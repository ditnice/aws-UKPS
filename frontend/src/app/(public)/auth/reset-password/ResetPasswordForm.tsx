'use client'

import { initialFormState, mergeForm, useForm, useTransform } from '@tanstack/react-form-nextjs'
import { ChangeEvent, startTransition, useActionState, useEffect, useRef } from 'react'

import { Button } from '@/components/Button/Button'
import { Input } from '@/components/Input/Input'
import FormLevelErrors from '@/lib/form/FormLevelErrors'
import { getFieldErrorMessage } from '@/lib/form/getFieldErrorMessage'

import { resetPasswordAction } from './resetPasswordAction'
import { passwordRestFormOptions } from './resetPasswordShared'

type ResetPasswordFormProps = {
  email?: string
  buttonContentOverride?: string
  onSuccess?: () => void
}

const ResetPasswordForm = ({ email, buttonContentOverride, onSuccess }: ResetPasswordFormProps) => {
  const boundAction = resetPasswordAction.bind(null, !email)
  const [state, action, isPending] = useActionState(boundAction, initialFormState)

  // Each action response is a new state object, so tracking the last handled one
  // fires onSuccess exactly once per successful submission (even if the effect re-runs).
  const lastHandledState = useRef<typeof state | null>(null)

  useEffect(() => {
    if ('success' in state && lastHandledState.current !== state) {
      lastHandledState.current = state
      onSuccess?.()
    }
  }, [state, onSuccess])

  const form = useForm({
    ...passwordRestFormOptions,
    defaultValues: {
      ...passwordRestFormOptions.defaultValues,
      ...(email ? { email } : {}),
    },
    transform: useTransform(
      (baseForm) =>
        mergeForm(baseForm, {
          ...state!,
          values: {
            ...passwordRestFormOptions.defaultValues,
            ...state?.values,
            ...(email ? { email } : {}),
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

  return (
    <form
      action={action as never}
      onSubmit={(e) => {
        e.preventDefault()
        void form.handleSubmit({ formData: new FormData(e.currentTarget) })
      }}
    >
      <FormLevelErrors errors={state.errors} />
      <form.Field name="email">
        {(field) => {
          if (email) {
            return <input type="hidden" name={field.name} value={email} />
          }

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
      <Button type="submit" variant="cta" disabled={isPending}>
        {buttonContentOverride ?? <>Send link</>}
      </Button>
    </form>
  )
}

export default ResetPasswordForm
