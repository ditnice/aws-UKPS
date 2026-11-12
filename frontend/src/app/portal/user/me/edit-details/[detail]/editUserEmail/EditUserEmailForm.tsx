'use client'

import { initialFormState, mergeForm, useForm, useTransform } from '@tanstack/react-form-nextjs'
import { ChangeEvent, useActionState } from 'react'

import { Button } from '@/components/Button/Button'
import { Input } from '@/components/Input/Input'
import { getFieldErrorMessage } from '@/lib/form/getFieldErrorMessage'

import { updateUserEmailAction } from './serverActions'
import { updateUserEmailOptions } from './shared'

type EditUserDetailFormProps = {
  userId: number
  initialValue: string
}
const EditUserDetailForm = ({ initialValue, userId }: EditUserDetailFormProps) => {
  const boundAction = updateUserEmailAction.bind(null, userId)
  const [state, action] = useActionState(boundAction, initialFormState)
  const defaultValues = {
    ...updateUserEmailOptions.defaultValues,
    value: initialValue,
  }
  const form = useForm({
    ...updateUserEmailOptions,
    defaultValues,
    transform: useTransform(
      (baseForm) =>
        mergeForm(baseForm, {
          ...state!,
          values: {
            ...defaultValues,
            ...state?.values,
          },
        }),
      [state, initialValue],
    ),
  })
  return (
    <form action={action as never} onSubmit={() => form.handleSubmit()}>
      <form.Field name="value">
        {(field) => {
          const errorMessage = getFieldErrorMessage(field.state.meta.errors)
          return (
            <Input
              autoComplete="email"
              error={Boolean(errorMessage)}
              errorMessage={errorMessage}
              label={'Email address'}
              name={field.name}
              onBlur={field.handleBlur}
              onChange={(event: ChangeEvent<HTMLInputElement>) =>
                field.handleChange(event.target.value)
              }
              type="text"
              value={field.state.value}
              width="two-thirds"
            />
          )
        }}
      </form.Field>
      <Button type="submit" variant="cta" disabled={form.state.isSubmitting}>
        Submit
      </Button>
    </form>
  )
}

export default EditUserDetailForm
