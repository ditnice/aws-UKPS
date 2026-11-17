'use client'

import { initialFormState, mergeForm, useForm, useTransform } from '@tanstack/react-form-nextjs'
import { ChangeEvent, startTransition, useActionState } from 'react'

import { EditableUserDetail } from '@/client/generated'
import { Button } from '@/components/Button/Button'
import { Input } from '@/components/Input/Input'
import FormLevelErrors from '@/lib/form/FormLevelErrors'
import { getFieldErrorMessage } from '@/lib/form/getFieldErrorMessage'

import { updateUserDetailsAction } from './serverActions'
import { updateUserDetailOptions } from './shared'

type EditUserDetailFormProps = {
  userId: number
  initialValue: string
  property: EditableUserDetail
  label: string
}
const EditUserDetailForm = ({ initialValue, userId, property, label }: EditUserDetailFormProps) => {
  const boundAction = updateUserDetailsAction.bind(null, userId)
  const [state, action, isPending] = useActionState(boundAction, initialFormState)
  const defaultValues = {
    ...updateUserDetailOptions.defaultValues,
    value: initialValue,
    property,
  }
  const form = useForm({
    ...updateUserDetailOptions,
    defaultValues,
    transform: useTransform(
      (baseForm) => {
        return mergeForm(baseForm, {
          ...state!,
          values: {
            ...defaultValues,
            ...state?.values,
            property,
          },
        })
      },
      [state, initialValue, property],
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
      <FormLevelErrors errors={state?.errors ?? []} />
      <form.Field name="property">
        {(field) => <input type="hidden" name={field.name} value={field.state.value} />}
      </form.Field>
      <form.Field name="value">
        {(field) => {
          const errorMessage = getFieldErrorMessage(field.state.meta.errors)
          return (
            <Input
              error={Boolean(errorMessage)}
              errorMessage={errorMessage}
              label={label}
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
      <Button type="submit" variant="cta" disabled={isPending}>
        Save
      </Button>
    </form>
  )
}

export default EditUserDetailForm
