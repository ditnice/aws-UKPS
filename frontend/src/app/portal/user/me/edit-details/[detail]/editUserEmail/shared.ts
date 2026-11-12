import { formOptions } from '@tanstack/react-form'
import { createServerValidate } from '@tanstack/react-form-nextjs'
import z from 'zod'

import { UpdateUserEmailCommand } from '@/client/generated'
import { errorMessages } from '@/lib/form/errorMessages'

export const formSchema = z.object({
  value: z
    .string()
    .trim()
    .min(1, errorMessages.workEmailRequired)
    .pipe(z.email(errorMessages.emailFormat)),
})

const defaultValues: UpdateUserEmailCommand = {
  value: '',
}

export const updateUserEmailOptions = formOptions({
  defaultValues: defaultValues,
  validators: {
    onDynamic: formSchema,
  },
})

export const serverValidate = createServerValidate({
  ...updateUserEmailOptions,
  onServerValidate: formSchema,
})
